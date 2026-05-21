#region Licence
/****************************************************************
 *  Filename: SchemaIdStrategy.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Date          2026-05-19
 *  Copyright (c) 2026 Martin Meissner.
 *                Released under the Apache License 2.0 as part of
 *                the open-source PlayOnDemand release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion
using System;
using System.Globalization;
using System.Linq;

namespace Pod.Web.Center.Swagger {
    public static class SchemaIdStrategy
    {
        public static string RemoveModelSufixStrategy(Type currentClass)
        {

            string returnedValue = currentClass.Name;
            if(returnedValue.Contains('`'))
            {
                returnedValue = FixNames(RemoveGenericFromName(returnedValue));
                returnedValue = returnedValue + GetGenericTypeArguments(currentClass.GenericTypeArguments);
            }
            else
            {
                return FixNames(returnedValue);
            }
            return returnedValue;
        }

        static string GetGenericTypeArguments(Type[] typesOnSameDepth)
        {
            if(!typesOnSameDepth.Any())return String.Empty;
            string typesList = "[";
            foreach(Type type in typesOnSameDepth)
            {
                if(IncludesGeneric(type.Name))
                {
                    typesList = typesList + FixNames(RemoveGenericFromName(type.Name));
                    typesList = typesList + GetGenericTypeArguments(type.GetGenericArguments());
                }
                else
                {
                    typesList = typesList + FixNames(type.Name);
                }
            }
            return typesList + "]";
        }

        static bool IncludesGeneric(string name)
        {
            if(name.Contains('`')) return true;
            return false;
        }
        static string RemoveGenericFromName(string nameWithGeneric)
        {
            return nameWithGeneric.Substring(0,nameWithGeneric.LastIndexOf('`'));
        }
        static string FixNames(string name)
        {
            if (name.EndsWith("ViewModel",true, CultureInfo.InvariantCulture))
                name = name.Replace("ViewModel", string.Empty,true, CultureInfo.InvariantCulture);
            if (name.EndsWith("Dto",true, CultureInfo.InvariantCulture))
                name = name.Replace("Dto", string.Empty,true, CultureInfo.InvariantCulture);
            if (name.Contains("ICollection", StringComparison.InvariantCultureIgnoreCase))
                name = name.Replace("ICollection", "Array",true, CultureInfo.InvariantCulture);
            if (name.Contains("IList", StringComparison.InvariantCultureIgnoreCase))
                name = name.Replace("IList", "Array",true, CultureInfo.InvariantCulture);
            if (name.Contains("IEnumerable", StringComparison.InvariantCultureIgnoreCase))
                name = name.Replace("IEnumerable", "Array",true, CultureInfo.InvariantCulture);
            return name;
        }
    }
}