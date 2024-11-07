namespace ProductMicroservice.Models.Message
{
    public class CategoryCheckExistResponse
    {
        public List<CategoryModel> CategoryName { get; set; }
        public bool IsExist { get; set; }
    }
}
