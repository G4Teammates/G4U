using Client.Models;

namespace Client.Repositories.Interfaces.Order
{
    public interface IExportService
    {
        public Task<ResponseModel> Export(DateTime datetime);
    }
}
