using System;

namespace GameUser.Features.BackendAuthentications
{
    public class AuthenticationToken
    {
        public string AccessToken { get; set; }

        public int ExporesIn { get; set; }

        public DateTime LastRequest { get; set; }
    }
}