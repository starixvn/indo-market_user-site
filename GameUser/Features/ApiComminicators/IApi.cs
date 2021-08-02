using System.Threading.Tasks;
using GameUser.Features.ApiComminicators.Models;
using Refit;

namespace GameUser.Features.ApiComminicators
{
    [Headers("Authorization: Bearer")]
    public interface IApi
    {
        [Post("/AuthVerification/verify")]
        Task<VerificationTokenResponse> VerifyToken(VerificationTokenRequest request);

        [Post("/Member/EnterPortal")]
        Task<MemberApiResponse<EnterPortalResponse>> EnterPortal(EnterPortalRequest request);

        [Get("/Member/GetBalance")]
        Task<MemberApiResponse<GetBalanceResponse>> GetBalance(GetBalanceRequest request);
    }
}