namespace GameUser.Features.CategoryConfigs.Models
{
    using GameUser.Shared.Enumerations;
    public class ExtraConfigType : Enumeration
    {
        public const string OfferedCustomerTypeValue = "offered-customer-type";
        public const string ExcludedSiteNameValue = "excluded-site-name";

        public static readonly ExtraConfigType OfferedCustomerType = new ExtraConfigType(OfferedCustomerTypeValue, "Offered Customer Type");
        public static readonly ExtraConfigType ExcludedSiteName = new ExtraConfigType(ExcludedSiteNameValue, "Excluded Site Name");

        public ExtraConfigType()
        {
        }

        public ExtraConfigType(string value, string displayName)
           : base(value, displayName)
        {
        }
    }
}
