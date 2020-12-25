using System;

namespace GameUser.Features.Authentications.Models
{
    public class RefreshSessionResponse
    {
        public RefreshSessionResponse(bool isSuccessful = false)
        {
            IsSuccessful = isSuccessful;
        }

        public RefreshSessionResponse(bool isSuccessful, DateTime expiredIn) : this(isSuccessful)
        {
            ExpiredIn = expiredIn;
        }

        public bool IsSuccessful { get; } = false;

        public DateTime ExpiredIn { get; }
    }
}