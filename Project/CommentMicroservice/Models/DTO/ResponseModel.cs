namespace CommentMicroservice.Models.DTO
{
    public class ResponseModel
    {
        public object Result { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;
    }
}
