using System.Collections.Generic;

namespace GameUser.Features.GameServerApiComunicators.Models
{
    public class GameConfigurations
    {
        public int GameId { get; set; }

        public string GameName { get; set; }

        public string IconSize { get; set; }

        public int SortOrder { get; set; }

        public IList<Chip> Chips { get; set; }
    }
}