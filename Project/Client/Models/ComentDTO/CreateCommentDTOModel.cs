using CommentMicroservice.DBContexts.Enum;

namespace Client.Models.ComentDTO
{
    public class CreateCommentDTOModel
    {
        public required string Content { get; set; }
        public CommentStatus Status { get; set; }
        public string UserName { get; set; }
        public string ProductId { get; set; }
        public string? ParentId { get; set; }
    }
}
