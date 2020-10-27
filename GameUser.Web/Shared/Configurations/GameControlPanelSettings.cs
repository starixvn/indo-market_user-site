namespace GameUser.Web.Shared.Configurations
{
    public interface IGameControlPanelSettings
    {
        string Host { get; }
    }

    public class GameControlPanelSettings : IGameControlPanelSettings
    {
        public GameControlPanelSettings(string host)
        {
            Host = host;
        }

        public string Host { get; }
    }
}
