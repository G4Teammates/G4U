namespace UserMicroservice.Models
{
    public class ResponseModel
    {
        public Object Result { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;

    }
}
