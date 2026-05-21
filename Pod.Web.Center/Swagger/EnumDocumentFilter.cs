#region Licence
/****************************************************************
 *  Filename: EnumDocumentFilter.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Pod.Web.Center.Swagger
{


    /// <summary>
    /// Add enum value descriptions to Swagger
    /// </summary>
    public class EnumDocumentFilter : IDocumentFilter
    {
        /// <inheritdoc />
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // add enum descriptions to result models
            var schemas = swaggerDoc.Components?.Schemas;
            if(schemas != null)
            {
                foreach(var schemaDictionaryItem in schemas)
                {
                    var schema = schemaDictionaryItem.Value;
                    if(schema?.Properties == null) continue;
                    foreach(var propertyDictionaryItem in schema.Properties)
                    {
                        var property = propertyDictionaryItem.Value;
                        if(property is not OpenApiSchema concrete) continue;
                        var propertyEnums = concrete.Enum;
                        //Could be also a list
                        if(propertyEnums == null && concrete.Items is OpenApiSchema itemsSchema)
                        {
                            propertyEnums = itemsSchema.Enum;
                        }
                        if(propertyEnums != null && propertyEnums.Count > 0)
                        {
                            concrete.Description += DescribeEnum(propertyEnums);
                        }
                    }
                }
            }

            if(swaggerDoc.Paths == null || swaggerDoc.Paths.Count <= 0) return;

            // add enum descriptions to input parameters
            foreach(var pathItem in swaggerDoc.Paths.Values)
            {
                DescribeEnumParameters(pathItem.Parameters);

                if(pathItem.Operations == null) continue;
                // head, patch, options, delete left out
                var verbs = new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put };
                foreach(var verb in verbs)
                {
                    if(pathItem.Operations.TryGetValue(verb, out var op) && op != null)
                    {
                        DescribeEnumParameters(op.Parameters);
                    }
                }
            }
        }

        private static void DescribeEnumParameters(IList<IOpenApiParameter> parameters)
        {
            if(parameters == null) return;

            foreach(var param in parameters)
            {
                if(param is not OpenApiParameter concrete) continue;
                var enums = (concrete.Schema as OpenApiSchema)?.Enum;
                if(enums != null && enums.Any())
                {
                    concrete.Description += DescribeEnum(enums.Cast<object>());
                }
            }
        }

        private static string DescribeEnum(IEnumerable<object> enums)
        {
            var enumDescriptions = new List<string>();
            Type type = null;
            foreach(var enumOption in enums)
            {
                if(enumOption == null) continue;
                if(type == null) type = enumOption.GetType();
                //Depends on the registered Json Serializer and Enum Converter for MVC
                if(type.Name == "String")
                {
                    enumDescriptions.Add((string)enumOption);
                }
                else if(type.Name == "Enum")
                {
                    enumDescriptions.Add(
                        $"{Convert.ChangeType(enumOption, type.GetEnumUnderlyingType())} = {Enum.GetName(type, enumOption)}");

                }
                else
                {
                    enumDescriptions.Add(enumOption.ToString());
                }
            }

            return $"{Environment.NewLine}{string.Join(Environment.NewLine, enumDescriptions)}";
        }
    }
}
