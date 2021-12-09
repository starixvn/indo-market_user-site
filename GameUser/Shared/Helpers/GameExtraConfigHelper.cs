namespace GameUser.Shared.Helper
{
    using GameUser.Features.CategoryConfigs.Models;
    using System.Collections.Generic;
    using System.Linq;

    public static class GameExtraConfigHelper
    {
        public static List<string> SplitSiteNames(string siteNameList)
           => siteNameList?
               .TrimEnd(',')
               .Split(',')
               .Select(s => s.Trim())
               .ToList();

        public static bool CheckGameExistsRightCustomerType(string offeredCustomerType, bool isCash)
        {
            var customerType = CustomerType.FromIsCash(isCash);
            if (offeredCustomerType != null
                && !Equals(offeredCustomerType, customerType.Value)
                && !Equals(offeredCustomerType, CustomerType.AllValue))
            {
                return true;
            }

            return false;
        }
    }
}
