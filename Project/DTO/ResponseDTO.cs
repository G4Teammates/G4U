namespace DTO
{
    public class ResponseDTO
    {
        public int StatusCode { get; set; } = 0;
        public bool IsSuccess { get; set; } = false;
        public string Result { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
