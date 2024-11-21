﻿using AutoMapper;
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
                // Step 1: Validate user input
                response = _helper.IsUserNotNull(userInput);
                if (!response.IsSuccess)
                {
                    response.Message = "Invalid user input.";
                    return response;
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
                userCreate.UpdatedAt = DateTime.UtcNow;
                await _context.Users.AddAsync(userCreate);
                await _context.SaveChangesAsync();

                // Step 7: Notify statistics update
                var totalRequest = await TotalRequest();
                _message.SendingMessageStatistiscal(totalRequest.Result);

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
                _message.SendingMessageStatistiscal(totalRequest.Result);

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
                    _message.SendingMessageStatistiscal(totalRequest.Result);
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
                var user = await _context.Users.SingleOrDefaultAsync(u=>u.Email == email);
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
                    // Cập nhật thông tin từ UserModel vào đối tượng User
                    user.DisplayName = updatedUserModel.DisplayName ?? user.DisplayName;
                    user.Username = updatedUserModel.Username ?? user.Username;
                    user.PhoneNumber = updatedUserModel.PhoneNumber ?? user.PhoneNumber;
                    user.Email = updatedUserModel.Email ?? user.Email;
                    user.Avatar = updatedUserModel.Avatar ?? user.Avatar;
                    user.Role = updatedUserModel.Role ?? user.Role;
                    user.Status = updatedUserModel.Status ?? user.Status;
                    user.UpdatedAt = DateTime.UtcNow;
                    // Lưu các thay đổi vào cơ sở dữ liệu
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

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
        public async Task<ResponseModel> FindUsers(string? query)
        {
            var response = new ResponseModel();

            try
            {
                response = _helper.NomalizeQuery(query);
                if (response.IsSuccess)
                {
                    query = response.Result.ToString();

                    ICollection<User> users = await _context.Users.Where(u =>
                         u.DisplayName!.Contains(query!) ||
                         u.NormalizedUsername!.Contains(query!) ||
                         u.NormalizedEmail!.Contains(query!) ||
                         u.PhoneNumber!.Contains(query!))
                         .ToListAsync();

                    if (users.Count == 0)
                    {
                        response.Message = $"'{query}' not found";
                        response.Result = null!;
                    }

                    else
                    {
                        response.Result = _mapper.Map<ICollection<UserModel>>(users);
                        response.Message = $"{users.Count} users were found";
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

                if(user.Status == UserStatus.Deleted)
                {
                    await _helper.SendEmailAsync(user.Email, "Account Deletion", "Your account has been deleted. If you did not request this, please contact us immediately.");
                }
                else if(user.Status == UserStatus.Block)
                {
                    await _helper.SendEmailAsync(user.Email, "Account Blocked", "Your account has been blocked. If you did not request this, please contact us immediately.");
                }


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


        //public async Task<ICollection<UserModel>> FindUsers(SearchCriteria criteria)
        //{
        //    var query = _context.Users.AsQueryable();

        //    if (!string.IsNullOrEmpty(criteria.DisplayName))
        //    {
        //        query = query.Where(u => u.DisplayName!.Contains(criteria.DisplayName));
        //    }

        //    if (!string.IsNullOrEmpty(criteria.Email))
        //    {
        //        query = query.Where(u => u.Email == criteria.Email);
        //    }

        //    if (!string.IsNullOrEmpty(criteria.PhoneNumber))
        //    {
        //        query = query.Where(u => u.PhoneNumber == criteria.PhoneNumber);
        //    }

        //    if (!string.IsNullOrEmpty(criteria.Username))
        //    {
        //        query = query.Where(u => u.Username == criteria.Username);
        //    }

        //    query = query.Where(u => u.Status == criteria.Status);

        //    var users = await query.ToListAsync();
        //    return _mapper.Map<ICollection<UserModel>>(users);
        //}
    }

}
