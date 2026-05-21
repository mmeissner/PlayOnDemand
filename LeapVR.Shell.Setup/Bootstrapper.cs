#region Licence
/****************************************************************
 *  Filename: Bootstrapper.cs
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
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using LeapVR.Shell.Categories;
using LeapVR.Shell.Controllers.Disk;
using LeapVR.Shell.Controllers.Firewall;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.Platform;
using LeapVR.Shell.Controllers.Security;
using LeapVR.Shell.Controllers.System;
using LeapVR.Shell.Controllers.UserInterface;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.Language;
using LeapVR.Shell.Modules;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.Modules.Platform.VBox;
using LeapVR.Shell.Modules.ShellConfigurator;
using LeapVR.Shell.Repository;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using LeapVR.Shell.Setup.UI.ViewModels;
using SimpleInjector;
using Container = SimpleInjector.Container;
using Logger = NLog.Logger;

namespace LeapVR.Shell.Setup
{
    public class Bootstrapper : BootstrapperBase
    {
        private static readonly Container Container = new Container();
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly App SetupApp;

        static Bootstrapper()
        {
            SetupApp = (App)Application.Current;
        }
        public Bootstrapper()
        {
            Initialize();
            SetupApp.DispatcherUnhandledException += OnApplicationDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        }

        protected override void Configure()
        {
            if(SetupApp.Type == SetupType.Config)
            {
                SetupContainerForConfig(Container);
            }
            else
            {
                SetupContainerForUninstall(Container);
            }
        }

        private void SetupContainerForConfig(Container container)
        {            
            Container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton<SetupHelper, SetupHelper>();
            container.RegisterSingleton<IWindowManager, WindowManager>();
            container.RegisterSingleton<ConfigViewModel, ConfigViewModel>();
            container.RegisterSingleton<SettingsWizViewModel, SettingsWizViewModel>();
            container.RegisterSingleton<RegisterAccountViewModel, RegisterAccountViewModel>();
            container.RegisterSingleton<IConfigFileRepository<DiskConfig>, ConfigFileRepository<DiskConfig>>();
            container.RegisterSingleton<IConfigFileRepository<LoginConfig>, ConfigFileRepository<LoginConfig>>();
            container.RegisterSingleton<IConfigFileRepository<SystemConfig>, ConfigFileRepository<SystemConfig>>();
            container.Register(() =>Container.GetInstance<IConfigFileRepository<SystemConfig>>().Get());
            container.RegisterSingleton<IUIMessageBroker, UIMessageBroker>();
            container.RegisterSingleton<ILanguageSelector, LanguageSelector>();
            container.RegisterSingleton<ShellConfigurator, ShellConfigurator>();
        }
        private void SetupContainerForUninstall(Container container)
        {
            //Dummies that do not have a real implementation but are required for dependency reasons
            
            container.RegisterSingleton<ICategoryProvider, CategoryProviderDummy>();
            container.RegisterSingleton<IAppInfoProcessor, AppInfoProcessorDummy>();
            container.RegisterSingleton<IVirtualRealityController, VirtualRealityControllerDummy>();
            container.RegisterSingleton<ILocalMachine, LocalMachineDummy>();

            container.RegisterSingleton<IWindowManager, WindowManager>();
            Container.RegisterSingleton<IEventAggregator, EventAggregator>();
            container.RegisterSingleton<IConfigFileRepository<SystemConfig>, ConfigFileRepository<SystemConfig>>();
            container.Register(() =>Container.GetInstance<IConfigFileRepository<SystemConfig>>().Get());
            container.RegisterSingleton<SetupHelper, SetupHelper>();
            container.RegisterSingleton<IConfigFileRepository<DiskConfig>, ConfigFileRepository<DiskConfig>>();
            container.RegisterSingleton<IAppDisplayRepository, AppDisplayRepository>();
            container.RegisterSingleton<IAppInstallationRepository, AppInstallationRepository>();
            Container.RegisterSingleton<IAppPlatformAccountRepository, AppPlatformAccountRepository>();
            container.RegisterSingleton<IAppPlatformRepository, AppPlatformRepository>();
            container.RegisterSingleton<IStoredPackageRepository, StoredPackageRepository>();
            container.RegisterSingleton<IUIMessageBroker, UIMessageBroker>();
            container.RegisterSingleton<IFirewallController, FirewallController>();
            container.RegisterSingleton<ISecurityController, SecurityController>();
            container.RegisterSingleton<ISystemController, SystemController>();
            container.RegisterSingleton<IDiskController, DiskController>();
            container.RegisterSingleton<IPlatformController, PlatformController>();
            //Collections of Concrete Singletons with Multi Interfaces
            var vBoxPlatformRegistration = Lifestyle.Singleton.CreateRegistration<VBoxPlatformModule>(container);
            container.AddRegistration(typeof(IPlatformModule), vBoxPlatformRegistration);
            container.RegisterCollection<IPlatformModule>(new[] {vBoxPlatformRegistration});
        }
        protected override object GetInstance(Type service, string key)
        {
            if(service == null)
            {
                var typeName =
                        Assembly.GetExecutingAssembly().
                                 GetTypes().
                                 Where(x => x.Name.Contains(key)).
                                 Select(x => x.AssemblyQualifiedName).
                                 Single();

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
        protected override void OnStartup(object sender, StartupEventArgs e)
        {

            switch(((App)Application.Current).Type)
            {
                case SetupType.Config:
                    dynamic configWindowSettings = new ExpandoObject();
                    configWindowSettings.WindowStyle = WindowStyle.SingleBorderWindow;
                    configWindowSettings.MinHeight = 600;
                    configWindowSettings.MinWidth = 600;
                    configWindowSettings.Title = "Leap Configuration";
                    configWindowSettings.Topmost = true;
                    configWindowSettings.SizeToContent = SizeToContent.WidthAndHeight;
                    configWindowSettings.ResizeMode = ResizeMode.NoResize;
                    configWindowSettings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    DisplayRootViewFor<ConfigViewModel>(configWindowSettings);
                    break;
                case SetupType.Uninstall:
                    var uninstaller = IoC.Get<Uninstaller>();
                    var retval = uninstaller.StartUninstall();
                    Application.Shutdown(retval);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            RecordExceptions(e.ExceptionObject);
        }
        private void OnApplicationDispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
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
    }
}
