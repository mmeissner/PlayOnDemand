#region Licence
/****************************************************************
 *  Filename: StationModels.cs
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
using System.ComponentModel.DataAnnotations;
using Pod.Enums;

namespace Pod.DtoModels
{
    /// <summary>
    /// Create Station Model.
    /// Allows to provide minimum data to create a new station
    /// </summary>
    public class RequestCreateStationDto
    {
        /// <summary>
        /// The name displayed by the station and returned by the API
        /// </summary>
        [Required, MinLength(3), MaxLength(30)]
        public string DisplayName { get; set; }
        /// <summary>
        /// The password for the station to login
        /// </summary>
        [Required, MinLength(10), MaxLength(80)]
        public string Password { get; set; }
    }

    /// <summary>
    /// Password Set Model.
    /// Allows to change the stations password for login  
    /// </summary>
    public class RequestSetStationPasswordDto
    {
        [Required, MinLength(10), MaxLength(80)]
        public string Password { get; set; }
    }

    /// <summary>
    /// Request Model to change the display settings of an Station
    /// </summary>
    public class RequestStationSettingsDto
    {
        /// <summary>
        /// The name displayed by the station and returned by the API
        /// </summary>
        [Required, MinLength(3), MaxLength(15)]
        public string DisplayName { get; set; }

        /// <summary>
        /// The operational mode of the station
        /// </summary>
        [Required]
        public StationControlMode Mode { get; set; }

        /// <summary>
        /// A String as QR Code, must be set when <see cref="StationControlMode.RemoteWithQrCode"/> is used
        /// </summary>
        [MinLength(1),MaxLength(128)]
        public string QrCode { get; set; }
    }

    /// <summary>
    /// Request Model to change the stations QR-Code
    /// This Qr-Code will be displayed if the station is in the operational mode <see cref="StationControlMode.RemoteWithQrCode"/>
    /// </summary>
    public class RequestStationQrCodeDto
    {
        /// <summary>
        /// A string that can be also a Url
        /// </summary>
        [MinLength(1), MaxLength(128)]
        public string QrCode { get; set; }
    }

    /// <summary>
    /// Request Model to change the stations operation mode
    /// </summary>
    public class RequestStationModeDto
    {
        /// <summary>
        /// The mode to change the station to
        /// </summary>
        [Required]
        public StationControlMode Mode { get; set; }
    }

    /// <summary>
    /// Session Request Model for an session
    /// </summary>
    public class RequestNewStationSessionDto
    {
        /// <summary>
        /// An reference for the API consumer to tag sessions and identify them
        /// </summary>
        [MaxLength(50)]
        public string Reference { get; set; }

        /// <summary>
        /// Max duration of the session in the format "HH:MM:SS".
        /// </summary>
        public TimeSpan? Duration { get; set; }
    }

    /// <summary>
    /// Update Session Request Model for an session.
    /// In case the session is already running without an duration limit
    /// the set duration will be accounted from the moment the limit is set.
    /// In case a session had already a duration, the duration will be just updated.
    /// An already set duration limit of an session can not be removed.
    /// </summary>
    public class RequestSessionUpdateDto
    {
        /// <summary>
        /// An reference for the API consumer to tag session updates and identify them
        /// </summary>
        [MaxLength(50)]
        public string Reference { get; set; }
        /// <summary>
        /// The duration in the format "HH:MM:SS".
        /// </summary>
        [Required]
        public TimeSpan Duration { get; set; }

    }
}
