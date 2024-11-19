using Client.Models;
using Client.Models.ComentDTO;
using Client.Models;
using ResponseModel = Client.Models.ResponseModel;

namespace Client.Repositories.Interfaces.Comment
{
    public interface ICommentService
    {
        Task<ResponseModel> GetByIdAsync(string id);
        Task<ResponseModel> GetByproductId(string productId, int page, int pageSize);
        Task<ResponseModel> GetAllCommentAsync(int pageNumber, int pageSize);
        Task<ResponseModel> GetListByIdAsync(string id, int page, int pageSize);
        Task<ResponseModel> CreateCommentAsync(CreateCommentDTOModel Comment, string userId);
        Task<ResponseModel> UpdateCommentAsync(CommentDTOModel Comment);
        Task<ResponseModel> DeleteCommentAsync(string id);
        Task<ResponseModel?> SearchCmtAsync(string searchString, int? pageNumber, int pageSize);
        Task<ResponseModel> GetByParentIdAsync(string parentId , int? pageNumber, int pageSize);
        Task<ResponseModel> IncreaseLike(string commentId, UserLikesModel userLikes);
        Task<ResponseModel> DecreaseLike(string commentId, UserDisLikesModel userDisLikes);
    }
}
