using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Repositories.Interfaces;
using UserMicroservice.Repositories.IRepositories;
using UserMicroService.DBContexts.Enum;
using UserMicroService.Models;

namespace UserMicroservice.Repositories.RepositoryService
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHelperService _helper;
        public UserService(UserDbContext context, IMapper mapper, IHelperService helper)
        {
            _context = context;
            _mapper = mapper;
            _helper = helper;
        }

        public async Task<ResponseModel> AddUser(UserModel user)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                response = _helper.IsUserNotNull(user);
                if (response.IsSuccess)
                {
                    response = await _helper.IsUserNotExist(user.Username, user.Email);
                    if (response.IsSuccess)
                    {
                        var newUser = _mapper.Map<User>(user);
                        await _context.Users.AddAsync(newUser);
                        await _context.SaveChangesAsync();
                        response.Result = _mapper.Map<UserModel>(newUser);
                        response.Message = "User was created successfully";
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

        public async Task<ResponseModel> DeleteUser(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ResponseModel> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return new ResponseModel { Result = _mapper.Map<ICollection<UserModel>>(users) };
        }

        public Task<ResponseModel> GetUser(Guid id)
        {
            throw new NotImplementedException();
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
                        u.DisplayName!.ToUpper().Contains(query!) ||
                        u.NormalizedUsername!.Contains(query!) ||
                        u.NormalizedEmail!.Contains(query!) ||
                        u.PhoneNumber!.Contains(query!))
                        .ToListAsync();

                    if (users.Count == 0)
                    {
                        response.Message = $"'{query}' not found";
                        response.Result = null;
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
