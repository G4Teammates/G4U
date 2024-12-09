using Microsoft.AspNetCore.Mvc;
using UserMicroservice.Models;
using UserMicroservice.Models.Message;
using UserMicroservice.Models.UserManagerModel;

namespace UserMicroservice.Repositories.Interfaces
{
    public interface IExportService
    {
        public Task<FileStreamResult> Export(ExportResult orders);
    }
}
