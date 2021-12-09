namespace GameUser.Features.CategoryConfigs.Models
{
    using GameUser.Shared.Enumerations;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CustomerType : Enumeration
    {
        public bool IsCash { get; }
        public const string AllValue = "1";
        public const string CreditValue = "2";
        public const string DepositValue = "3";

        public static readonly CustomerType Credit = new CustomerType(CreditValue, "Credit", false);
        public static readonly CustomerType Deposit = new CustomerType(DepositValue, "Deposit", true);

        public CustomerType()
        {
        }

        public CustomerType(string value, string displayName, bool isCash)
           : base(value, displayName)
        {
            IsCash = isCash;
        }

        public static IEnumerable<CustomerType> List() =>
            new[] { Credit, Deposit };
        
        public static CustomerType FromIsCash(bool isCash)
        {
            var matchingItem = List().SingleOrDefault(s => s.IsCash == isCash);

            if (matchingItem == null)
            {
                var message = string.Format("'{0}' is not a valid", isCash);
                throw new ArgumentOutOfRangeException(message);               
            }

            return matchingItem;
        }
    }
}
