using System;

namespace GameUser.Features.ApiComminicators.Models
{
    public class MemberApiResponse<T>
    {
        public DateTimeOffset TimeStamp { get; set; }

        public T Result { get; set; }
    }

    public class EnterPortalResponse
    {
        public string Seq { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorDescription { get; set; }

        public bool IsSuccessful() => ErrorCode == 0;
    }
}