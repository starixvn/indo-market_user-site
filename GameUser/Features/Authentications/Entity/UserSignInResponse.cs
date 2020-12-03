namespace GameUser.Features.Authentications.Entity
{
    public class UserSignInResponse
    {
        public bool IsAuthenticated { get; set; }

        public string Username { get; set; }

        public string Language { get; set; }

        public string Session { get; set; }

        public string Currency { get; set; }
    }
}