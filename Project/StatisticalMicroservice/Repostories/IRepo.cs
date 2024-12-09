using StatisticalMicroservice.DBContexts.Entities;
using StatisticalMicroservice.Model;
using StatisticalMicroservice.Model.DTO;
using StatisticalMicroservice.Model.Message;
using StatisticalMicroservice.Models.DTO;

namespace StatisticalMicroservice.Repostories
{
    public interface IRepo
    {
        Task<ResponseDTO> GetAll(int page, int pageSize);
        Task<ResponseDTO> CreateStastistical(CreateStatisticalModel Stastistical);
        Task<ResponseDTO> UpdateStastistical(StatisticalModel Stastistical);
        Task<ResponseDTO> UpdateStastisticalProduct(ProductResponse productResponse);
        Task<ResponseDTO> UpdateStastisticalUser(UserResponse userResponse);
        Task<ResponseDTO> UpdateStastisticalOder(OrderResponse orderResponse);
        Task<ResponseDTO> GetStastisticalByUser(TotalGroupByUserRequest totalGroupByUserRequest);

    }
}
