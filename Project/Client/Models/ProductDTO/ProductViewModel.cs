

using Client.Models.CategorisDTO;
using Client.Models.ComentDTO;
using Client.Models.UserDTO;

namespace Client.Models.ProductDTO
{
    public class ProductViewModel
    {
        public CreateProductModel? CreateProductModel { get; set; }
        public UpdateProductModel? UpdateProductModel { get; set; }
        public ICollection<CommentDTOModel> CommentDTOModels { get; set; }
        public CreateCommentDTOModel? createcomment { get; set; }
        
        public CommentDTOModel CMT { get; set; }
        public ICollection<CategoriesModel>? CategoriesModel { get; set; }
        public ICollection<ProductModel>? Product { get; set; }

        public ProductModel? Prod { get; set; }
       /* public ICollection<UsersDTO>? User { get; set; }*/

        public ICollection<WishlistModel>? Wishlist { get; set; }
       /* public UsersDTO? User1 { get; set; }*/
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int totalItem { get; set; }
        public int pageCount { get; set; }
        public string userName { get; set; }
        public string userID {  get; set; }

    }
}
