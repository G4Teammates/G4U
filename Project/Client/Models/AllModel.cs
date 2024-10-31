using Client.Models.AuthenModel;
using Client.Models.CategorisDTO;
using Client.Models.ComentDTO;
using Client.Models.ProductDTO;

namespace Client.Models
{
	public class AllModel
	{
		public ICollection<CategoriesModel>? CategoriesModel { get; set; }
		public ICollection<ProductModel>? Product { get; set; }
		public LoginResponseModel? User { get; set; }
		public ICollection<CommentDTOModel>? comment { get; set; }

        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int totalItem { get; set; }
        public int pageCount { get; set; }
    }
}
