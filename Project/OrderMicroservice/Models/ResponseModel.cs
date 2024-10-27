namespace OrderMicroservice.Models
{
    public class ResponseModel
    {
        public object? Result { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = string.Empty;

    }
}
