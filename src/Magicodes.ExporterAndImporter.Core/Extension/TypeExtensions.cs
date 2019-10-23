﻿// ======================================================================
// 
//           Copyright (C) 2019-2030 湖南心莱信息科技有限公司
//           All rights reserved
// 
//           filename : TypeExtensions.cs
//           description :
// 
//           created by 雪雁 at  2019-09-11 16:53
//           文档官网：https://docs.xin-lai.com
//           公众号教程：麦扣聊技术
//           QQ群：85318032（编程交流）
//           Blog：http://www.cnblogs.com/codelove/
// 
// ======================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Magicodes.ExporterAndImporter.Core.Extension
{
    /// <summary>
    ///     Defines the <see cref="TypeExtensions" />
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     获取显示名
        /// </summary>
        /// <param name="customAttributeProvider"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static string GetDisplayName(this ICustomAttributeProvider customAttributeProvider, bool inherit = false)
        {
            string displayName = null;
            var displayAttribute = customAttributeProvider.GetAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                displayName = displayAttribute.Name;
            }
            else
            {
                var displayNameAttribute = customAttributeProvider.GetAttribute<DisplayNameAttribute>();
                if (displayNameAttribute != null)
                    displayName = displayNameAttribute.DisplayName;
            }

            return displayName;
        }

        /// <summary>
        ///     获取类型描述
        /// </summary>
        /// <param name="customAttributeProvider"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static string GetDescription(this ICustomAttributeProvider customAttributeProvider, bool inherit = false)
        {
            var des = string.Empty;
            var desAttribute = customAttributeProvider.GetAttribute<DescriptionAttribute>();
            if (desAttribute != null) des = desAttribute.Description;
            return des;
        }

        /// <summary>
        ///     获取类型描述或显示名
        /// </summary>
        /// <param name="customAttributeProvider"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static string GetTypeDisplayOrDescription(this ICustomAttributeProvider customAttributeProvider,
            bool inherit = false)
        {
            var dispaly = customAttributeProvider.GetDescription(inherit);
            if (dispaly.IsNullOrWhiteSpace()) dispaly = customAttributeProvider.GetDisplayName(inherit);
            return dispaly ?? string.Empty;
        }


        /// <summary>
        ///     获取程序集属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static T GetAttribute<T>(this ICustomAttributeProvider assembly, bool inherit = false)
            where T : Attribute
        {
            return assembly
                .GetCustomAttributes(typeof(T), inherit)
                .OfType<T>()
                .FirstOrDefault();
        }

        /// <summary>
        ///     检查指定指定类型成员中是否存在指定的Attribute特性
        /// </summary>
        /// <typeparam name="T">要检查的Attribute特性类型</typeparam>
        /// <param name="assembly">The assembly<see cref="ICustomAttributeProvider" /></param>
        /// <param name="inherit">是否从继承中查找</param>
        /// <returns>是否存在</returns>
        public static bool AttributeExists<T>(this ICustomAttributeProvider assembly, bool inherit = false)
            where T : Attribute
        {
            return assembly.GetCustomAttributes(typeof(T), inherit).Any(m => m as T != null);
        }

        /// <summary>
        ///     是否必填
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsRequired(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetAttribute<RequiredAttribute>(true) != null) return true;
            //Boolean、Byte、SByte、Int16、UInt16、Int32、UInt32、Int64、UInt64、Char、Double、Single
            if (propertyInfo.PropertyType.IsPrimitive) return true;
            switch (propertyInfo.PropertyType.Name)
            {
                case "DateTime":
                case "Decimal":
                    return true;
            }

            return false;
        }

        /// <summary>
        ///     获取当前程序集中应用此特性的类
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="inherit">The inherit<see cref="bool" /></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesWith<TAttribute>(this Assembly assembly, bool inherit)
            where TAttribute : Attribute
        {
            var attrType = typeof(TAttribute);
            foreach (var type in assembly.GetTypes().Where(type => type.GetCustomAttributes(attrType, true).Length > 0))
                yield return type;
        }

        /// <summary>
        ///     获取枚举定义列表
        /// </summary>
        /// <returns>返回枚举列表元组（名称、值、描述）</returns>
        public static List<Tuple<string, int, string>> GetEnumDefinitionList(this Type type)
        {
            var list = new List<Tuple<string, int, string>>();
            var attrType = type;
            if (!attrType.IsEnum) return null;
            var names = Enum.GetNames(attrType);
            var values = Enum.GetValues(attrType);
            var index = 0;
            foreach (var value in values)
            {
                var name = names[index];
                string des = null;
                var objAttrs = value.GetType().GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (objAttrs != null &&
                    objAttrs.Length > 0)
                {
                    var descAttr = objAttrs[0] as DescriptionAttribute;
                    des = descAttr?.Description;
                }

                var item = new Tuple<string, int, string>(name, Convert.ToInt32(value), des);
                list.Add(item);
                index++;
            }

            return list;
        }

        /// <summary>
        ///     获取枚举显示名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IDictionary<string, int> GetEnumDisplayNames(this Type type)
        {
            if (!type.IsEnum) throw new InvalidOperationException();
            var names = Enum.GetNames(type);
            IDictionary<string, int> displayNames = new Dictionary<string, int>();
            foreach (var name in names)
            {
                if (type.GetField(name)
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .SingleOrDefault() is DisplayAttribute displayAttribute)
                {
                    var value = (int) Enum.Parse(type, name);
                    displayNames.Add(displayAttribute.Name, value);
                }
            }

            return displayNames;
        }

        /// <summary>
        ///     获取类的所有枚举
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Tuple<string, int, string>>> GetClassEnumDefinitionList(this Type type)
        {
            var enumPros = type.GetProperties().Where(p => p.PropertyType.IsEnum);
            var dic = new Dictionary<string, List<Tuple<string, int, string>>>();
            foreach (var item in enumPros) dic.Add(item.Name, item.PropertyType.GetEnumDefinitionList());
            return dic;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetCSharpTypeName(this Type type)
        {
            var sb = new StringBuilder();
            var name = type.Name;
            if (!type.IsGenericType) return name;
            sb.Append(name.Substring(0, name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(", ", type.GetGenericArguments()
                .Select(t => t.GetCSharpTypeName())));
            sb.Append(">");
            return sb.ToString();
        }
    }
}