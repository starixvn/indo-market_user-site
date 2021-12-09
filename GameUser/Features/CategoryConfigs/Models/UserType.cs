namespace GameUser.Features.CategoryConfigs.Models
{
    public class UserType
    {
        public string MarketId { get; }

        public bool IsCash { get; }

        public UserType(string marketId, bool isCash) => (MarketId, IsCash) = (marketId, isCash);
       
    }
}
