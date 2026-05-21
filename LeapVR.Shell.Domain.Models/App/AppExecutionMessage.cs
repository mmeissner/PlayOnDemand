#region Licence
/****************************************************************
 *  Filename: AppExecutionMessage.cs
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
using System.Collections.Generic;
using LeapVR.Shell.Domain.Models.Execution;

namespace LeapVR.Shell.Domain.Models.App
{
    public class AppExecutionMessage
    {

        #region Fields & Properties
        public IApplicationExecution AppExecutionData { get; }
        public ExecutionPhase Phase { get; }
        /// <summary>
        /// Gets or sets a value indicating whether [request termination].
        /// Use it in case of failure
        /// </summary>
        /// <value>
        ///   <c>true</c> if [request termination]; otherwise, <c>false</c>.
        /// </value>
        public bool TerminationRequested { get; private set; }
        public List<TerminationReason> TerminationReasons { get; private set; }

        #endregion

        #region Constructors
        public AppExecutionMessage(IApplicationExecution publisher, ExecutionPhase executionPhase, List<TerminationReason> reasons = null)
        {
            AppExecutionData = publisher;
            Phase = executionPhase;
            if (reasons == null)
            {
                TerminationRequested = false;
                TerminationReasons = new List<TerminationReason>();
            }
            else
            {
                TerminationRequested = true;
                TerminationReasons = reasons;
            }
        }

        #endregion

        #region Methods
        public void RequestTermination(TerminationReason reason)
        {
            TerminationReasons.Add(reason);
            TerminationRequested = true;
        }
        #endregion
    }


    public enum TerminationReason
    {
        None = 0,
        SystemShutdown=1,
        ExceptionOnNext = 10,
        //VR Module/Controller Errors
        VRModuleUnavailible = 100,
        VRModuleOff=101,
    }

}
