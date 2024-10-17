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
using UserMicroService.DBContexts.Enum;
using static Google.Apis.Requests.BatchRequest;

namespace UserMicroservice.Repositories.Services
{
    public class UserService(UserDbContext context, IMapper mapper, IHelperService helper) : IUserService
    {
        private readonly UserDbContext _context = context;
        private readonly IMapper _mapper = mapper;
        private readonly IHelperService _helper = helper;

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
                // Check if the user model is valid
                response = _helper.IsUserNotNull(userInput);
                if (!response.IsSuccess) return response;

                // Check if username or email already exists
                response = await _helper.IsUserNotExist(userInput.Username, userInput.Email);
                if (!response.IsSuccess) return response;

                // Map UserModel to User entity

                UserModel userMapper = _mapper.Map<UserModel>(userInput);

                User userCreate = _mapper.Map<User>(userMapper);

                // Generate random password for admin creation (future functionality: send via email)
                userCreate.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Abc123!");

                // Save the user to the database
                await _context.Users.AddAsync(userCreate);
                await _context.SaveChangesAsync();

                response.Message = "User created successfully";
                response.Result = userMapper;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
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

        public async Task<ResponseModel> GetAll()
        {
            ResponseModel response = new();
            try
            {
                var users = await _context.Users.ToListAsync();
                if (users != null)
                {
                    response.Message = $"Found {users.Count} users";
                    response.Result = _mapper.Map<ICollection<UserModel>>(users);
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
                if(user != null)
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
                response = await _helper.IsUserNotExist(updatedUserModel.Username, phone: updatedUserModel.PhoneNumber);
                if (!response.IsSuccess)
                {
                    // Cập nhật thông tin từ UserModel vào đối tượng User
                    user.DisplayName = updatedUserModel.DisplayName ?? user.DisplayName;
                    user.Username = updatedUserModel.Username ?? user.Username;
                    user.PhoneNumber = updatedUserModel.PhoneNumber ?? user.PhoneNumber;
                    user.Email = updatedUserModel.Email ?? user.Email;
                    user.Avatar = updatedUserModel.Avatar ?? user.Avatar;
                    user.Role = updatedUserModel.Role ?? user.Role;

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
                    response.Message = response.Message;
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
