using System.Collections.Generic;

namespace GameUser.Features.GameServerApiComunicators.Models
{
    public class MarketConigurations
    {
        public IList<string> Currencies { get; set; }

        public string DefaultChip { get; set; }

        public string MarketName { get; set; }

        public IList<GameConfigurations> Settings { get; set; }
    }
}