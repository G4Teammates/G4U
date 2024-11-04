﻿using MongoDB.Bson;
using UserMicroservice.DBContexts.Entities;
using UserMicroservice.DBContexts.Enum;
using UserMicroservice.Models;
using UserMicroservice.Models.UserManagerModel;
namespace UserMicroservice.Repositories.Interfaces
{
    public interface IUserService
    {
        Task<ResponseModel> GetAll(int pageNumber, int pageSize);
        Task<ResponseModel> GetUser(string id);
        Task<ResponseModel> AddUserAsync(AddUserModel user);
        Task<ResponseModel> UpdateUser(UserUpdate user);
        Task<ResponseModel> DeleteUser(string id);
        Task<ResponseModel>? FindUsers(string? query);
        Task<ResponseModel> ChangeStatus(string id, UserStatus status);
        Task<ResponseModel> TotalRequest();
        //Task<ICollection<UserModel>> FindUsersCriteria(SearchCriteria criteria);


    }

}
