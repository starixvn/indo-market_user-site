namespace GameUser.Web.Shared.Models
{
    using Newtonsoft.Json;
    public class ErrorDetails
    {
        public ErrorDetails(int statusCode, string message)
        {
            this.StatusCode = statusCode;
            this.Message = message;
        }

        public int StatusCode { get; }

        public string Message { get; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
