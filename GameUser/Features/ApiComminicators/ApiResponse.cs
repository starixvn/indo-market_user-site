namespace GameUser.Features.ApiComminicators
{
    public class ApiResponse<T> where T : ApiResult
    {
        public bool IsSuccessful { get; set; } = true;

        public T Result { get; set; }
    }
}