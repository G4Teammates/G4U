using Client.Models.UserDTO;

namespace Client.Models.CategorisDTO
{
    public class CategoriesViewModel
    {
        public CreateCategories? CreateCategory { get; set; }

        public ICollection<CategoriesModel>? Categories { get; set; }
        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public int totalItem { get; set; }
        public int pageCount { get; set; }
    }
}
