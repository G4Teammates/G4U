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
using UserMicroservice.Repositories.Interfaces;
using UserMicroservice.Repositories.IRepositories;
using UserMicroService.DBContexts.Enum;
using UserMicroService.Models;

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
        public async Task<ResponseModel> AddUserAsync(UserModel userModel, bool isAdmin, string? password = null)
        {
            ResponseModel response = new();
            try
            {
                // Check if the user model is valid
                response = _helper.IsUserNotNull(userModel);
                if (!response.IsSuccess) return response;

                // Check if username or email already exists
                response = await _helper.IsUserNotExist(userModel.Username, userModel.Email);
                if (!response.IsSuccess) return response;

                // Map UserModel to User entity
                User user = _mapper.Map<User>(userModel);

                if (isAdmin)
                {
                    // Generate random password for admin creation (future functionality: send via email)
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Abc123!");
                }
                else
                {
                    // Hash the password provided by the user during registration
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password!);
                }

                // Save the user to the database
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                response.Message = "User created successfully";
                response.Result = _mapper.Map<UserModel>(user);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }




        public Task<ResponseModel> DeleteUser(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return new ResponseModel { Result = _mapper.Map<ICollection<UserModel>>(users) };
        }

        public async Task<ResponseModel> GetUser(string id)
        {

            var user = await _context.Users.FindAsync(id);
            return new ResponseModel { Result = _mapper.Map<UserModel>(user) };
        }

        public Task<ResponseModel> UpdateUser(UserModel user)
        {
            throw new NotImplementedException();
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
                         u.DisplayName!.Contains(query!, StringComparison.OrdinalIgnoreCase) ||
                         u.NormalizedUsername!.Contains(query!, StringComparison.OrdinalIgnoreCase) ||
                         u.NormalizedEmail!.Contains(query!, StringComparison.OrdinalIgnoreCase) ||
                         u.PhoneNumber!.Contains(query!, StringComparison.OrdinalIgnoreCase))
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
