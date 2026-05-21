#region Licence
/****************************************************************
 *  Filename: IdentificationServiceClientOut.cs
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
using System.Text;
using System.Threading.Tasks;
using LeapVR.VBox.Controllers.Interfaces.Services;
using LeapVR.VBox.ObjectModel.Interfaces.Identification;

namespace LeapVR.VBox.Services
{
    public class IdentificationServiceClientOut : IIdentificationServiceClientOut
    {
        #region Properties & Fields

        //

        #endregion Properties & Fields

        #region Constructors

        //

        #endregion Constructors

        #region Methods

        public ILoginResult AttemptLogin(ILoginAttempt attempt)
        {
            throw new NotImplementedException();
        }

        public Task<ILoginResult> AttemptLoginAsync(ILoginAttempt attempt)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
