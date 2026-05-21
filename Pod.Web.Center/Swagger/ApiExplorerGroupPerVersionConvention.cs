#region Licence
/****************************************************************
 *  Filename: ApiExplorerGroupPerVersionConvention.cs
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
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Pod.Web.Center.Swagger
{
    /// <summary>
    /// This Method applies Conventions for the ApiExplorerGroups that are used by swagger to assign Controller Methods to Swagger Documents
    /// The default implementation inspects ApiDescription.GroupName and returns true if the value is either null OR equal to the requested document name.
    /// </summary>
    public class ApiExplorerGroupPerVersionConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            var controllerNamespace = controller.ControllerType.Namespace; // e.g. "Controllers.V1"
            var apiVersion = controllerNamespace.Split('.').Last().ToLower();
            bool isInternal = false;
            foreach(object attribute in controller.Attributes)
            {
                if(attribute is RouteAttribute route)
                {
                    if(route.Template != null && route.Template.Contains("internal"))
                    {
                        isInternal = true;
                    }
                }
            }

            if(isInternal)
            {
                controller.ApiExplorer.GroupName = apiVersion + "_internal";
            }
            else
            {
                controller.ApiExplorer.GroupName = apiVersion;
            }
        }
    }

    public class LowercaseDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Lowercase all routes, for Swagger, as discussed here:
            //   https://github.com/domaindrivendev/Swashbuckle/issues/834
            // Issue has a reference to the original gist, which can be found here:
            //   https://gist.github.com/smaglio81/e57a8bdf0541933d7004665a85a7b198
            if(swaggerDoc.Paths == null) return;

            var lowered = new OpenApiPaths();
            foreach(var entry in swaggerDoc.Paths)
            {
                lowered.Add(LowercaseEverythingButParameters(entry.Key), entry.Value);
            }
            swaggerDoc.Paths = lowered;
        }

        private static string LowercaseEverythingButParameters(string key)
        {
            return string.Join('/', key.Split('/').Select(x => x.Contains("{") ? x : x.ToLower()));
        }
    }
}
