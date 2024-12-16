using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using System.Text;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Models.AuthModel;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.Repositories.Interfaces;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Azure.Core;
using static System.Net.WebRequestMethods;
using System;
using Amazon.SecurityToken.Model;
using System.Security.Cryptography;

namespace UserMicroservice.Repositories.Services
{
    public class HelperService(UserDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : IHelperService
    {
        private readonly UserDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IConfiguration _configuration = configuration;

        public string GenerateJwtAsync(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtOptionModel.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("Avatar", user.Avatar),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("LoginType", user.LoginType.ToString())
            };
            if (user.LoginType != DBContexts.Enum.UserLoginType.Local)
                user.IsRememberMe = true;
            DateTime expiresTime = user.IsRememberMe?DateTime.Now.AddDays(7) : DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                issuer: JwtOptionModel.Issuer,
                audience: JwtOptionModel.Audience,
                claims,
                expires: expiresTime,
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ResponseModel CheckAndReadToken(string token)
        {
            // Khởi tạo đối tượng ResponseModel mặc định
            if (string.IsNullOrEmpty(token))
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Token is null"
                };
            }

            var handler = new JwtSecurityTokenHandler();

            // Kiểm tra nếu token không hợp lệ hoặc không thể đọc
            if (!handler.CanReadToken(token))
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Invalid token format"
                };
            }

            // Đọc token
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            if (jsonToken == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Unable to analyze token"
                };
            }

            // Trường hợp token hợp lệ
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Token is valid",
                Result = jsonToken
            };
        }

        public async Task<ResponseModel> IsUserNotExist(string username, string? email = null, string? phoneNumber = null)
        {
            var response = new ResponseModel();
            var count = new CountModel();

            try
            {
                // Kiểm tra số lượng username trùng khớp
                if (!string.IsNullOrEmpty(username))
                {
                    count.NumUsername = await _context.Users.CountAsync(u => u.Username == username);
                }

                // Kiểm tra số lượng email trùng khớp
                if (!string.IsNullOrEmpty(email))
                {
                    count.NumEmail = await _context.Users.CountAsync(u => u.Email == email);
                }

                // Kiểm tra số lượng phoneNumber trùng khớp
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    count.NumPhoneNumber = await _context.Users.CountAsync(u => u.PhoneNumber == phoneNumber);
                }

                // Xây dựng thông báo phản hồi dựa trên kết quả
                var messages = new List<string>();

                if (count.NumUsername > 0)
                {
                    messages.Add($"Username '{username}' already exists in the database.");
                }

                if (count.NumEmail > 0)
                {
                    messages.Add($"Email '{email}' already exists in the database.");
                }

                if (count.NumPhoneNumber > 0)
                {
                    messages.Add($"Phone number '{phoneNumber}' already exists in the database.");
                }

                // Thiết lập thông báo dựa trên kết quả của từng field
                if (messages.Any())
                {
                    response.Message = string.Join(" ", messages); // Ghép các thông báo lại
                    response.IsSuccess = true;
                }
                else
                {
                    response.Message = "Username, email, and phone number are all available.";
                    response.IsSuccess = true;
                }

                response.Result = count;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }


            return response;
        }


        public ResponseModel IsUserNotNull(AddUserModel user)
        {
            var response = new ResponseModel();
            response.Message = "User was not null";

            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User is null";
            }
            return response;
        }

        public ResponseModel IsUserNotNull(User user)
        {
            var response = new ResponseModel();
            response.Message = "User was not null";

            if (user == null)
            {
                response.IsSuccess = false;
                response.Message = "User is null";
            }
            return response;
        }

        public ResponseModel NomalizeQuery(string? query)
        {
            var response = new ResponseModel();
            if (string.IsNullOrEmpty(query))
            {
                response.IsSuccess = false;
                response.Message = "Query is null or empty";
            }
            else
            {
                response.Message = "Query is normalized";
                response.Result = query.ToUpper().Trim();
            }
            return response;
        }

        public async Task<ResponseModel> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            ResponseModel response = new();
            try
            {
                var emailstring = _configuration["21"];
                var SenderPassword = _configuration["22"];
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(emailstring, SenderPassword),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(emailstring),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                response.Message = "Email sent successfully";
                response.IsSuccess = true; 
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                response.IsSuccess = false;
            }
            return response;
        }

        public string GetAppBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request != null ? $"{request.Scheme}://{request.Host}" : "http://defaultUrl";
        }



        public string GeneratePasswordResetToken(UserModel model)
        {
            // Tạo token bảo mật và mã hóa
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0123456789_0123456789_0123456789"));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, model.Id.ToString()),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Name, model.Username),
                new Claim(ClaimTypes.GivenName, model.DisplayName),
                new Claim("Avatar", model.Avatar),
                new Claim("LoginType", model.LoginType.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: JwtOptionModel.Issuer,
                audience: JwtOptionModel.Audience,
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: signingCredentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);

        }


        public ResponseModel DecodeToken(string token)
        {
            // Khởi tạo đối tượng ResponseModel mặc định
            if (string.IsNullOrEmpty(token))
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Token is null"
                };
            }

            var handler = new JwtSecurityTokenHandler();

            // Kiểm tra nếu token không hợp lệ hoặc không thể đọc
            if (!handler.CanReadToken(token))
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Invalid token format"
                };
            }

            // Đọc token
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            if (jsonToken == null)
            {
                return new ResponseModel
                {
                    IsSuccess = false,
                    Message = "Unable to analyze token"
                };
            }

            // Trường hợp token hợp lệ
            return new ResponseModel
            {
                IsSuccess = true,
                Message = "Token is valid",
                Result = jsonToken
            };
        }

        public User GetUserIdFromToken(JwtSecurityToken token)
        {
            return new User
            {
                Id = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value,
                Email = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)?.Value,
                Username = token?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value
            };
        }

        public string GenerateRandomPassword(int length)
        {
            const string asciiChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] password = new char[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < length; i++)
                {
                    int randomIndex = randomBytes[i] % asciiChars.Length;
                    password[i] = asciiChars[randomIndex];
                }
            }

            return new string(password);
        }
        public bool IsContentAppropriate(string content)
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
            "goujat", "goujATttt", "dégénéré", "DEGENEREEEEE", "salopard", "saloPARDddd", "con", "connNNNN", "imbécile heureux",
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
    }
}
