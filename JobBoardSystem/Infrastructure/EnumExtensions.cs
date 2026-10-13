using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace JobBoardSystem.Infrastructure
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var field = value.GetType().GetField(value.ToString());
            if (field == null)
            {
                return value.ToString();
            }

            var attribute = field.GetCustomAttribute<DisplayAttribute>();
            return attribute == null ? value.ToString() : attribute.GetName();
        }

        public static IList<SelectListItem> ToSelectList<TEnum>(TEnum? selected = null) where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<Enum>()
                .Select(value => new SelectListItem
                {
                    Value = value.ToString(),
                    Text = value.GetDisplayName(),
                    Selected = selected.HasValue && selected.Value.ToString() == value.ToString()
                })
                .ToList();
        }
    }
}
