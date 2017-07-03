// #region Author Information
// // ParserUtilities.cs
// // 
// // (c) Apexol Technologies. All Rights Reserved.
// // 
// #endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using SpruceFramework.Extensions;

namespace SpruceFramework
{
    public class QueryParserUtilities
    {
        
       
        internal static string[] ParseTypeKeyValues(Type type, params string[] exclude)
        {
            var props = type.GetDatabaseUsableProperties();
            var columns = props.Select(p => p.Name).Where(s => !exclude.Contains(s)).ToArray();
            var parameters = columns.Select(name => name + " = @" + name).ToArray();
            return parameters;
        }

        internal static Dictionary<string, object> ParseObjectKeyValues(dynamic obj, params string[] exclude)
        {
            if (obj == null)
                return null;
            var props = ((Type) obj.GetType()).GetDatabaseUsableProperties().ToArray();
            props = props.Where(x => !exclude.Contains(x.Name)).ToArray();
            var dict = new Dictionary<string, object>();
            foreach(var p in props)
            {
                var propertyName = p.Name;
                var propertyValue = p.GetValue(obj);
                dict.Add(propertyName, propertyValue);
            }

            if (!props.Any())
            {
                dict = ((IDictionary<string, object>) obj).ToDictionary(x => x.Key, x => x.Value);
            }
            return dict;
        }

       

       
    }
}