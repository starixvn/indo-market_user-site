using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sentry;

namespace GameUser.Features.Avatars
{
    public interface IAvatarService
    {
        Task<AvatarObject> GetAvatar(string avatarId);
    }

    public class AvatarService : IAvatarService
    {
        private readonly ISentryClient sentryClient;

        public AvatarService(ISentryClient sentryClient)
        {
            this.sentryClient = sentryClient;
        }

        public async Task<AvatarObject> GetAvatar(string avatarId)
        {
            return await Task.FromResult(ReadPlistFile(avatarId));
        }

        private AvatarObject ReadPlistFile(string avatarId)
        {
            try
            {
                var directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                var file = Path.Combine(directory, "Features/Avatars/profile-picture.plist.meta.json");
                var avatarPlist = JsonConvert.DeserializeObject<AvatarPlist>(File.ReadAllText(file));
                var avatarObject = avatarPlist.SubMetas.First(a => a.Key.Replace(".png", "") == avatarId);

                return avatarObject.Value;
            }
            catch (Exception e)
            {
                sentryClient.CaptureException(e);
                return new AvatarObject { TrimX = 1, TrimY = 1 };
            }
        }
    }
}