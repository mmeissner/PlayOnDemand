#region Licence
/****************************************************************
 *  Filename: QrCodeLoginSettingsBase.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-3-8
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

namespace LeapVR.Shell.Domain.Models.Billing
{
    /// <summary>
    /// Represents settings for Station in QrCodeLogin Session mode.
    /// </summary>
    public abstract class QrCodeLoginSettingsBase : ISessionSettings
    {
        /// <summary>
        /// URL string to be encoded as QR Code.
        /// </summary>
        public string QrUrl { get; set; }

        public bool Equals(ISessionSettings other)
        {
            var otherQrCodeLoginSetup = other as QrCodeLoginSettingsBase;
            return otherQrCodeLoginSetup != null && otherQrCodeLoginSetup.QrUrl == QrUrl;
        }

        public override string ToString()
        {
            return $"QrCodeLoginSettingsBase: QrUrl={QrUrl}";
        }
    }
}
