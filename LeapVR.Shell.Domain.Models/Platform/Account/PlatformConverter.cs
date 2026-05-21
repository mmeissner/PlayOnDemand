#region Licence
/****************************************************************
 *  Filename: PlatformConverter.cs
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
using System.Text;

namespace LeapVR.Shell.Domain.Models.Platform.Account {
    public static class PlatformConverter
    {
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

        public static void GetPlatformId(Guid platformGuid, out string nameId,out ulong id)
        {
            var byteGuid= platformGuid.ToByteArray();
            nameId = Encoding.ASCII.GetString(byteGuid, 0, 8);
            id = BitConverter.ToUInt64(byteGuid, 8);
        }
        private static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays) {
                Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}