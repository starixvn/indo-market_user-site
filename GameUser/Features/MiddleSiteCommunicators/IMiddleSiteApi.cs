using System.Threading.Tasks;
using Refit;

namespace GameUser.Features.MiddleSite
{
    [Headers("Authorization: Bearer")]
    public interface IMiddleSiteApi
    {
        [Get("/MemberData/IsNicknameExisted")]
        Task<bool> IsNicknameExisted(string nickname, [Header("Authorization")] string authorization = "");
    }
}