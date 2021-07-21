using GameUser.Features.BackendAuthentications;

namespace GameUser.Features.MiddleSite
{
    public class MiddleSiteSettings
    {
        public string BaseUrl { get; set; }

        public AuthenticationSettings AuthenticationSettings { get; set; }
    }
}