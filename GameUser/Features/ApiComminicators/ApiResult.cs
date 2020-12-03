namespace GameUser.Features.ApiComminicators
{
    public class ApiResult
    {
        public bool IsSuccessful { get; set; }

        public int Code { get; set; }

        public string Message { get; set; }
    }
}