using System.Collections.Generic;
using GameUser.Shared.Enumerations;
using GameUser.Shared.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace GameUser.Features.Feedbacks
{
    public interface IFeedbackTypeLocalization
    {
        IEnumerable<SelectListItem> GetSelectListItems(string language = null);
    }

    public class FeedbackTypeLocalization : EnumLocalization<FeedbackType, Resources>, IFeedbackTypeLocalization
    {
        public FeedbackTypeLocalization(IStringLocalizer<Resources> localizer) : base(localizer)
        {
        }
    }
}