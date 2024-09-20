using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserMicroservice.DBContexts;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.Models;
using UserMicroservice.Repositories.IRepositories;
using UserMicroService.DBContexts.Enum;
using UserMicroService.Models;

namespace UserMicroservice.Repositories.RepositoryService
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly IMapper _mapper;
        public UserService(UserDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Task<UserModel> AddUser(UserModel user)
        {
            throw new NotImplementedException();
        }

        public Task<UserModel> DeleteUser(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<UserModel>> GetAll()
        {
            var users = await _context.Users.ToListAsync();
            return _mapper.Map<ICollection<UserModel>>(users);
        }

        public Task<UserModel> GetUser(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserModel> UpdateUser(UserModel user)
        {
            throw new NotImplementedException();
        }

        public async Task<ICollection<UserViewModel>> FindUsers(string? query)
        {
            List<User> users = await _context.Users.Where(u => u.DisplayName.Contains(query)).ToListAsync();

            if (users == null || users.Count == 0)
            {
                return new List<UserViewModel>();
            }

            return _mapper.Map<ICollection<UserViewModel>>(users);
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
