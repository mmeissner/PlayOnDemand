#region Licence
/****************************************************************
 *  Filename: AppBootstrapper.cs
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
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Modules.Interfaces.Vr;
using LeapVR.Utilities.Steam.Steam;
using System.Reflection;
using NLog;
using SimpleInjector;
using System.Linq;
using System;
using System.Collections.Generic;
using Caliburn.Micro;
using LeapVR.Shell.Modules;
using LeapVR.Content.Creator.UI.ViewModels;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Utilities.Windows;
using LeapVR.Shell.Modules.FileConfig;
using LeapVR.Shell.Modules.Interfaces.Repositories;
using LeapVR.Shell.Modules.Vr;
using LeapVR.Content.Creator;

namespace LeapVR.ContentCreator
{
    public class AppBootstrapper : BootstrapperBase
    {
        //SimpleContainer _container;
        private static readonly Container Container = new Container();

        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public AppBootstrapper()
        {
            Initialize();

            System.Windows.Application.Current.DispatcherUnhandledException += OnApplicationDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        }

        protected override void Configure()
        {
            var globalConfiguration= GlobalConfig.GetGlobalConfiguration();
            Container.RegisterInstance(typeof(IGlobalConfiguration),globalConfiguration);
            Container.RegisterSingleton<SteamLib>();
            Container.RegisterSingleton<ExecutableLibrary>();

            Container.RegisterSingleton<IConfigFileRepository<ContentCreatorConfig>,ConfigFileRepository<ContentCreatorConfig>>();
            Container.RegisterSingleton<IWindowManager, WindowManager>();
            Container.RegisterSingleton<IEventAggregator, EventAggregator>();
            Container.RegisterSingleton<IShell, ShellViewModel>();
            var openVrModuleConfigRepo = new ConfigFileRepository<OpenVrModuleConfig>();
            Container.RegisterInstance<IConfigFileRepository<OpenVrModuleConfig>>(openVrModuleConfigRepo);
            Container.RegisterInstance<IOpenVrSettingsSetRepository>(new OpenVrSettingsSetRepository(globalConfiguration, openVrModuleConfigRepo));
            var registration = Lifestyle.Singleton.CreateRegistration<OpenVrModule>(Container);
            Container.AddRegistration(typeof(IOpenVrModule), registration);
            Container.AddRegistration(typeof(IVrModule), registration);
            Container.RegisterCollection<IVrModule>(new[] { registration });
            Container.Verify();
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
        }

        protected override object GetInstance(Type service, string key)
        {
            if (service == null)
            {
                var typeName =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .Where(x => x.Name.Contains(key))
                        .Select(x => x.AssemblyQualifiedName)
                        .Single();

                service = Type.GetType(typeName);
            }
            return Container.GetInstance(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            IServiceProvider provider = Container;
            Type collectionType = typeof(IEnumerable<>).MakeGenericType(service);
            var services = (IEnumerable<object>)provider.GetService(collectionType);
            return services ?? Enumerable.Empty<object>();
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            RecordExceptions(e.ExceptionObject);
        }
        private void OnApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            RecordExceptions(e.Exception);
        }
        private void RecordExceptions(object exceptionObject)
        {
            var exception = exceptionObject as Exception;
            if (exception == null)
            {
                return;
            }
            if (exception.InnerException != null)
            {
                RecordExceptions(exception.InnerException);
            }

            var aggregationException = exceptionObject as AggregateException;

            if (aggregationException != null)
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
    }
}