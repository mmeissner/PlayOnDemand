#region Licence
/****************************************************************
 *  Filename: OnlyApiResponseAndRequestFilterOrdered.cs
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
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Pod.Web.Center.Swagger
{
    public class OnlyApiResponseAndRequestFilterOrdered: IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            var schemas = swaggerDoc.Components?.Schemas;
            if(schemas == null || schemas.Count == 0) return;

            var responses = new List<KeyValuePair<string, IOpenApiSchema>>();
            var requests = new List<KeyValuePair<string, IOpenApiSchema>>();
            var otherModels = new List<KeyValuePair<string, IOpenApiSchema>>();
            foreach(var docDefinition in schemas)
            {
                if(docDefinition.Key.StartsWith("Request"))
                {
                    requests.Add(docDefinition);
                }
                else
                {
                    otherModels.Add(docDefinition);
                }
            }

            var responsesOrdered = responses.OrderBy(pair => pair.Key, StringComparer.InvariantCulture).ToDictionary(x => x.Key, x => x.Value);
            var subModelsOrdered = requests.OrderBy(pair => pair.Key, StringComparer.InvariantCulture).ToArray();
            var otherModelsOrdered = otherModels.OrderBy(pair => pair.Key, StringComparer.InvariantCulture).ToArray();
            foreach(var schema in subModelsOrdered)
            {
                responsesOrdered.Add(schema.Key, schema.Value);
            }

            foreach(var otherModel in otherModelsOrdered)
            {
                responsesOrdered.Add(otherModel.Key, otherModel.Value);
            }
            swaggerDoc.Components.Schemas = responsesOrdered;
        }
    }
}
