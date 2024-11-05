namespace StatisticalMicroservice.Model.DTO
{
    public class CreateStatisticalModel
    {
        public TotalWebsiteInfoModel TotalWebsite { get; set; }

        /*public List<UserInfoModel> TotalUser { get; set; }*/

        public DateTime CreateAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
