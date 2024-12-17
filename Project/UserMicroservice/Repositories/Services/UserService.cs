using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.EntityFrameworkCore.Storage.ValueConversion;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Models.UserManagerModel;
using UserMicroservice.Repositories.Interfaces;
using UserMicroservice.DBContexts.Enum;
using static Google.Apis.Requests.BatchRequest;
using X.PagedList.Extensions;
using UserMicroservice.Models.Message;
using ZstdSharp.Unsafe;
using UserMicroservice.Models.AuthModel;
using RabbitMQ.Client;

namespace UserMicroservice.Repositories.Services
{
    public class UserService(UserDbContext context, IMapper mapper, IHelperService helper, IMessage message) : IUserService
    {
        private readonly UserDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IHelperService _helper = helper;
        private readonly IMessage _message = message;

        /// <summary>
        /// Adds a user to the database. If it's an admin, the system generates a password and skips password input. 
        /// If it's a user registration, the password is required.
        /// </summary>
        /// <param name="userModel">The user model containing user details</param>
        /// <param name="isAdmin">Flag to indicate whether it's an admin or user registration</param>
        /// <param name="password">Password input from the user or null if admin</param>
        /// <returns>A response indicating the success or failure of the operation.</returns>
        public async Task<ResponseModel> AddUserAsync(AddUserModel userInput)
        {
            ResponseModel response = new();
            try
            {
                // Step 0: Moderate description, name
                if (!_helper.IsContentAppropriate(userInput.Username))
                {
                    response.IsSuccess = false;
                    response.Message = "The Content is not for community";
                    return response;
                }
                // Step 1: Validate user input
                response = _helper.IsUserNotNull(userInput);
                if (!response.IsSuccess)
                {
                    response.Message = "Invalid user input.";
                    return response;
                }


                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userInput.Email);
                if (user != null)
                {
                    if (user.Status != UserStatus.Inactive)
                    {
                        response.Message = "User is exist";
                        response.IsSuccess = false;
                        return response;
                    }

                    await DeleteUser(user.Id);
                }

                // Step 2: Check if username or email already exists
                response = await _helper.IsUserNotExist(userInput.Username, userInput.Email);
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
                if (count.NumEmail != 0)
                {
                    response.IsSuccess = false;
                    response.Message = "Email already exists.";
                    return response;
                }

                // Step 3: Map AddUserModel to User entity
                UserModel userMapper = _mapper.Map<UserModel>(userInput);

                userMapper.Status = UserStatus.Active;
                userMapper.EmailConfirmation = EmailStatus.Confirmed;
                userMapper.UpdatedAt = DateTime.UtcNow;
                if (string.IsNullOrEmpty(userMapper.Avatar))
                {
                    userMapper.Avatar = "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";
                }

                User userCreate = _mapper.Map<User>(userMapper);

                // Step 4: Generate random password and hash it
                string randomPassword = _helper.GenerateRandomPassword(6);
                userCreate.PasswordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword);

                // Step 5: Send email with account details
                ResponseModel emailSent = await _helper.SendEmailAsync(
                    userCreate.Email,
                    "Account Creation",
                    $"Your account has been created. Your password is: {randomPassword}"
                );

                if (!emailSent.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to send account creation email.";
                    return response;
                }

                // Step 6: Save user to the database

                await _context.Users.AddAsync(userCreate);
                await _context.SaveChangesAsync();

                // Step 7: Notify statistics update
                var totalRequest = await TotalRequest();
                _message.SendingMessage2(totalRequest.Result, "Stastistical", "totalUser_for_stastistical", "totalUser_for_stastistical", ExchangeType.Direct, true, false, false, false);

                response.IsSuccess = true;
                response.Message = "User created successfully.";
                response.Result = userMapper;
            }
            catch (DbUpdateException dbEx)
            {
                response.IsSuccess = false;
                response.Message = "Database operation failed.";
                // Log dbEx (add a logging library or mechanism)
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = "An unexpected error occurred.";
                // Log ex (add a logging library or mechanism)
            }

            return response;
        }





        public async Task<ResponseModel> DeleteUser(string id)
        {
            ResponseModel response = new();
            try
            {
                // Tìm người dùng theo ID
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"User with ID {id} not found.";
                    return response;
                }

                // Xóa người dùng
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                var totalRequest = await TotalRequest();
                _message.SendingMessage2(totalRequest.Result, "Stastistical", "totalUser_for_stastistical", "totalUser_for_stastistical", ExchangeType.Direct, true, false, false, false);

                response.IsSuccess = true;
                response.Message = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> GetAll(int pageNumber, int pageSize)
        {
            ResponseModel response = new();
            try
            {
                var users = await _context.Users.ToListAsync();
                if (users != null)
                {
                    response.Message = $"Found {users.Count} users";
                    response.Result = _mapper.Map<ICollection<UserModel>>(users).ToPagedList(pageNumber, pageSize);

                    var totalRequest = await TotalRequest();
                    _message.SendingMessage2(totalRequest.Result, "Stastistical", "totalUser_for_stastistical", "totalUser_for_stastistical", ExchangeType.Direct, true, false, false, false);
                }
                else
                {
                    response.Message = "Not found any user";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> GetUser(string id)
        {
            ResponseModel response = new();
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    response.Message = $"Found success user: {id} ";
                    response.Result = _mapper.Map<UserModel>(user);
                }
                else
                {
                    response.Message = $"Not found user: {id}";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ResponseModel> GetUserByEmail(string email)
        {
            ResponseModel response = new();
            try
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    response.Message = $"Found success user: {email} ";
                    response.Result = _mapper.Map<UserModel>(user);
                }
                else
                {
                    response.Message = $"Not found user: {email}";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }



        public async Task<ResponseModel> UpdateUser(UserUpdate updatedUserModel)
        {
            var response = new ResponseModel();

            try
            {
                // Step 0: Moderate description, name
                if (!_helper.IsContentAppropriate(updatedUserModel.Username))
                {
                    response.IsSuccess = false;
                    response.Message = "The Content is not for community";
                    return response;
                }
                // Kiểm tra xem người dùng có tồn tại không dựa trên ID của họ
                User user = await _context.Users.FindAsync(updatedUserModel.Id);
                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"User with ID {updatedUserModel.Id} not found.";
                    return response;
                }

                // Kiểm tra xem email và username có tồn tại không
                response = await _helper.IsUserNotExist(updatedUserModel.Username, phone: updatedUserModel.PhoneNumber, email: updatedUserModel.Email);
                CountModel count = new CountModel();
                count = (CountModel)response.Result;

                // Kiểm tra xem các giá trị mới có khác với giá trị cũ hay không
                bool isUsernameChanged = !string.Equals(user.Username, updatedUserModel.Username, StringComparison.OrdinalIgnoreCase);
                bool isEmailChanged = !string.Equals(user.Email, updatedUserModel.Email, StringComparison.OrdinalIgnoreCase);
                bool isPhoneNumberChanged = !string.Equals(user.PhoneNumber, updatedUserModel.PhoneNumber, StringComparison.OrdinalIgnoreCase);

                // Nếu không trùng lặp và có thay đổi các trường username, email hoặc phone number
                if (response.IsSuccess &&
                    (!isUsernameChanged || count.NumUsername == 0) &&
                    (!isPhoneNumberChanged || count.NumPhoneNumber == 0) &&
                    (!isEmailChanged || count.NumEmail == 0))
                {
                    var data = new UpdateUserNameModel()
                    {
                        OldUserName = user.Username,
                        NewUserName = updatedUserModel.Username
                    };
                    // Cập nhật thông tin từ UserModel vào đối tượng User
                    user.DisplayName = updatedUserModel.DisplayName;
                    user.Username = updatedUserModel.Username ?? user.Username;
                    user.PhoneNumber = updatedUserModel.PhoneNumber;
                    user.Email = updatedUserModel.Email ?? user.Email;
                    user.Avatar = updatedUserModel.Avatar ?? "https://static.vecteezy.com/system/resources/previews/020/911/747/non_2x/user-profile-icon-profile-avatar-user-icon-male-icon-face-icon-profile-icon-free-png.png";
                    user.Role = updatedUserModel.Role ?? user.Role;
                    user.EmailConfirmation = updatedUserModel.EmailConfirmation ?? user.EmailConfirmation;
                    user.Status = updatedUserModel.Status ?? user.Status;
                    user.BankName = updatedUserModel.BankName;
                    user.BankAccount = updatedUserModel.BankAccount;
                    user.UpdatedAt = DateTime.UtcNow;
                    // Lưu các thay đổi vào cơ sở dữ liệu
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    _message.SendingMessage3(data, "UpdateUserName", "updateUserName_queue_cmt", "updateUserName_queue_cmt", ExchangeType.Direct, true, false, false, false);

                    _message.SendingMessage3(data, "UpdateUserName", "updateUserName_queue_od", "updateUserName_queue_od", ExchangeType.Direct, true, false, false, false);

                    _message.SendingMessage3(data, "UpdateUserName", "updateUserName_queue_pro", "updateUserName_queue_pro", ExchangeType.Direct, true, false, false, false);

                    _message.SendingMessage3(data, "UpdateUserName", "updateUserName_queue_rp", "updateUserName_queue_rp", ExchangeType.Direct, true, false, false, false);

                    // Trả về thông báo thành công cùng với thông tin người dùng đã cập nhật
                    response.IsSuccess = true;
                    response.Message = "User updated successfully.";
                    response.Result = _mapper.Map<UserModel>(user);
                }
                else
                {
                    // Nếu có lỗi, trả về thông báo lỗi
                    response.IsSuccess = false;
                }

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }



        /// <summary>
        /// Tìm kiếm danh sách người dùng dựa trên truy vấn chuỗi.
        /// Nếu tìm thấy người dùng phù hợp với <see cref="UserModel.DisplayName"/>, <see cref="UserModel.NormalizedUsername"/>, hoặc <see cref="UserModel.NormalizedEmail"/>, trả về danh sách người dùng.
        /// Nếu không tìm thấy hoặc truy vấn trống, trả về thông báo tương ứng.
        /// </summary>
        /// <param name="query">Chuỗi truy vấn để tìm kiếm người dùng (theo <see cref="UserModel.DisplayName"/>, <see cref="UserModel.NormalizedUsername"/> hoặc <see cref="UserModel.NormalizedEmail"/>). Có thể là <see langword="null"/>.</param>
        /// <returns>
        /// Một đối tượng <see cref="ResponseModel"/> chứa <see cref="ResponseModel.Result"/> là danh sách người dùng được tìm thấy hoặc thông báo ở <see cref="ResponseModel.Message"/> nếu không tìm thấy.
        /// Thuộc tính <see cref="ResponseModel.IsSuccess"/> sẽ là <see langword="false"/> nếu có lỗi xảy ra.
        /// </returns>
        public async Task<ResponseModel> FindUsers(string? query, int pageNumber, int pageSize)
        {
            var response = new ResponseModel();

            try
            {
                response = _helper.NomalizeQuery(query);
                if (response.IsSuccess)
                {
                    query = response.Result.ToString();

                    // Try to parse the query as an ObjectId
                    if (ObjectId.TryParse(query, out var objectId))
                    {
                        // If it's a valid ObjectId, search by ObjectId
                        ICollection<User> users = await _context.Users.Where(u => u.Id == objectId.ToString()).ToListAsync();

                        if (users.Count == 0)
                        {
                            response.Message = $"User with ID '{query}' not found";
                            response.Result = null!;
                        }
                        else
                        {
                            response.Message = $"Found {users.Count} user(s) by ID";
                            response.Result = _mapper.Map<ICollection<UserModel>>(users).ToPagedList(pageNumber, pageSize);
                        }
                    }
                    else
                    {
                        // If it's not an ObjectId, proceed with normal search
                        ICollection<User> users = await _context.Users.Where(u =>
                            u.DisplayName!.ToUpper().Contains(query!) ||
                            u.NormalizedUsername!.ToUpper().Contains(query!) ||
                            u.NormalizedEmail!.ToUpper().Contains(query!) ||
                            u.PhoneNumber!.Contains(query!))
                            .ToListAsync();

                        if (users.Count == 0)
                        {
                            response.Message = $"'{query}' not found";
                            response.Result = null!;
                        }
                        else
                        {
                            response.Message = $"Found {users.Count} users";
                            response.Result = _mapper.Map<ICollection<UserModel>>(users).ToPagedList(pageNumber, pageSize);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }



        public async Task<ResponseModel> ChangeStatus(string id, UserStatus status)
        {
            var response = new ResponseModel();

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    response.IsSuccess = false;
                    response.Message = $"User with ID {id} not found.";
                    return response;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = $"Invalid status value: {status}.";
                }
                user.Status = status;
                user.UpdatedAt = DateTime.UtcNow;

                //if (user.Status == UserStatus.Deleted)
                //{
                //    await _helper.SendEmailAsync(user.Email, "Account Deletion", $"Your account has been deleted by. If you did not request this, please contact us immediately.");
                //}
                //else if (user.Status == UserStatus.Block)
                //{
                //    await _helper.SendEmailAsync(user.Email, "Account Blocked", "Your account has been blocked. If you did not request this, please contact us immediately.");
                //}


                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                response.IsSuccess = true;
                response.Message = "User status updated successfully.";
                response.Result = _mapper.Map<UserModel>(user);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> GetAllProductsInWishList(string id)
        {
            var response = new ResponseModel();

            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    throw new Exception($"User with ID {id} not found.");
                }

                if (user.Wishlist == null)
                {
                    throw new Exception($"User with ID {id} doesn't have any product in wishlist.");
                }
                var wishList = user.Wishlist.ToList();

                response.IsSuccess = true;
                response.Result = wishList;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }


        public async Task<ResponseModel> TotalRequest()
        {
            ResponseModel response = new();
            try
            {
                var Pros = await _context.Users.ToListAsync();
                if (Pros != null)
                {
                    var totalUser = Pros.Count;
                    var totalRequest = new TotalRequest()
                    {
                        totalUsers = totalUser,
                        updateAt = DateTime.Now,
                    };
                    response.Result = totalRequest;
                    return response;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any User";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<bool> CheckUserByUserNameAsync(string userName)
        {
            bool userExists = await _context.Users
            .AnyAsync(u => u.Username.Equals(userName, StringComparison.OrdinalIgnoreCase));
            return userExists;
        }

        public async Task<ResponseModel> AddToWishList(UserWishlistModel userWishlistModel, string userName)
        {
            ResponseModel response = new();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == userName);

                if (user != null)
                {
                    // Kiểm tra xem mục này đã tồn tại trong Wishlist của user chưa
                    bool isItemExists = user.Wishlist.Any(w => w.ProductId == userWishlistModel.ProductId);

                    if (!isItemExists)
                    {
                        user.Wishlist.Add(_mapper.Map<UserWishlist>(userWishlistModel));
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                        response.Result = user;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Item already exists in the wishlist";
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Not found any User";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<ResponseModel> RemoveFromWishList(string productId, string userName)
        {
            ResponseModel response = new();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == userName);

                if (user != null)
                {
                    // Kiểm tra xem mục này có tồn tại trong Wishlist của user không
                    var itemToRemove = user.Wishlist.FirstOrDefault(w => w.ProductId == productId);

                    if (itemToRemove != null)
                    {
                        user.Wishlist.Remove(itemToRemove);
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                        response.IsSuccess = true;
                        response.Message = "Item removed successfully from the wishlist.";
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Item not found in the wishlist.";
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "User not found.";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseModel> GetUserByListUsername(ExportResult models)
        {
            var response = new ResponseModel();
            try
            {
                models.ExportProfits = models.ExportProfits.OrderByDescending(i => i.PublisherName).ToList();
                var profitDictionary = models.ExportProfits.ToDictionary(m => m.PublisherName, m => m.TotalProfit);

                // Gọi hàm cập nhật TotalProfit
                //response = await UpdateUserTotalProfitAsync(profitDictionary, models.CreateAt);

                // Lấy danh sách username không tìm thấy
                var usernames = models.ExportProfits.Select(m => m.PublisherName).ToList();
                var users = await _context.Users
                    .Where(u => usernames.Contains(u.Username))
                    .OrderByDescending(i=>i.Username)
                    .ToListAsync();

                var foundUsernames = users.Select(u => u.Username).ToHashSet(); // Tăng tốc so sánh
                var notFoundUsernames = usernames.Where(u => !foundUsernames.Contains(u)).ToList();

                var responseUserExport = new FindUsernameModel()
                {
                    Users = _mapper.Map<ICollection<UserModel>>(users),                     // Danh sách user đã tìm thấy
                    UsersExport = _mapper.Map<ICollection<ExportProfitModel>>(users), // Danh sách thông tin cần thiết cần export của user
                    MissingUsers = notFoundUsernames,                                       // Danh sách username không tìm thấy
                    CreateAt = models.CreateAt
                };

                for(int i = 0; i<responseUserExport.UsersExport.Count; i++)
                {
                    responseUserExport.UsersExport.ElementAt(i).ProfitOfMonth = models.ExportProfits.ElementAt(i).TotalProfit;
                    responseUserExport.UsersExport.ElementAt(i).OriginalPriceOfMonth = models.ExportProfits.ElementAt(i).TotalPrice;
                    responseUserExport.UsersExport.ElementAt(i).PublisherName = models.ExportProfits.ElementAt(i).PublisherName;

                }

                //responseUserExport.UsersExport = _mapper.Map<ICollection<ExportProfitModel>>(models.ExportProfits);
                // Kết quả trả về
                response.IsSuccess = true;
                response.Message = notFoundUsernames.Count > 0
                    ? $"Some usernames were not found: {string.Join(", ", notFoundUsernames)}"
                    : "All users found successfully.";
                response.Result = responseUserExport;

            }
            catch (Exception ex)
            {
                // Xử lý lỗi với thông báo thân thiện
                response.IsSuccess = false;
                response.Message = $"An error occurred while processing: {ex.Message}";
            }

            return response;
        }


        //private async Task<ResponseModel> UpdateUserTotalProfitAsync(Dictionary<string, decimal> profitDictionary, DateTime createAt)
        //{
        //    ResponseModel response = new();
        //    try
        //    {

        //        // Lấy danh sách username từ profitDictionary
        //        var usernames = profitDictionary.Keys.ToList();

        //        // Lấy danh sách user từ database
        //        var users = await _context.Users
        //            .Where(u => usernames.Contains(u.Username))
        //            .ToListAsync();

        //        // Cập nhật TotalProfit cho từng user
        //        foreach (var user in users)
        //        {
        //            if (profitDictionary.TryGetValue(user.Username, out var profit))
        //            {
        //                user.TotalProfit += profit; // Cộng thêm profit
        //            }
        //        }

        //        // Lưu thay đổi vào database
        //        _context.Users.UpdateRange(users);
        //        await _context.SaveChangesAsync();

        //        response.Result = users;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Xử lý lỗi với thông báo thân thiện
        //        response.IsSuccess = false;
        //        response.Message = $"An error occurred while processing: {ex.Message}";
        //    }

        //    return response;
        //}

    }

}
