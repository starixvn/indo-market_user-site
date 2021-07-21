using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace GameUser.Shared.Enumerations
{
    public class EnumLocalization<T1, T2> where T1 : Enum
    {
        private readonly IStringLocalizer<T2> localizer;

        public EnumLocalization(IStringLocalizer<T2> localizer)
        {
            this.localizer = localizer;
        }

        [Obsolete]
        public IEnumerable<SelectListItem> GetSelectListItems(string language = null)
        {
            var enums = Enum.GetValues(typeof(T1)).Cast<T1>();
            return enums.Select(x => new SelectListItem
            {
                Value = x.ToString(),
                Text = string.IsNullOrWhiteSpace(language) ? localizer[x.ToString()] : localizer.WithCulture(new CultureInfo(language))[x.ToString()]
            });
        }
    }
}