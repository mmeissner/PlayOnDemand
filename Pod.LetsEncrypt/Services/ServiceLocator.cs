#region Licence
/****************************************************************
 *  Filename: ServiceLocator.cs
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
namespace Pod.LetsEncrypt.Services
{
    /// <summary>
    /// For Kestrel Configuration to Provide it one point were to get SSL Certificates before
    /// the IOC is useable. This is needed as we need to setup Kestrel before we can configure the IOC 
    /// </summary>
    public static class ServiceLocator
    {
        private static CertificateSelector _certificateSelector;

        /// <summary>
        /// Method for Kestrel to get the Certificate Provider/Selector
        /// </summary>
        /// <returns></returns>
        public static CertificateSelector GetCertificateSelector()
        {
            return _certificateSelector;
        }

        /// <summary>
        /// Allows to setup the Selector during IOC Configuration
        /// </summary>
        /// <param name="certificateSelector"></param>
        internal static void SetCertificateSelector(CertificateSelector certificateSelector)
        {
            _certificateSelector = certificateSelector;
        }
    }
}
