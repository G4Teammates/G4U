using Client.Models.OrderModel;
using Client.Models;
using Client.Repositories.Interfaces;
using Client.Utility;
using Client.Repositories.Interfaces.Order;
using System;

namespace Client.Repositories.Services.Order
{
    public class ExportService(IBaseService baseService) : IExportService
    {
        private readonly IBaseService _baseService = baseService;

        public async Task<ResponseModel> Export(DateTime dateTime)
        {
            return await _baseService.SendAsync(new RequestModel()
            {
                ApiType = StaticTypeApi.ApiType.POST,
                Url = StaticTypeApi.APIGateWay + "/Export?datetime=" + dateTime
            });
        }
    }
}
