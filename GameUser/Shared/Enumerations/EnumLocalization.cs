using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameUser.Shared.Enumerations
{
    public class EnumLocalization<T1, T2> where T1 : Enum
    {
        private readonly IStringLocalizer<T2> localizer;

        public EnumLocalization(IStringLocalizer<T2> localizer)
        {
            this.localizer = localizer;
        }

        public IEnumerable<SelectListItem> GetSelectListItems()
        {
            var enums = Enum.GetValues(typeof(T1)).Cast<T1>();
            return enums.Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = localizer[x.ToString()]
            });
        }
    }
}
