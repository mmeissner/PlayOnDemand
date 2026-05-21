#region Licence
/****************************************************************
 *  Filename: IZipContainerHeaderSerializer.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  LeapVR
 *  Date          2018-6-4
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System.IO;

namespace LeapVR.Shell.Domain.Models.Container
{
    /// <summary>
    /// Responsible for serialization and deserialization of ZIP type container header (see <see cref="IZipContainerHeader"/>).
    /// </summary>
    public interface IZipContainerHeaderSerializer
    {
        /// <summary>
        /// Deserialize from file.
        /// </summary>
        /// <param name="filePath">Path to serialized file</param>
        /// <returns>Deserialized object</returns>
        IZipContainerHeader LoadFromFile(string filePath);

        /// <summary>
        /// Deserializes <see cref="IZipContainerHeader"/> from given stream.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        IZipContainerHeader LoadFromStream(Stream source);

        /// <summary>
        /// Serialize to file.
        /// </summary>
        /// <param name="filePath">Path to write serialized data</param>
        /// <param name="header">Object to serialize</param>
        /// <returns>Bool indicating success/failure (true/false).</returns>
        bool SaveToFile(string filePath, IZipContainerHeader header);

        /// <summary>
        /// Serializes header to <see cref="destination"/> stream.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        bool SaveToStream(Stream destination, IZipContainerHeader header);
    }
}
