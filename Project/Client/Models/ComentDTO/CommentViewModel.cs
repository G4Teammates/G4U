﻿using Client.Models.CategorisDTO;

namespace Client.Models.ComentDTO
{
    public class CommentViewModel
    {
        public CreateCommentDTOModel? CreateComment { get; set; }

        public ICollection<CommentDTOModel>? Comment { get; set; }

		public int pageNumber { get; set; }
		public int pageSize { get; set; }
		public int totalItem { get; set; }
		public int pageCount { get; set; }
	}
}
