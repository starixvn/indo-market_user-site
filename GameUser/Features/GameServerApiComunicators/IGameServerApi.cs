using System.Threading.Tasks;
using GameUser.Features.GameServerApiComunicators.Models;
using Refit;

namespace Features.GameUser.GameServerApiComunicators
{
    [Headers("Authorization: Bearer")]
    public interface IGameServerApi
    {
        [Get("/api/gameSettings")]
        Task<AllGamesConfigurations> GetGameSettings();
    }
}