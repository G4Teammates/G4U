using Client.Models;
using Client.Models.ComentDTO;
using CommentMicroservice.Models;
using CommentMicroservice.Models.DTO;
using ResponseModel = Client.Models.ResponseModel;

namespace Client.Repositories.Interfaces.Comment
{
    public interface ICommentService
    {
        Task<ResponseModel> GetByIdAsync(string id);
        Task<ResponseModel> GetByproductId(string productId, int page, int pageSize);
        Task<ResponseModel> GetAllCommentAsync(int pageNumber, int pageSize);
        Task<ResponseModel> GetListByIdAsync(string id);
        Task<ResponseModel> CreateCommentAsync(CreateCommentDTOModel Comment);
        Task<ResponseModel> UpdateCommentAsync(CommentDTOModel Comment);
        Task<ResponseModel> DeleteCommentAsync(string id);
        Task<ResponseModel?> SearchCmtAsync(string searchString, int? pageNumber, int pageSize);
    }
}
