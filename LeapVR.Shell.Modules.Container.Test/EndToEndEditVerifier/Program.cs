#region Licence
/****************************************************************
 *  Filename: Program.cs
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using LeapVR.Content.Creator.Logic;
using LeapVR.Content.Shared.Container;
using LeapVR.Shell.Modules.Container;

namespace EndToEndEditVerifier
{
    /// <summary>
    /// Console front-end that runs the same operation the Content Creator
    /// WPF edit wizard performs: open .vbox -> mutate description -> save ->
    /// reopen -> verify persistence + game-file byte-identity.
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine(
                    "usage: EndToEndEditVerifier <path-to-vbox>");
                return 2;
            }
            var vbox = args[0];
            if (!File.Exists(vbox))
            {
                Console.Error.WriteLine("FAIL: file not found: " + vbox);
                return 1;
            }

            try
            {
                return Run(vbox);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("FAIL: " + e);
                return 1;
            }
        }

        private static int Run(string vbox)
        {
            var serializer = new AppInstallationHeaderSerializer();
            var module = new ContainerModule(serializer);

            // ---- pre-mutation snapshot ----------------------------------
            long metadataOffsetBefore;
            {
                var probe = module.OpenForEdit(vbox);
                if (probe.Metadata == null)
                {
                    Console.Error.WriteLine(
                        "FAIL: .vbox has no Metadata package; cannot be edited.");
                    return 1;
                }
                metadataOffsetBefore = probe.Metadata.FileOffset;
            }
            string hashBefore = HashFileRange(vbox, 0, metadataOffsetBefore);
            var fileLengthBefore = new FileInfo(vbox).Length;

            Console.WriteLine("[before]");
            Console.WriteLine("  file size:        " + fileLengthBefore + " bytes");
            Console.WriteLine("  metadata offset:  " + metadataOffsetBefore);
            Console.WriteLine("  pre-meta sha256:  " + hashBefore);

            // ---- open via the wizard's editor ---------------------------
            var editor = new LeapVrContainerEditor(vbox);
            if (!editor.IsValid)
            {
                Console.Error.WriteLine("FAIL: editor reports invalid: "
                    + editor.OccuredException);
                return 1;
            }

            var originalDesc = editor.DisplayData.Description;
            var newDesc = "[edited-by-EndToEndEditVerifier at "
                + DateTime.UtcNow.ToString("O") + "]";

            Console.WriteLine("[open]");
            Console.WriteLine("  ApplicationGuid:  " + editor.ApplicationGuid);
            Console.WriteLine("  DisplayName:      " + editor.DisplayName);
            Console.WriteLine("  Old description:  " + originalDesc);

            // ---- mutate (this is what the WPF TwoWay binding does) ------
            editor.DisplayData.Description = newDesc;

            // ---- save (this is what wizard's Create/Save action calls) --
            editor.Save();

            Console.WriteLine("[save]");
            Console.WriteLine("  New description:  " + newDesc);

            // ---- post-save verification ---------------------------------
            long metadataOffsetAfter;
            {
                var probe2 = module.OpenForEdit(vbox);
                metadataOffsetAfter = probe2.Metadata.FileOffset;
            }
            string hashAfter = HashFileRange(vbox, 0, metadataOffsetBefore);
            var fileLengthAfter = new FileInfo(vbox).Length;

            Console.WriteLine("[after]");
            Console.WriteLine("  file size:        " + fileLengthAfter + " bytes");
            Console.WriteLine("  metadata offset:  " + metadataOffsetAfter);
            Console.WriteLine("  pre-meta sha256:  " + hashAfter);

            bool offsetOk = metadataOffsetBefore == metadataOffsetAfter;
            bool hashOk = hashBefore == hashAfter;

            // ---- reopen and verify persisted ----------------------------
            var reopened = new LeapVrContainerEditor(vbox);
            if (!reopened.IsValid)
            {
                Console.Error.WriteLine("FAIL: reopen invalid: "
                    + reopened.OccuredException);
                return 1;
            }
            var persistedDesc = reopened.DisplayData.Description;

            Console.WriteLine("[reopen]");
            Console.WriteLine("  Read description: " + persistedDesc);

            bool persistedOk = string.Equals(persistedDesc, newDesc,
                StringComparison.Ordinal);

            Console.WriteLine();
            Console.WriteLine("checks:");
            Console.WriteLine("  metadata offset unchanged: " + offsetOk);
            Console.WriteLine("  pre-metadata bytes equal:  " + hashOk);
            Console.WriteLine("  description persisted:     " + persistedOk);

            if (offsetOk && hashOk && persistedOk)
            {
                Console.WriteLine();
                Console.WriteLine("PASS");
                return 0;
            }
            Console.WriteLine();
            Console.WriteLine("FAIL");
            return 1;
        }

        private static string HashFileRange(string path, long from, long count)
        {
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
            using (var sha = SHA256.Create())
            {
                fs.Seek(from, SeekOrigin.Begin);
                var buffer = new byte[64 * 1024];
                long remaining = count;
                while (remaining > 0)
                {
                    int want = (int)Math.Min(remaining, buffer.Length);
                    int got = fs.Read(buffer, 0, want);
                    if (got == 0) break;
                    sha.TransformBlock(buffer, 0, got, null, 0);
                    remaining -= got;
                }
                sha.TransformFinalBlock(new byte[0], 0, 0);
                return BitConverter.ToString(sha.Hash).Replace("-", "");
            }
        }
    }
}
