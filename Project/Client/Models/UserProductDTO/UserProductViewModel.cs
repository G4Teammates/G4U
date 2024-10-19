using Client.Models.AuthenModel;
using Client.Models.ProductDTO;

namespace Client.Models.UserProductDTO
{
    public class UserProductViewModel
    {
        public LoginResponseModel? User { get; set; }
        public ICollection<ProductModel>? Products { get; set; }
    }
}
