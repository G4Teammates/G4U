namespace CategoryMicroservice.Models.DTO
{
    public class CategoryCheckExist
    {
        public ICollection<CategoryNameModel> categories { get; set; }
        public bool IsExist { get; set; }   
    }
}
