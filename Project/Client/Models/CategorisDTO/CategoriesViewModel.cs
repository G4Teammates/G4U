using Client.Models.UserDTO;

namespace Client.Models.CategorisDTO
{
    public class CategoriesViewModel
    {
        public CreateCategories? CreateCategory { get; set; }
        public UpdateCategories? UpdateCategory { get; set; }
        public ICollection<CategoriesModel>? Categories { get; set; }
    }
}
