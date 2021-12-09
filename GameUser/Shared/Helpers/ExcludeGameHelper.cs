namespace GameUser.Shared.Helpers
{
    using GameUser.Features.CategoryConfigs.Models;
    using GameUser.Features.GameExtraConfigs;
    using GameUser.Features.UserSessions;
    using GameUser.Shared.Helper;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public static class ExcludeGameHelper
    {
        public static bool ShouldExcludeGameBySiteName(int gameId, List<GameExtraConfigEntity> gameExtraConfigs, UserSessionEntity userSession)
        {
            var gameByExcludeSiteType = gameExtraConfigs.FirstOrDefault(e => e.GameId == gameId && Equals(e.Type, ExtraConfigType.ExcludedSiteNameValue));
            var siteNames = GameExtraConfigHelper.SplitSiteNames(gameByExcludeSiteType?.Value);
            return siteNames != null && siteNames.Any(s => String.Equals(s, userSession.SiteName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ShouldExcludeGameByCustomerType(int gameId, List<GameExtraConfigEntity> gameExtraConfigs, UserSessionEntity userSession)
        {
            var gameByOfferedCustomerType = gameExtraConfigs.FirstOrDefault(o => o.GameId == gameId && Equals(o.Type, ExtraConfigType.OfferedCustomerTypeValue));
            return GameExtraConfigHelper.CheckGameExistsRightCustomerType(gameByOfferedCustomerType?.Value, userSession.IsCash);
        }
    }
}
