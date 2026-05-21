#region Licence
/****************************************************************
 *  Filename: Program.cs
 *  ----------------------------------------------------------
 *  Author        Martin Meissner
 *  Contributors  FrostHe
 *  Date          2018-2-7
 *  Copyright (c) 2017-2026 Martin Meissner. Originally
 *                authored at VSpace Tech Dev Ltd. as part of the
 *                LeapVR / LeapPlay product; relicensed under the
 *                Apache License 2.0 in the open-source PlayOnDemand
 *                release.
 *
 *  SPDX-License-Identifier: Apache-2.0
 ****************************************************************/
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using LeapVR.Shared.Lib.Win.Classes;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.System;
using LeapVR.Shell.Modules;
using LeapVR.Shell.Modules.Hardware;
using LeapVR.Shell.Modules.ShellConfigurator;
using LeapVR.Shell.Setup;
using NLog;
using Unosquare.FFME;
using Unosquare.FFME.Platform;
using Unosquare.FFME.Shared;

namespace LeapVR.Shell
{
    public static class Program
    {
        
        private static SingleInstanceGuard _singleInstanceGuard;
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        [STAThread]
        public static void Main()
        {
            if(Environment.GetCommandLineArgs().Any(x => x.ToLowerInvariant().Contains(GlobalConfig.RemoteDebuggerParameter)))
            {
                MessageBox.Show(
                        $"Attach your Remote Debugger{Environment.NewLine}And Press OK",
                        "Remote Debugging Helper",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }
            if (!AssureIsOnlyInstance())
            {
                Logger.Info("There is already an Instance of this application running!");
                return;
            }           
            try
            {
                
                AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

                bool isConfig = Environment.GetCommandLineArgs().
                                            Any(x => x.ToLowerInvariant().Contains(GlobalConfig.ConfigParameter));
                bool isUninstall = Environment.GetCommandLineArgs().
                                               Any(x => x.ToLowerInvariant().Contains(GlobalConfig.UninstallParameter));
                bool isDebug = Environment.GetCommandLineArgs().
                                           Any(
                                                   x => x.ToLowerInvariant().
                                                          Contains(GlobalConfig.DebugParameterShort) ||
                                                        x.ToLowerInvariant().Contains(GlobalConfig.DebugParameterLong));
                if(!isUninstall && !isDebug)
                {
                    var splashScreenNo = new Random().Next(1,5);
                    var splashScreen = new SplashScreen($"Resources/Images/Splashscreens/splashscreen_{splashScreenNo}.png");
                    splashScreen.Show(true,true);
                }

                ShellConfigurator shellConfigurator = new ShellConfigurator(new ConfigFileRepository<DiskConfig>());

                if (isConfig || isUninstall || !shellConfigurator.HasValidDiskConfig)
                {
                    var application = isUninstall ? new Setup.App(SetupType.Uninstall) : new Setup.App(SetupType.Config);
                    application.DispatcherUnhandledException += OnApplicationDispatcherUnhandledException;
                    application.InitializeComponent();
                    application.Run();
                }
                else
                {
                    AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
                    var application = new App();
                    application.DispatcherUnhandledException += OnApplicationDispatcherUnhandledException;
                    application.InitializeComponent();
                    application.Run();
                }
            }
           
            catch (Exception e)
            {
                Logger.Fatal( e, "Application Throw and Exception was not handled");
            }
            
            finally
            {
                TryReleaseSingleInstanceGuard();
                try
                {
                    (bool start, string exe, string parameter) startVal = (false,null,null);
                    switch (Environment.ExitCode)
                    {
                        case (int)TerminationSignal.Restart:
                            startVal = GetProcessExecutionInfo();
                            break;
                    }
                    if(startVal.start) Process.Start(startVal.exe, startVal.parameter);
                }
                catch (Exception e)
                {
                    Logger.Warn( e, "Application Throw during attempt of restart");
                }
            }
        }
        private static void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e);
            RecordExceptions(e.ExceptionObject);
        }
        private static void OnApplicationDispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e);
            RecordExceptions(e.Exception);
        }

        private static void RecordExceptions(object exceptionObject)
        {
            if (!(exceptionObject is Exception exception))
            {
                return;
            }
            if (exception.InnerException != null)
            {
                RecordExceptions(exception.InnerException);
            }

            if (exceptionObject is AggregateException aggregationException)
            {
                foreach (var innerException in aggregationException.InnerExceptions)
                {
                    RecordExceptions(innerException);
                }
            }
            else
            {
                Logger.Error(exception);
            }
        }
        private static bool AssureIsOnlyInstance()
        {
            return SingleInstanceGuard.TryAcquire(@"LeapVR.Shell", out _singleInstanceGuard);
        }
        private static void TryReleaseSingleInstanceGuard()
        {
            _singleInstanceGuard?.Dispose();
        }

        private static (bool start, string exe, string parameter) GetProcessExecutionInfo()
        {
            try
            {
                var args = Environment.GetCommandLineArgs();
                string param = "";
                for (int i = 1; i < args.Length; i++)
                {
                    if (i == args.Length)
                    {
                        param = param + args[i];
                        continue;
                    }
                    param = param + args[i] + " ";
                }
                return (true, args[0], param);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return (false, null, null);
            }
        }
    }
}
