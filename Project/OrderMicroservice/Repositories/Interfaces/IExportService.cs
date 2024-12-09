using Microsoft.AspNetCore.Mvc;
using OrderMicroservice.Models;
using OrderMicroservice.Models.UserModel;
namespace OrderMicroservice.Repositories.Interfaces
{
    public interface IExportService
    {
        public Task<ResponseModel> Export(DateTime datetime);
        //public void PrepareData(FindUsernameModel users);
    }
}
