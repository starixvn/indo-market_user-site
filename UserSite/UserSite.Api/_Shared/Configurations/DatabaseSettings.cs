namespace UserSite.Api._Shared.Configurations
{
    public interface IDatabaseSettings
    {
        string ConnectionString { get; set; }

        string DatabaseName { get; set; }
    }

    public class DatabaseSettings : IDatabaseSettings
    {
        public string ConnectionString { get; set; }

        public string DatabaseName { get; set; }
    }
}
