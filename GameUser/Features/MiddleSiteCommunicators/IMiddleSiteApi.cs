using Refit;
using System.Threading.Tasks;

namespace GameUser.Features.MiddleSite
{
    [Headers("Authorization: Bearer")]
    public interface IMiddleSiteApi
    {
        [Get("/MemberData/IsNicknameExisted")]
        Task<bool> IsNicknameExisted(string nickname, [Header("Authorization")] string authorization = "");
    }
}