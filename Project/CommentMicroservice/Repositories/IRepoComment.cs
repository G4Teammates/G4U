using CommentMicroservice.DBContexts.Entities;
using CommentMicroservice.Models;
using CommentMicroservice.Models.DTO;

namespace CommentMicroservice.Repositories
{
    public interface IRepoComment
    {
        Task<ResponseModel> GetAll(int page, int pageSize);
        Task<ResponseModel> GetById(string id);
        Task<ResponseModel> GetListById(string id, int page, int pageSize);
        Task<ResponseModel> CreateComment(CreateCommentDTO Comment);
        Task<ResponseModel> UpdateComment(CommentModel Comment);
        Task<ResponseModel> DeleteComment(string id);
        Task<ResponseModel> Search(string searchstring, int page, int pageSize);
        Task<ResponseModel> GetByproductId(string productId, int page, int pageSize);
        Task<ResponseModel> GetByParentId(string Parentid, int page, int pageSize);
       
    }
}
