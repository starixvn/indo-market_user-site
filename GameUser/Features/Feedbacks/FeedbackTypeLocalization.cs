using GameUser.Shared.Enumerations;
using GameUser.Shared.Resources;
using Microsoft.Extensions.Localization;

namespace GameUser.Features.Feedbacks
{
    public class FeedbackTypeLocalization : EnumLocalization<FeedbackTypeEnum, Resources>
    {
        public FeedbackTypeLocalization(IStringLocalizer<Resources> localizer) : base(localizer)
        {
        }
    }
}
