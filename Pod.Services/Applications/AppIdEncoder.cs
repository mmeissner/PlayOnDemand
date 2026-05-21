#region Licence
/****************************************************************
 *  Filename: AppIdEncoder.cs
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
using Pod.Enums;

namespace Pod.Services.Applications
{
    /// <summary>
    /// Encoder/Decoder for AppId's Guids with information about the platform
    /// </summary>
    public static class AppIdEncoder
    {
        private const string SteamNameId = "STEAM___";
        private const string EpicNameId = "EPIC____";
        private const string GogNameId = "GOG_____";
        private const string UbiNameId = "UBI_____";
        private const string OriginNameId = "ORIGIN__";
        private static readonly Dictionary<string,PlatformType> IdentifierEnumMapping =
                new Dictionary<string,PlatformType>()
                {
                        {SteamNameId,PlatformType.Steam},
                        {EpicNameId,PlatformType.EpicGames},
                        {GogNameId,PlatformType.GoodOldGames},
                        {UbiNameId,PlatformType.UbiSoft},
                        {OriginNameId,PlatformType.EaOrigin}
                };

        /// <summary>
        /// Encodes an NameId in ASCII with a ulong into a Guid
        /// </summary>
        /// <param name="nameId">The Name id with max 8 chars as otherwise truncated</param>
        /// <param name="id">The id to encode</param>
        /// <returns>The resulting Guid</returns>
        public static Guid CreateGuid(string nameId, ulong id)
        {
            string resultId;
            if(nameId.Length > 8)
            {
                resultId = nameId.Substring(0, 8);
            }
            else if(nameId.Length < 8)
            {
                int missingChars = 8 - nameId.Length;
                resultId = nameId;
                for(int i = 0; i < missingChars; i++)
                {
                    resultId = resultId + "_";
                }
            }
            else
            {
                resultId = nameId;
            }

            var ascii = Encoding.ASCII.GetBytes(resultId);
            byte[] appId = BitConverter.GetBytes(id);
            var guidArray = Combine(ascii, appId);
            return new Guid(guidArray);
        }

        /// <summary>
        /// Decodes an PlatformType and an id from an Application Guid
        /// </summary>
        /// <param name="applicationId">The Application Guid</param>
        /// <param name="id">The encoded Id</param>
        /// <returns>The PlatformType</returns>
        public static PlatformType DecodeGuid(this Guid applicationId, out ulong id)
        {
            GetPlatformId(applicationId,out var nameId, out id);
            return ToPlatformType(nameId);
        }
        
        /// <summary>
        /// Gets a PlatformType from a string
        /// </summary>
        /// <param name="identifier">The identifier string</param>
        /// <returns></returns>
        private static PlatformType ToPlatformType(this string identifier)
        {
            if(IdentifierEnumMapping.TryGetValue(identifier, out var enumValue))
            {
                return enumValue;
            }
            return PlatformType.Local;
        }

        /// <summary>
        /// Encodes from a Guid the Platform Identifier as string and the id
        /// </summary>
        /// <param name="platformGuid">The Guid with the encoded information</param>
        /// <param name="nameId">The PlatformId as string</param>
        /// <param name="id">The App Id as ulong</param>
        private static void GetPlatformId(Guid platformGuid, out string nameId, out ulong id)
        {
            var byteGuid = platformGuid.ToByteArray();
            nameId = Encoding.ASCII.GetString(byteGuid, 0, 8);
            id = BitConverter.ToUInt64(byteGuid, 8);
        }

        /// <summary>
        /// Combines multiple Byte Arrays
        /// </summary>
        /// <param name="arrays">The byte arrays to combine</param>
        /// <returns></returns>
        private static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach(byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }

            return rv;
        }
    }
}