namespace GameUser.Shared.Models
{
    public class UpdateResult
    {
        public UpdateResult(bool isSuccess, string message)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
        }

        public bool IsSuccess { get; }
        public string Message { get; }
    }
}
