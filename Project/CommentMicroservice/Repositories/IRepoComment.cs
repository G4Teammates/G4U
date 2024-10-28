using CommentMicroservice.DBContexts.Entities;
using CommentMicroservice.Models;
using CommentMicroservice.Models.DTO;

namespace CommentMicroservice.Repositories
{
    public interface IRepoComment
    {
        IEnumerable<Comment> Comments { get; }
        Task<Comment> GetById(string id);
        Task<List<Comment>> GetListById(string id);
        Comment CreateComment(CreateCommentDTO Comment);
        Task<Comment> UpdateComment(CommentModel Comment);
        Task DeleteComment(string id);
    }
}
