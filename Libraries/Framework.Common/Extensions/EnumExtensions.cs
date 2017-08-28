using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.ComponentModel;

namespace Framework.Common.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 获取enum的DisplayArribute name 的集合
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <typeparam name="T">int,byte等</typeparam>
        /// <typeparam name="requireAll">是否列出所有值，默认第一项不列出，主要用于查询</typeparam>
        /// <returns></returns>
        public static List<SelectListItem> GetDisplayNames<TEnum, T>(
            string defaultTxt = "",
            object defaultVal = null,
            bool requireAll = true)
            where TEnum : struct, IConvertible
        {
            var type = typeof(TEnum);
            var items = new List<SelectListItem>();
            Array values = Enum.GetValues(type);

            if (values == null || values.Length == 0)
                return items;

            if (!string.IsNullOrEmpty(defaultTxt))
            {
                items.Add(new SelectListItem
                {
                    Text = defaultTxt,
                    Value = defaultVal == null ? string.Empty : defaultVal.ToString()
                });
            }
            else if (requireAll)
            {
                items.Add(new SelectListItem { Text = "全部", Value = string.Empty });
            }

            foreach (var val in values)
            {
                var memInfo = type.GetMember(val.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                string name = string.Empty, value = string.Empty;

                if (attributes != null && attributes.Length > 0)
                {
                    name = ((DisplayAttribute)attributes[0]).Name;
                }
                else
                {
                    attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                    if (attributes != null && attributes.Length > 0)
                    {
                        name = ((DescriptionAttribute)attributes[0]).Description;
                    }
                }

                if (string.IsNullOrEmpty(name))
                {
                    name = value = val.ToString();
                }
                else
                {
                    value = typeof(T).Name.ToLower() == "string" ? val.ToString() : ((T)val).ToString();
                }

                var item = new SelectListItem { Text = name, Value = value };

                items.Add(item);
            }

            return items;
        }
        /// <summary>
        /// 根据枚举值获取DisplayArribute name名称
        /// 或者DescriptionAttribute 值
        /// </summary>
        /// <typeparam name="TEnum">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <returns></returns>
        public static string GetDisplayName<TEnum>(this TEnum value)
             where TEnum : struct, IConvertible
        {
            var displayAttr = value.GetAttributeOfType<TEnum, DisplayAttribute>();

            if (displayAttr != null)
                return displayAttr.Name;

            var descriptionAttr = value.GetAttributeOfType<TEnum, DescriptionAttribute>();

            if (descriptionAttr != null)
                return descriptionAttr.Description;

            return value.ToString();
        }

        private static T GetAttributeOfType<TEnum, T>(this TEnum value)
            where TEnum : struct, IConvertible
            where T : Attribute
        {
            try
            {
                return value.GetType()
                       .GetMember(value.ToString()).Any()
                ? value.GetType()
                    .GetMember(value.ToString())
                    .First()
                    .GetCustomAttributes(false)
                    .OfType<T>()
                    .LastOrDefault()
                : null;
                //return value.GetType()
                //            .GetMember(value.ToString())
                //            .First()
                //            .GetCustomAttributes(false)
                //            .OfType<T>()
                //            .LastOrDefault();
            }
            catch
            {
                return null;
            }
        }
    }
}