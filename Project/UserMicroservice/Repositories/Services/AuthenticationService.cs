using AutoMapper;
using Azure;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.Repositories.Interfaces;
using UserMicroservice.DBContexts.Enum;
using System.Security.Claims;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;
using Newtonsoft.Json;

/*hiện tại bọn em đang làm chức năng trả lương cho nhân viên bằng ngân hàng, nhưng em chưa tìm được keyword để nghiên cứu. 
Cho em hỏi dịch vụ chi hộ của ngân hàng có phải dùng để trả lương không,

 */
namespace UserMicroservice.Repositories.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserService _userService;
        private readonly UserDbContext _context;
        private readonly IHelperService _helper;
        private readonly IMapper _mapper;
        public AuthenticationService(
            UserDbContext context,
            IUserService userService,
            IHelperService helper,
            IMapper mapper
            )
        {
            _context = context;
            _userService = userService;
            _helper = helper;
            _mapper = mapper;
            _httpContextAccessor = new HttpContextAccessor();
        }
        private readonly IHttpContextAccessor _httpContextAccessor;



        public ResponseModel GetUserInfoByClaim(IEnumerable<Claim> claims)
        {
            ResponseModel response = new();
            try
            {
                var loginGoogleRequestModel = new LoginGoogleRequestModel
                {
                    Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    Username = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    DisplayName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    EmailConfirmation = claims.ToString() == "true" ? EmailStatus.Confirmed : EmailStatus.Unconfirmed,
                    Picture = claims.FirstOrDefault(c => c.Type == "urn:google:picture")?.Value
                };
                response.Message = "Get user info by claim successful";
                response.Result = loginGoogleRequestModel;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public Task<ResponseModel> GoogleCallback(LoginGoogleRequestModel loginGoogleRequestModel)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel> LoginAsync(LoginRequestModel loginRequestModel)
        {
            loginRequestModel.UsernameOrEmail = loginRequestModel.UsernameOrEmail.ToUpper();
            var response = new ResponseModel();

            if (loginRequestModel == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "LoginRequestModel is null"
                };
            }

            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(x =>
                    x.NormalizedUsername == loginRequestModel.UsernameOrEmail ||
                    x.NormalizedEmail == loginRequestModel.UsernameOrEmail);

                if (user == null)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Username or password is incorrect"
                    };
                }


                if (user.Status == UserStatus.Inactive && user.EmailConfirmation == EmailStatus.Unconfirmed)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Account is not active, please check your email to active account"
                    };
                }


                if (user.Status != UserStatus.Active)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = $"Account is {user.Status.ToString()}, please contact to us."
                    };
                }


                // Xác minh mật khẩu
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(loginRequestModel.Password, user.PasswordHash);
                if (!isPasswordCorrect)
                {
                    return new ResponseModel
                    {
                        IsSuccess = false,
                        Message = "Username or password is incorrect"
                    };
                }

                // Tạo JWT Token
                UserModel userModel = _mapper.Map<UserModel>(user);
                userModel.IsRememberMe = loginRequestModel.IsRememberMe;
                string token = _helper.GenerateJwtAsync(userModel);

                // Chuẩn bị response thành công
                response.Result = new LoginResponseModel
                {
                    Token = token,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    Id = user.Id,
                    Email = user.Email,
                    Avatar = user.Avatar!,
                    Role = user.Role.ToString(),
                    LoginType = user.LoginType.ToString(),
                    IsRememberMe = userModel.IsRememberMe
                };
                response.IsSuccess = true;
                response.Message = "Login successful";
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }




        public async Task<ResponseModel> LoginGoogleAsync(LoginGoogleRequestModel loginGoogleRequestModel)
        {
            var response = new ResponseModel();

            if (loginGoogleRequestModel == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "LoginGoogleRequestModel is null"
                };
            }

            try
            {
                // Kiểm tra tài khoản có tồn tại hay không
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == loginGoogleRequestModel.Email);

                if (user != null)
                {
                    // Nếu user tồn tại nhưng chưa được active hoặc email chưa được xác nhận, xóa user
                    if (user.Status == UserStatus.Inactive || user.EmailConfirmation == EmailStatus.Unconfirmed)
                    {
                        await _userService.DeleteUser(user.Id);
                        user = null; // Đặt lại user để tạo mới
                    }
                }

                if (user == null)
                {
                    // Tạo tài khoản mới
                    var userCreateModel = new UserModel
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        Email = loginGoogleRequestModel.Email!,
                        Username = loginGoogleRequestModel.Email!,
                        Role = UserRole.User,
                        Avatar = loginGoogleRequestModel.Picture!,
                        DisplayName = loginGoogleRequestModel.DisplayName,
                        Status = UserStatus.Active,
                        EmailConfirmation = EmailStatus.Confirmed,
                        LoginType = UserLoginType.Google,
                        IsRememberMe = true
                    };

                    user = _mapper.Map<User>(userCreateModel);
                    await _context.AddAsync(user);
                    await _context.SaveChangesAsync();
                }

                // Tạo token và trả về response
                var userModel = _mapper.Map<UserModel>(user);
                string token = _helper.GenerateJwtAsync(userModel);

                response.Result = new LoginResponseModel
                {
                    Token = token,
                    Username = user.Username,
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    LoginType = user.LoginType.ToString(),
                    IsRememberMe = true
                };

                response.Message = "Login successful";
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ResponseModel> LoginWithoutPassword(string email)
        {
            var response = new ResponseModel();

            if (email == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "RegisterRequestModel is null"
                };
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
                UserModel userModel = _mapper.Map<UserModel>(user);
                string token = _helper.GenerateJwtAsync(userModel);
                response.Result = new LoginResponseModel
                {
                    Token = token,
                    Username = user!.Username,
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Avatar = user.Avatar,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    LoginType = UserLoginType.Local.ToString(),
                    IsRememberMe = false
                };
                response.Message = "Login successful";

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ResponseModel> RegisterAsync(RegisterRequestModel registerRequestModel)
        {
            var response = new ResponseModel();

            if (registerRequestModel == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "RegisterRequestModel is null"
                };
            }
            // Kiểm duyệt nội dung
            if (!IsContentAppropriate(registerRequestModel.Username))
            {
                response.IsSuccess = false;
                response.Message = "The Username is not for community";
            }

            try
            {

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerRequestModel.Email);
                if (user != null)
                {
                    if (user.Status != UserStatus.Inactive && user.EmailConfirmation != EmailStatus.Unconfirmed)
                    {
                        response.Message = "User is exist";
                        response.IsSuccess = false;
                        return response;
                    }
                    await _userService.DeleteUser(user.Id);

                    // Step 2: Check if username or email already exists
                    response = await _helper.IsUserNotExist(registerRequestModel.Username, registerRequestModel.Email);
                    if (!response.IsSuccess)
                    {
                        response.Message = "Failed to verify if user exists.";
                        return response;
                    }

                    CountModel count = (CountModel)response.Result;
                    if (count.NumUsername != 0)
                    {
                        response.IsSuccess = false;
                        response.Message = "Username already exists.";
                        return response;
                    }
                }
                UserModel userModel = _mapper.Map<UserModel>(registerRequestModel);
                userModel.Id = ObjectId.GenerateNewId().ToString();
                User userCreate = _mapper.Map<User>(userModel);
                userCreate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequestModel.Password);
                await _context.AddAsync(userCreate);
                await _context.SaveChangesAsync();

                //Gửi mail kích hoạt tài khoản
                await ActiveUserAsync(registerRequestModel.Email);

                response.Result = userModel;
                response.Message = "Register is success";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }


        public async Task<ResponseModel> ChangePassword(ChangePasswordModel model)
        {
            ResponseModel response = new();
            try
            {
                User? user = await _context.Users.FindAsync(model.Id);
                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = "User need change password is null";
                    return response;
                }

                if (user.LoginType != UserLoginType.Local)
                {
                    response.IsSuccess = false;
                    response.Message = $"User login by {user.LoginType.ToString()} don't need change password";
                    return response;
                }

                else
                {
                    bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(model.OldPassword, user.PasswordHash);
                    if (isPasswordCorrect)
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        _context.Users.Attach(user);     // Đính kèm vào context nhưng không theo dõi tất cả trường
                        _context.Entry(user).Property(u => u.PasswordHash).IsModified = true; // Chỉ cập nhật trường cần thiết
                        await _context.SaveChangesAsync();
                        response.Message = "Change password is success";
                        return response;
                    }
                    response.IsSuccess = false;
                    response.Message = "Password is not correct";

                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }


        #region Forgot and Reset Password
        public async Task<ResponseModel> ForgotPasswordAsync(string email)
        {
            ResponseModel response = new();
            try
            {

                // Kiểm tra xem email có tồn tại không
                ResponseModel findUser = await _userService.GetUserByEmail(email);
                if (findUser == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Email does not exist";
                    return response;
                }
                UserModel user = (UserModel)findUser.Result;
                response = await SendPasswordResetEmailAsync(user);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }


        private async Task<ResponseModel> SendPasswordResetEmailAsync(UserModel model)
        {
            ResponseModel response = new();
            try
            {
                // Tạo token đặt lại mật khẩu
                var token = _helper.GeneratePasswordResetToken(model);
                var gateway = "https://gatewayapi-fbb8b8hcdcdgcqfq.southeastasia-01.azurewebsites.net";
                // Tạo URL thủ công nếu không có ngữ cảnh HttpRequest
                string confirmationLink = $"{gateway}/User/ResetPassword?userId={model.Id}&token={token}";

                var emailSubject = "Reset your password";
                var emailBody = $"Click to link to reset password: <a href='{confirmationLink}'>Reset Password</a>";

                // Kiểm tra và gửi email
                if (model.Email != null)
                {
                    response = await _helper.SendEmailAsync(model.Email, emailSubject, emailBody);
                    response.Result = token;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Email is null";
                }

            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.IsSuccess = false;
            }
            return response;
        }


        public async Task<ResponseModel> ResetPassword(ResetPasswordModel model)
        {
            ResponseModel response = new();
            try
            {
                if (model.Token.IsNullOrEmpty())
                {
                    response.IsSuccess = false;
                    response.Message = "Token is null";
                    return response;
                }
                ResponseModel jsonToken = _helper.DecodeToken(model.Token);
                User user = _helper.GetUserIdFromToken((JwtSecurityToken)jsonToken.Result);
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                _context.Users.Attach(user);     // Đính kèm vào context nhưng không theo dõi tất cả trường
                _context.Entry(user).Property(u => u.PasswordHash).IsModified = true; // Chỉ cập nhật trường cần thiết
                await _context.SaveChangesAsync();

                response.Message = "Reset password is success";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }

            return response;
        }

        #endregion

        #region Active User

        public async Task<ResponseModel> ActiveUserAsync(string email)
        {
            ResponseModel response = new();
            try
            {

                // Kiểm tra xem email có tồn tại không
                ResponseModel findUser = await _userService.GetUserByEmail(email);
                if (findUser == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Email does not exist";
                    return response;
                }
                UserModel user = (UserModel)findUser.Result;

                response = await SendActiveUserEmailAsync(user);
                return response;


            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }


        private async Task<ResponseModel> SendActiveUserEmailAsync(UserModel model)
        {
            ResponseModel response = new();
            try
            {
                // Tạo token đặt lại mật khẩu
                //gateway host
                var gateway = "https://gatewayapi-fbb8b8hcdcdgcqfq.southeastasia-01.azurewebsites.net";

                //gateway local
                //var gateway = "https://localhost:7296";


                // Tạo URL thủ công nếu không có ngữ cảnh HttpRequest
                string confirmationLink = $"{gateway}/User/ActiveUser?userId={model.Id}";

                var emailSubject = "Active your account";
                var emailBody = $"Click to link to active account: <a href='{confirmationLink}'>Active account</a>";

                // Kiểm tra và gửi email
                if (model.Email != null)
                {
                    response = await _helper.SendEmailAsync(model.Email, emailSubject, emailBody);
                    //response.Result = token;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Email is null";
                }

            }
            catch (Exception ex)
            {
                response.Message = $"Error: {ex.Message}";
                response.IsSuccess = false;
            }
            return response;
        }


        #endregion

        #region method
        private readonly List<string> _bannedWords = new List<string>
        {
            // Tiếng Việt - Nội dung không phù hợp và các biến thể lách luật
            "chết tiệt", "chếttttt", "đồ ngu", "ngu ngốc", "nguuu", "khốn nạn", "khónnnn", "đĩ", "đĩiiiiii", "mẹ mày", "mẹeeeee",
            "cút đi", "cúttttt", "xấu xa", "xấuuuu", "vô học", "vôooo", "biến đi", "biếnnnn", "đồ chó", "chóooo", "khốn kiếp",
            "kiếppppp", "bố láo", "láooooo", "cái l*n", "vãi l*n", "mẹ kiếp", "đ**", "d*m", "chửi thề", "đồ phản bội", "phản boiiii",
            "phá hoại", "hoạiiii", "phản động", "phảnnn", "vô dụng", "dụnggggg", "mất dạy", "dạyyyy", "cặn bã", "cặnnnnn",
            "thối nát", "náttttt", "biến thái", "biếnnnn", "vô liêm sỉ", "hèn hạ", "ngốc nghếch", "ngu si", "thô tục", "tụcccc",
            "đầu bò", "đần độn", "dốtttt", "thần kinh", "đầu gấu", "đồ ngớ ngẩn", "tởm lợm", "chửi tục", "cút xéo", "ngớ ngẩn",
            "tởm", "ăn hại", "ăn cắp", "vô lại", "đê tiện", "xấu xí", "mất nết", "vô tích sự", "vô giáo dục", "thô lỗ", "phản bội",
            "đụ", "đụ má", "đụ mẹ", "đụmmmm", "dụ nhau", "cu", "cu to", "cặc", "cặcccc", "c*k", "cc", "chịch", "chịch nhau", "chịcc",
            "nứng", "nứng nà", "nứng ơi", "dâm đãng", "d*m đãng", "d*m dục", "địt", "địtttt", "đjt", "dm", "dm bạn", "ch*ch","lồn",
            "lồnnn", "vãi lồn", "cái lồn", "l*n", "loz", "lozzzz", "l*nz", "l**z", "lol", "lồllll", "cặc", "cặkkkk", "cạkkkk", "cặk",
            "cặt", "cậtttt", "c*c", "c*k", "cc", "cặccc", "cắcccc", "cặccccc", "cứt", "cức", "cứtttt", "cứcccc", "đổ cứt", "đổ cức",
            "óc chó", "óc ch*o", "óc chooo", "óc c*o", "chó óc", "c*n l*n", "l*l", "đồ loz", "thằng lol", "đồ lol", "ngu lol", "đồ c*o",
            "ăn lol", "đồ l*n", "loằnn", "ngu loz", "vô loz", "ph* loz", "lốnnnn",


            // Tiếng Anh - Offensive and inappropriate words and variations
            "idiot", "idiotttttt", "stupid", "stupidddd", "moron", "moroNnnn", "jerk", "jerkkkk", "dumb", "dumBBB", "loser",
            "looooser", "shut up", "shuttupppp", "trash", "trasshhhhh", "fool", "fooool", "freak", "freeeak", "pervert", "pErverTttt",
            "slut", "sluTtt", "bitch", "biatch", "whore", "wh0r3", "pussy", "pussssyyy", "douche", "douchee", "retard", "retaaard",
            "scumbag", "sCumbagg", "twat", "twatttt", "dirtbag", "dirtttt", "degenerate", "dEgeneRate", "hypocrite", "hypooo",
            "worthless", "worTHLessss", "scum", "SCumm", "dumbbell", "dUmBBBell", "retard", "retaRRRd", "slob", "sloBBb",
            "creep", "creeeeeep", "loser", "looooser", "psychopath", "PSYchooooo", "trash", "trasshhhhh", "cringe", "cRingEee",

            // Thêm từ chửi thề phổ biến bằng tiếng Anh
            "fuck", "f*ck", "f***", "f**k", "fuuuck", "fuuuccckkk", "fking", "fkng", "fck", "sh*t", "shiiiit", "shyt", "shttt",
            "bullshit", "b*llshit", "bsh*t", "motherfucker", "m*therf*cker", "mf", "mfker", "screw you", "scrw you", "scrwy",
            "asshole", "assh*le", "a$$hole", "a$$", "arsehole", "dickhead", "d*ckhead", "dikhed", "d*uche", "douchebag", "doucheb*g",
            "wtf", "wtfff", "wtfuu", "damn", "d*mn", "d4mn", "hell", "he11", "helll", "h3ll", "jackass", "j4ckass", "jerk", "biatch",
            "b*tch", "b1tch", "beyotch", "beeotch", "idiot", "dumbass", "dumbf*ck", "dummkopf",
    
            // Tiếng Tây Ban Nha - Palabras ofensivas y inapropiadas con variaciones
            "idiota", "idiotaaaa", "estúpido", "estuPIDoooo", "imbécil", "imbecilLLLLL", "basura", "baSURaaa", "mierda", "mierdddddaaa",
            "puta", "putaaaaaa", "carajo", "cArAjoOOO", "tonto", "toooNTOO", "pervertido", "pervEERTido", "sucio", "SUcioooo", "malnacido",
            "malnACido", "pendejo", "pendEjjjjO", "miserable", "MIsERableeeee", "patán", "patAnNNN", "idiotez", "idioteZZZZ", "vago",
            "vagooo", "asno", "ASnooooo", "estúpida", "estuPIDa", "imbeciloide", "imbeciLoIDe", "cabeza hueca", "caBeZA hUeCA",
            "idiotez", "idioTeZZ", "inepto", "inePTTTo", "hipócrita", "HIPoCritaA", "maleducado", "malEDUcaDo", "sucia", "suCiaaaa",
    
            // Tiếng Pháp - Mots offensants et inappropriés et leurs variations
            "idiot", "IDIOTTTT", "imbécile", "IMBECiLeee", "connard", "CONNardDD", "salaud", "SAlAUDddd", "ordure", "OrDureEEE",
            "merde", "MERRRRDDE", "putain", "PUTainNNN", "crétin", "CRETiiiiNNN", "débile", "dEBILEee", "abruti", "ABRUtiiiiii",
            "goujat", "goujATttt", "dégénéré", "DEGENEREEEEE", "salopard", "saloPARDddd", "connNNNN", "imbécile heureux",
            "IMBECILEEEEE", "branleur", "branleEEEur", "impoli", "IMPolIeee", "inculte", "INculteeeee", "imbu de lui-même",
            "imbUUU", "fainéant", "fAINNEEANTtt", "paresseux", "PAREsSEuxxx", "épais", "EPaaaiss", "salopard", "saloPARDD",
    
            // Tiếng Đức - Beleidigende Wörter mit Varianten
            "idiot", "IDIOTTtt", "dummkopf", "DUMMKOPFff", "arschloch", "ARSCHloCH", "schlampe", "SCHLAMpppeee", "verdammt",
            "verDAMMMT", "scheiße", "SCHHHHHeisse", "hure", "HUUURE", "miststück", "MiSTStuckkk", "dumm", "DUMMMMM",
            "blödmann", "BLÖDMANNN", "verrückt", "VERRUCKTTT", "krank", "KRRRANK", "ekelhaft", "EKELHAFtTT", "pervers",
            "pERverseee", "idiot", "IDIOTEEE", "krüppel", "KRUPPEL", "grobian", "GROOOOBIAN", "unmensch", "UNMENSCHHH",
    
            // Tiếng Nhật - 不適切な言葉とそのバリエーション
            "馬鹿", "バカaaaa", "くそ", "くそおおお", "アホ", "あほほほほ", "死ね", "しねえええ", "気持ち悪い", "気持ちわる",
            "最低", "さいてい", "嫌い", "きらい", "消えろ", "きえろお", "野郎", "やろううう", "バカたれ", "たれええ",
            "ゴミ", "ごみいいい", "アホンダラ", "あほんだら", "サイテー", "さいてええ", "うざい", "うざあああ", "やかましい",
            "やかまああ", "詐欺", "さぎいいい", "馬鹿げた", "ばかげたああ", "不愉快", "ふゆかいい", "くたばれ", "くたばれえ",
            "馬鹿馬鹿しい", "ばかばかしいいい"
        };

        private bool IsContentAppropriate(string content)
        {
            // Kiểm tra xem content có chứa từ bị cấm hay không
            foreach (var bannedWord in _bannedWords)
            {
                if (content.Contains(bannedWord, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Nội dung không phù hợp
                }
            }
            return true; // Nội dung phù hợp
        }
        #endregion

    }
}