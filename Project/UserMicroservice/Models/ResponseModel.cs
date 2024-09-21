namespace UserMicroservice.Models
{
    public class ResponseModel
    {
        public Object Result { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;

    }
}
