#region Licence
/****************************************************************
 *  Filename: ResultPresenter.cs
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
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Pod.Data.Infrastructure;
using Pod.ViewModels;

namespace Pod.Web.Center.Presenter
{
    public static class ResultPresenter
    {
        public static ActionResult GetResult(IResult result)
        {
            if(result.IsSuccess())return new OkResult();
            return new BadRequestObjectResult(result);
        }
        public static ActionResult GetResult<T>(IResult<T> result)
        {
            if (result.IsSuccess()) return new OkObjectResult(result.ReturnValue);
            return new BadRequestObjectResult((IResult)result);
        }
    }
}