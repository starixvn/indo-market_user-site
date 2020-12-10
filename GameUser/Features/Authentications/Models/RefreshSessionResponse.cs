using System;

namespace GameUser.Features.Authentications.Models
{
    public class RefreshSessionResponse
    {
        public bool IsSuccessful { get; set; } = false;

        public DateTime ExpiredIn { get; set; }
    }
}