using Client.Models.CategorisDTO;

namespace Client.Models.ComentDTO
{
    public class CommentViewModel
    {
        public CreateCommentDTOModel? CreateComment { get; set; }

        public ICollection<CommentDTOModel>? Comment { get; set; }
    }
}
