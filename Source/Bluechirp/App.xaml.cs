using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Bluechirp.Library.Helpers;
using Bluechirp.Library.Services;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Bluechirp.Core;
using Bluechirp.Services;
using Bluechirp.View;

namespace Bluechirp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            await SetupAppAsync();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter

                if (ClientDataHelper.GetLastUsedProfile() != null)
                {
                    ClientHelper.LoadLastUsedProfile();
                    rootFrame.Navigate(typeof(ShellView), e.Arguments);
                }
                else
                {
                    rootFrame.Navigate(typeof(LoginView), e.Arguments);

                }

                NavService.CreateInstance(rootFrame);
            }

            if (e.PrelaunchActivated == false)
            {
                TryEnablePrelaunch();

                // Ensure the current window is active
            }
            Window.Current.Activate();
        }


        protected async override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                var fullUri = eventArgs.Uri.Query;
                Debug.WriteLine(fullUri);
                await AuthHelper.Instance.FinishOAuth(fullUri);
            }
            else
            {
                Frame rootFrame = Window.Current.Content as Frame;
                await SetupAppAsync();
                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active
                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        //TODO: Load state from previously suspended application
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter

                    if (ClientDataHelper.GetLastUsedProfile() != null)
                    {
                        ClientHelper.LoadLastUsedProfile();
                        rootFrame.Navigate(typeof(ShellView), null);
                    }
                    else
                    {
                        rootFrame.Navigate(typeof(LoginView), null);

                    }

                    NavService.CreateInstance(rootFrame);
                }

                Window.Current.Activate();
            }

        }
        private void TryEnablePrelaunch()
        {
            CoreApplication.EnablePrelaunch(true);
        }

        private async Task SetupAppAsync()
        {
            await ClientDataHelper.StartUpAsync();

            var appView = ApplicationView.GetForCurrentView();
            appView.SetPreferredMinSize(new Size(400, 500));
            ExtendAcrylicIntoTitleBar();
            APIConstants.SetAPIConstants();
            GlobalKeyboardShortcutService.Initialize();
            try
            {
                await CacheService.LoadKeyboardShortcutsContent();
            }
            catch (Exception)
            {

            }
        }

        private void ExtendAcrylicIntoTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}

