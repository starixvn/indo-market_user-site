using System.Collections.Generic;

namespace GameUser.Features.GameServerApiComunicators.Models
{
    public class AllGamesConfigurations
    {
        public IList<Chip> Chips { get; set; }

        public IList<MarketConigurations> MarketConfigs { get; set; }

        public Dictionary<string, IList<string>> SupportedGameByCurrency { get; set; }
    }
}