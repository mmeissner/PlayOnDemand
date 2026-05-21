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
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using LeapVR.Content.Shared.Container;
using LeapVR.Shared.Lib.Win;
using LeapVR.Shared.Lib.Win.WinApi.Win32;
using LeapVR.Shell.Categories;
using LeapVR.Utilities.Windows;
using SimpleInjector;
using LeapVR.Shell.Controllers.Behavior;
using LeapVR.Shell.Controllers.Disk;
using LeapVR.Shell.Controllers.Firewall;
using LeapVR.Shell.Controllers.GamePad;
using LeapVR.Shell.Controllers.Interfaces;
using LeapVR.Shell.Controllers.Platform;
using LeapVR.Shell.Controllers.RemoteService;
using LeapVR.Shell.Controllers.RemoteService.Interfaces;
using LeapVR.Shell.Controllers.Security;
using LeapVR.Shell.Controllers.Station;
using LeapVR.Shell.Controllers.Statistics;
using LeapVR.Shell.Controllers.System;
using LeapVR.Shell.Controllers.UserInterface;
using LeapVR.Shell.Controllers.VirtualReality;
using LeapVR.Shell.Domain.Models;
using LeapVR.Shell.Domain.Models.Controllers;
using LeapVR.Shell.Domain.Models.Customization;
using LeapVR.Shell.Domain.Models.Execution;
using LeapVR.Shell.Domain.Models.Language;
using LeapVR.Shell.Domain.Models.Platform;
using LeapVR.Shell.Domain.Models.Station;
using LeapVR.Shell.Domain.Models.System;
using LeapVR.Shell.Domain.Models.UserInterface;
using LeapVR.Shell.FileConfig;
using LeapVR.Shell.Language;
using LeapVR.Shell.Managers.LocalMachine;
using LeapVR.Shell.Managers.UsbStorage;
using LeapVR.Shell.Managers.UsbStorage.Interfaces;
using LeapVR.Shell.Modules;
using LeapVR.Shell.Modules.Container;
using LeapVR.Shell.Modules.FileConfig;
using LeapVR.Shell.Modules.Interfaces;
using LeapVR.Shell.Modules.Interfaces.Multimedia;
using LeapVR.Shell.Modules.Interfaces.Network;
using LeapVR.Shell.Modules.Interfaces.Platform;
using LeapVR.Shell.Modules.Interfaces.Repositories;
using LeapVR.Shell.Modules.Interfaces.Vr;
using LeapVR.Shell.Modules.Interfaces.XInput;
using LeapVR.Shell.Modules.Multimedia;
using LeapVR.Shell.Modules.Network;
using LeapVR.Shell.Modules.Platform.Steam;
using LeapVR.Shell.Modules.Platform.VBox;
using LeapVR.Shell.Modules.Vr;
using LeapVR.Shell.Modules.XInput;
using LeapVR.Shell.Repository;
using LeapVR.Shell.Repository.Interfaces.Interfaces;
using LeapVR.Shell.Services.Factory;
using LeapVR.Shell.UI;
using LeapVR.Shell.UI.Core;
using LeapVR.Shell.UI.Interfaces;
using LeapVR.Shell.UI.Shell.Dashboard.ViewModels;
using LeapVR.Shell.UI.Shell.Login.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Installation.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Management.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.Applications.Platform.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.Hardware.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.Settings.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.Statistics.ViewModels;
using LeapVR.Shell.UI.Shell.SystemAdministration.ViewModels;
using LeapVR.Shell.UI.Shell.ViewModels;
using LeapVR.Shell.UI.Universal.Platform.ViewModels;
using LeapVR.Shell.UI.Universal.StationDetails.ViewModels;
using LeapVR.Shell.UI.Universal.ViewModels;
using Logger = NLog.Logger;
using LogManager = Caliburn.Micro.LogManager;
using LeapVR.Utilities.Steam.Steam;
using IAppInstallationHeaderSerializer = LeapVR.Shell.Domain.Models.Container.IAppInstallationHeaderSerializer;
using PlatformProvider = LeapVR.Shell.UI.Core.PlatformProvider;

namespace LeapVR.Shell
{
    public class Bootstrapper : BootstrapperBase, IApplicationHost
    {
        #region Fields & Properties
        private volatile bool _isTaskbarVisible = true;
        private IStationController _stationController;
        private static readonly Container Container = new Container();
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        #endregion

        #region Constructors
        public Bootstrapper() { Initialize(); }
        #endregion

        #region Bootstrapper Methods
        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            var systemController = Container.GetInstance<ISystemController>();
            systemController.Initialize();
            //To Request Shutdown for actions as Alt+F4 
            _stationController = Container.GetInstance<IStationController>();
        }
        protected override void OnExit(object sender, EventArgs e)
        {
            //If we quit the application by ALT + F4 we need to still properly shutdown
            _stationController?.RequestShutdown();
            //Regardless if its hidden, fire and forget
            ShowTaskbar();
            User32.ShowCursor(true);
            base.OnExit(sender, e);
        }

        protected override void Configure()
        {
            Logger.Info(
                    $"LeapVR.Shell [Version: {VersionProvider.SoftwareVersion}] is starting up...");
            Application.Current.DispatcherUnhandledException += OnApplicationDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            try
            {
                var globalConfiguration = GlobalConfig.GetGlobalConfiguration();
                RegisterHostSystem(globalConfiguration);
                RegisterConfigurations();
                RegisterConcreteRepositories(globalConfiguration);

                RegisterRPCServices();
                RegisterControllers();
                RegisterModules();

                RegisterViewModels();
                ConfigureCaliburnMicro();
                Container.Verify();
            }
            catch(Exception e)
            {
                Logger.Fatal(e, "Error on configuring System on Startup!");
                throw;
            }

            base.Configure();
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
        private IEnumerable<Type> GetTypesFromExecutingAssembly(Type myType)
        {
            var types =
                    from type in Assembly.GetExecutingAssembly().GetTypes()
                    where
                            (type.GetInterface(myType.Name) != null || myType.IsAssignableFrom(type)) &&
                            !type.IsAbstract &&
                            type.IsClass
                    select type;
            return types;
        }
        #endregion

        #region IOC Registration Methods
        private void RegisterHostSystem(IGlobalConfiguration globalConfiguration)
        {
            // Container.RegisterInstance(Container);
            Container.RegisterInstance(typeof(IApplicationHost), this);
            Container.RegisterInstance<IGlobalConfiguration>(globalConfiguration);
            Container.RegisterSingleton<IEventAggregator, EventAggregator>();
            Container.RegisterSingleton<IWindowManager, WindowManager>();
            Container.RegisterSingleton<IUIMessageBroker, UIMessageBroker>();
            Container.RegisterSingleton<ILanguageSelector, LanguageSelector>();
        }

        private void RegisterConcreteRepositories(IGlobalConfiguration globalConfiguration)
        {
            Container.RegisterSingleton<IAppDisplayRepository, AppDisplayRepository>();
            Container.RegisterSingleton<IAppInstallationRepository, AppInstallationRepository>();
            Container.RegisterSingleton<IAppPlatformRepository, AppPlatformRepository>();
            Container.RegisterSingleton<IStoredPackageRepository, StoredPackageRepository>();
            Container.RegisterSingleton<IAppStatisticsRepository, AppStatisticsRepository>();
            Container.RegisterSingleton<IAppPlatformAccountRepository, AppPlatformAccountRepository>();
            Container.RegisterSingleton<IMultimediaSettingsRepository, MultimediaSettingsRepository>();
            Container.RegisterSingleton<IMultimediaPlaylistRepository, MultimediaPlaylistRepository>();

            var openVrModuleConfigRepo = new ConfigFileRepository<OpenVrModuleConfig>();
            Container.RegisterInstance<IOpenVrSettingsSetRepository>(
                    new OpenVrSettingsSetRepository(globalConfiguration, openVrModuleConfigRepo));
        }
        private void RegisterConfigurations()
        {
            Container.RegisterSingleton(typeof(IConfigFileRepository<>), typeof(ConfigFileRepository<>));

            Container.RegisterSingleton(
                    typeof(RpcClientConfig),
                    () => new ConfigFileRepository<RpcClientConfig>().Get());
            Container.RegisterSingleton(
                    typeof(UiConfig),
                    () => new ConfigFileRepository<UiConfig>().Get());
            //File-backed ServerConfig. Operators set Host/Port/optional root-CA in
            //ServerConfig.json next to the kiosk binary; defaults assume a local
            //docker-compose stack on https://localhost.
            Container.RegisterInstance(typeof(IServerConfig), new ConfigFileRepository<ServerConfig>().Get());
            Container.RegisterInstance(
                    typeof(SystemConfig),
                    new ConfigFileRepository<SystemConfig>().Get());
            Container.RegisterInstance(
                    typeof(SecurityConfig),
                    new ConfigFileRepository<SecurityConfig>().Get());
        }
        private void RegisterRPCServices()
        {
            Logger.Info("Registering Services to IoC in << ONLINE >> mode.");
            Container.RegisterSingleton<RemoteServiceFactory>();
            Container.Register<IRemoteServiceSet>(
                    () => Container.GetInstance<RemoteServiceFactory>().GetStationServices(),
                    Lifestyle.Singleton);
        }
        private void RegisterControllers()
        {
            Logger.Info("Registering Controllers to IoC in << ONLINE >> mode.");

            Container.RegisterSingleton<IFirewallController, FirewallController>();
            Container.RegisterSingleton<ISecurityController, SecurityController>();
            Container.RegisterSingleton<ISystemController, SystemController>();
            Container.RegisterSingleton<IRemoteServiceController, RemoteServiceController>();
            Container.RegisterSingleton<IDiskController, DiskController>();
            Container.RegisterSingleton<IBehaviorController, BehaviorController>();

            var regStatisticsController = Lifestyle.Singleton.CreateRegistration<StatisticsController>(Container);
            Container.AddRegistration(typeof(IStatisticsController), regStatisticsController);

            var regPlatformController = Lifestyle.Singleton.CreateRegistration<PlatformController>(Container);
            Container.AddRegistration(typeof(IPlatformController), regPlatformController);

            var regGamePadController = Lifestyle.Singleton.CreateRegistration<GamepadController>(Container);
            Container.AddRegistration(typeof(IGamepadController), regGamePadController);

            var regVrController = Lifestyle.Singleton.CreateRegistration<VirtualRealityController>(Container);
            Container.AddRegistration(typeof(IVirtualRealityController), regVrController);

            var rpcController = Lifestyle.Singleton.CreateRegistration<RemoteServiceController>(Container);
            Container.AddRegistration(typeof(RemoteServiceController), rpcController);

            var stationMessageReceiversCollection =
                    new List<Registration>
                    {
                            regVrController,
                            regGamePadController,
                            regPlatformController,
                            rpcController
                    };

            var regStationController = Lifestyle.Singleton.CreateRegistration<StationController>(Container);
            Container.AddRegistration(typeof(StationController), regStationController);
            Container.AddRegistration(typeof(IStationController), regStationController);


            Container.RegisterCollection<IRunLevelMsgReceiver>(stationMessageReceiversCollection);
            Container.RegisterCollection<IExecutionMessageReceiver>(new[] {regStatisticsController, regVrController});
        }
        private void RegisterModules()
        {
            Container.RegisterSingleton<IGenericCacheProvider, GenericCacheProvider>();
            Container.RegisterSingleton<IMultimediaProvider, MultimediaProvider>();
            Container.RegisterSingleton<IPlaylistModule, PlaylistModule>();

            Container.RegisterSingleton<IAppInfoProcessor, AppInfoProcessor>();
            Container.RegisterSingleton<INetworkModule, NetworkModule>();
            Container.RegisterSingleton<IUsbDevicesManager, UsbStorageManager>();
            Container.RegisterSingleton<ILocalMachine, LocalMachine>();
            Container.RegisterSingleton<ExecutableLibrary>();

            Container.RegisterSingleton<IAppInstallationHeaderSerializer, AppInstallationHeaderSerializer>();
            Container.RegisterSingleton<SteamLib>();

            Container.RegisterSingleton<IContainerModule, ContainerModule>();
            Container.RegisterSingleton<IVrDesktopModule, VrDesktopModule>();


            var xinputModuleRegistration = Lifestyle.Singleton.CreateRegistration<XInputModule>(Container);
            Container.AddRegistration(typeof(IXInputModule), xinputModuleRegistration);

            //Collections of Concrete Singletons with Multi Interfaces
            var vBoxPlatformRegistration = Lifestyle.Singleton.CreateRegistration<VBoxPlatformModule>(Container);
            Container.AddRegistration(typeof(VBoxPlatformModule), vBoxPlatformRegistration);
            var steamPlatformRegistration = Lifestyle.Singleton.CreateRegistration<SteamPlatformModule>(Container);
            Container.AddRegistration(typeof(SteamPlatformModule), steamPlatformRegistration);
            Container.RegisterCollection<IPlatformModule>(new[] {vBoxPlatformRegistration, steamPlatformRegistration});

            var openVrModuleRegistration = Lifestyle.Singleton.CreateRegistration<OpenVrModule>(Container);
            Container.AddRegistration(typeof(IVrModule), openVrModuleRegistration);
            Container.AddRegistration(typeof(IOpenVrModule), openVrModuleRegistration);
            Container.RegisterCollection<IVrModule>(new[] {openVrModuleRegistration});

            var vBoxModuleRegistrations =
                    new[]
                    {
                            xinputModuleRegistration, vBoxPlatformRegistration, steamPlatformRegistration,
                            openVrModuleRegistration
                    };
            Container.RegisterCollection<IBaseModule>(vBoxModuleRegistrations);
        }
        private void RegisterViewModels()
        {
            //Add some needed Conventions for Caliburn Micro to make PasswordBox Binding work
            ConventionManager.AddElementConvention<PasswordBox>(
                    PasswordBoxHelper.BoundPasswordProperty,
                    "Password",
                    "PasswordChanged");

            //Provider and Factories
            Container.RegisterSingleton<ICategoryProvider, CategoryProvider>();
            Container.RegisterSingleton<ViewModelFactory>();
            Container.RegisterSingleton<PlatformProvider>();

            //Views
            Container.RegisterSingleton<IShell, ShellViewModel>();
            Container.RegisterSingleton<ILoginViewModel, LoginViewModel>();
            Container.RegisterSingleton<IDashboardViewModel, DashboardViewModel>();
            Container.RegisterSingleton<IAdministrationViewModel, AdministrationViewModel>();
            Container.RegisterSingleton<KeypadViewModel>();
            Container.RegisterSingleton<AdministrationConductorViewModel>();
            Container.RegisterSingleton<IStatusBarViewModel, StatusBarViewModel>();
            Container.RegisterSingleton<InstallViewModel>();
            Container.RegisterSingleton<UninstallViewModel>();
            Container.RegisterSingleton<UsbSticksBarViewModel>();
            Container.RegisterSingleton<PlatformAccountsViewModel>();


            // widgets
            Container.RegisterSingleton<StationDetailsViewModel>();
            Container.RegisterSingleton<ModeSwitchViewModel>();
            Container.RegisterSingleton<ClockViewModel>();
            Container.RegisterSingleton<ConnectionIndicatorViewModel>();
            Container.RegisterSingleton<LanguageSelectViewModel>();

            //Controller Input Handler
            Container.RegisterSingleton<IViewInputHandler, ViewInputHandler>();

            // register system administration sub tab views
            var types = GetTypesFromExecutingAssembly(typeof(ITabItemSystemScreen));
            var registrationList = new List<Registration>();
            foreach(var type in types)
            {
                var registration = Lifestyle.Singleton.CreateRegistration(type, Container);
                registrationList.Add(registration);
            }

            Container.RegisterCollection(typeof(ITabItemSystemScreen), registrationList);

            // register statistics sub tab views
            var statisticApplications =
                    Lifestyle.Singleton.CreateRegistration<TabItemStatisticsApplicationsViewModel>(Container);
            Container.AddRegistration(typeof(TabItemStatisticsApplicationsViewModel), statisticApplications);
            var statisticGlobal = Lifestyle.Singleton.CreateRegistration<TabItemStatisticsGlobalViewModel>(Container);
            Container.AddRegistration(typeof(TabItemStatisticsGlobalViewModel), statisticGlobal);
            Container.RegisterCollection(
                    typeof(ITabItemStatisticsScreen),
                    new[] {statisticGlobal, statisticApplications});

            // register app management sub tab views
            var managmentAccess = Lifestyle.Singleton.CreateRegistration<TabItemManagementAccessViewModel>(Container);
            Container.AddRegistration(typeof(TabItemManagementAccessViewModel), managmentAccess);
            var itemInstallation = Lifestyle.Singleton.CreateRegistration<TabItemInstallationViewModel>(Container);
            Container.AddRegistration(typeof(TabItemInstallationViewModel), itemInstallation);
            var steamManagement = Lifestyle.Singleton.CreateRegistration<TabItemPlatformViewModel>(Container);
            Container.AddRegistration(typeof(TabItemPlatformViewModel), steamManagement);
            Container.RegisterCollection(
                    typeof(ITabItemAppManagementScreen),
                    new[] {managmentAccess, itemInstallation, steamManagement});
        }
        #endregion

        #region IApplicationHost Interface
        public void ShowGUI()
        {
            var startupOptions = GetStartupOptions();
            dynamic settings = new ExpandoObject();
            if(startupOptions.HasFlag(StartupOptions.RunInWindow))
            {
                settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                settings.Width = 1920d;
                settings.Height = 1080d;
                settings.WindowState = WindowState.Normal;
                settings.WindowStyle = WindowStyle.None;
                settings.AllowsTransparency = true;
                settings.ResizeMode = ResizeMode.CanResizeWithGrip;
                settings.BorderThickness = new Thickness(15);
                settings.BorderBrush = new SolidColorBrush(Colors.White);
            }

            if(startupOptions.HasFlag(StartupOptions.HideTaskBar))
            {
                HideTaskbar();
            }

            if(startupOptions.HasFlag(StartupOptions.HideCursor))
            {
                // throw new NotImplementedException("Not yet refactored");
            }

            DisplayRootViewFor<IShell>(settings);
        }
        public void Restart()
        {
            Logger.Info("Attempting restart");
            Application.Current.Shutdown((int)TerminationSignal.Restart);
        }
        public void Shutdown()
        {
            Logger.Info("Shutting down Application");
            Application.Current.Shutdown();
        }
        public void RequestPoweroff()
        {
            #region Info
            //// By Default the Shutdown will take place after 30 Seconds
            ////if you want to change the Delay try this one
            //Process.Start("shutdown.exe", "-s -t 10");
            ////Replace xx with Seconds example 10,20 etc

            //Process.Start("shutdown.exe", "-r");
            //// By Default the Restart will take place after 30 Seconds
            ////if you want to change the Delay try this one
            //Process.Start("shutdown.exe", "-r -t 10");
            ////Replace xx with Seconds example 10,20 etc

            //Process.Start("shutdown.exe", "-l");
            ////This Code Will Directly Log Off the System Without warnings
            #endregion

            // SHUT DOWN
            Process.Start("shutdown.exe", "-s -t 0");
        }
        #endregion

        #region Private Methods
        private static StartupOptions GetStartupOptions()
        {
            string[] args = Environment.GetCommandLineArgs();
            StartupOptions startupOptions = StartupOptions.HideTaskBar | StartupOptions.HideCursor;
            for(int index = 1; index < args.Length; index += 2)
            {
                if(args[index].ToLowerInvariant().Equals(GlobalConfig.DebugParameterShort) ||
                   args[index].ToLowerInvariant().Equals(GlobalConfig.DebugParameterLong))
                {
                    startupOptions |= StartupOptions.RunInWindow;
                    startupOptions &= ~StartupOptions.HideTaskBar;
                    startupOptions &= ~StartupOptions.HideCursor;
                }
            }

            return startupOptions;
        }

        private void ConfigureCaliburnMicro()
        {
            #region Configure Logging
            // TODO [FH] Caliburn Log cause every time write to the file. very laggy in the UI.
            var caliburnLogConfig = ConfigurationUtil.GetAppSettingsValueByKey("CaliBurnLog");
            if(!string.IsNullOrEmpty(caliburnLogConfig) &&
               bool.TryParse(caliburnLogConfig, out var isCaliburnLogEnabled))
            {
                if(isCaliburnLogEnabled) LogManager.GetLog = type => new NLogLogger(type);
            }
            #endregion
        }
        private void HideTaskbar()
        {
            if(!_isTaskbarVisible) return;
            Taskbar.Hide();
            _isTaskbarVisible = false;
        }
        private void ShowTaskbar()
        {
            if(_isTaskbarVisible) return;

            Taskbar.Show();
            _isTaskbarVisible = true;
        }
        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e);
            _stationController?.RequestShutdown();
        }
        private void OnApplicationDispatcherUnhandledException(
                object sender,
                System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Fatal(e);
            _stationController?.RequestShutdown();
        }
        private void RecordExceptions(object exceptionObject)
        {
            if(!(exceptionObject is Exception exception))
            {
                return;
            }

            if(exception.InnerException != null)
            {
                RecordExceptions(exception.InnerException);
            }

            if(exceptionObject is AggregateException aggregationException)
            {
                foreach(var innerException in aggregationException.InnerExceptions)
                {
                    RecordExceptions(innerException);
                }
            }
            else
            {
                Logger.Error(exception);
            }
        }
        #endregion
    }
}