using System;
using GameUser.Shared.Models;

namespace GameUser.Features.Authentications.Models
{
    public class UserSignInResponse
    {
        public UserSignInResponse(bool isAuthenticate = false)
        {
            IsAuthenticated = isAuthenticate;
        }

        public UserSignInResponse(bool isAuthenticate, string username, string language, string session, string currency, DateTime expiredIn) : this(isAuthenticate)
        {
            Username = username;
            Language = language;
            Session = session;
            Currency = currency;
            ExpiredIn = expiredIn;
        }

        public bool IsAuthenticated { get; }

        public string Username { get; }

        public string Language { get; }

        public string Session { get; }

        public string Currency { get; }

        public DateTime ExpiredIn { get; }
    }
}