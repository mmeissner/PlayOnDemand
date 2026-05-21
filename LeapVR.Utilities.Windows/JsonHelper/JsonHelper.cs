#region Licence
/****************************************************************
 *  Filename: JsonHelper.cs
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
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeapVR.Utilities.Windows.JsonHelper
{
    public static class JsonHelper
    {
        /// <summary>
        /// Takes <see cref="baseJson"/> string in JSON format and adds/overrides it's properties with <see cref="overrideJson"/>'s string in JSON format.
        /// </summary>
        /// <param name="baseJson"></param>
        /// <param name="overrideJson"></param>
        /// <returns></returns>
        public static string OverrideJsonString(string baseJson, string overrideJson)
        {
            if (string.IsNullOrEmpty(baseJson) || string.IsNullOrEmpty(overrideJson))
            {
                throw new ArgumentException(string.IsNullOrEmpty(baseJson) ? nameof(baseJson) : nameof(overrideJson));
            }

            var baseObj = (JToken)JsonConvert.DeserializeObject(baseJson);
            var overrideObj = (JToken)JsonConvert.DeserializeObject(overrideJson);

            OverrideJsonLogic(baseObj, overrideObj);

            return baseObj.ToString();
        }

        private static void OverrideJsonLogic(JToken baseObj, JToken overrideObj)
        {
            if (baseObj == null || overrideObj == null)
            {
                throw new ArgumentNullException(baseObj == null ? nameof(baseObj) : nameof(overrideObj));
            }

            var overrideObjValue = overrideObj as JValue;
            if (overrideObjValue != null)
            {
                baseObj.Replace(new JValue(overrideObjValue.Value));
                return;
            }

            var overrideObjObject = overrideObj as JObject;
            if (overrideObjObject != null)
            {
                var baseObjObject = baseObj as JObject;
                if (baseObjObject != null)
                {
                    var overrideObjChildren = overrideObjObject.Children().Cast<JProperty>().ToList();
                    var baseObjChildren = baseObjObject.Children().Cast<JProperty>().ToList();
                    foreach (var overrideChild in overrideObjChildren)
                    {
                        var baseChild = baseObjChildren.SingleOrDefault(q => q.Name == overrideChild.Name);
                        if (baseChild != null)
                        {
                            OverrideJsonLogic(baseChild, overrideChild);
                        }
                        else
                        {
                            baseObjObject.Add(overrideChild);
                        }
                    }
                    return;
                }

                baseObj.Replace(overrideObj);
                return;
            }

            var overrideObjProperty = overrideObj as JProperty;
            if (overrideObjProperty != null)
            {
                var baseObjProperty = (JProperty)baseObj; // TODO [RM]: can assume that its always a JProperty?

                var overrideObjPropertyValue = overrideObjProperty.Value;
                var baseObjPropertyValue = baseObjProperty.Value;

                OverrideJsonLogic(baseObjPropertyValue, overrideObjPropertyValue);
                return;
            }

            var overrideObjArray = overrideObj as JArray;
            if (overrideObjArray != null)
            {
                // TODO [RM]: arrays are tricky; Requeires to specify override strategy. How to deal with duplicate JValues? How to deal with `duplicate` nested arrays and objects?
                // TODO [RM]: For now values are not duplicated; nested arrays/objects are duplicated if occures in both baseObjArray and overrideObjArray.

                var baseObjArray = baseObj as JArray;
                if (baseObjArray != null)
                {
                    var overrideObjChildren = overrideObjArray.Children().ToList();
                    var baseObjChildren = baseObjArray.Children().ToList();

                    foreach (var overrideChild in overrideObjChildren)
                    {
                        var overrideChildValue = overrideChild as JValue;
                        if (overrideChildValue != null)
                        {
                            var isValuePresentInBaseObjArray = baseObjChildren
                                .Where(q => q is JValue)
                                .Cast<JValue>()
                                .Any(q => q.Value.Equals(overrideChildValue.Value));

                            if (isValuePresentInBaseObjArray)
                            {
                                continue;
                            }
                        }

                        baseObjArray.Add(overrideChild);
                    }

                    return;
                }

                baseObj.Replace(overrideObj);
                return;
            }
        }
    }
}
