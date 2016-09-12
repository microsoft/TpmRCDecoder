using System;
using System.Linq;
using TpmRcDecoder.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TpmRcDecoder
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private Frame rootFrame;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary> 
        /// Both the OnLaunched and OnActivated event handlers need to make sure the root frame has been created, so the common  
        /// code to do that is factored into this method and called from both. 
        /// </summary> 
        private void EnsureRootFrame(ApplicationExecutionState previousExecutionState, string arguments)
        {
            AppShell shell = Window.Current.Content as AppShell;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (shell == null)
            {
                // Create a AppShell to act as the navigation context and navigate to the first page
                shell = new AppShell();

                // Set the default language
                shell.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                shell.AppFrame.NavigationFailed += OnNavigationFailed;

                if (previousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }
            }

            // Place our app shell in the current Window
            Window.Current.Content = shell;

            if (shell.AppFrame.Content == null)
            {
                ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
                const string DontShowLandingPageSetting = "DontShowLandingPage";

                if (roamingSettings.Values[DontShowLandingPageSetting] != null &&
                    roamingSettings.Values[DontShowLandingPageSetting].Equals(true.ToString()))
                {
                    // When the navigation stack isn't restored, navigate to the first page
                    // suppressing the initial entrance animation. Because landing page is disabled,
                    // got to first content page.
                    shell.AppFrame.Navigate(typeof(RcDecoder), arguments, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
                }
                else
                {
                    // When the navigation stack isn't restored, navigate to the first page
                    // suppressing the initial entrance animation.
                    shell.AppFrame.Navigate(typeof(LandingPage), arguments, new Windows.UI.Xaml.Media.Animation.SuppressNavigationTransitionInfo());
                }
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            // Change minimum window size 
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(320, 600));

            // Darken the window title bar using a color value to match app theme
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (titleBar != null)
            {
                Color titleBarColor = (Color)App.Current.Resources["SystemChromeMediumColor"];
                titleBar.BackgroundColor = titleBarColor;
                titleBar.ButtonBackgroundColor = titleBarColor;
            }

            EnsureRootFrame(e.PreviousExecutionState, e.Arguments);

            /* no voice controls for now
            // The app must install its command sets at least once. Doing this in OnLaunched 
            // causes it to happen as infrequently as possible. 
            try
            {
                var storageFile = await Package.Current.InstalledLocation.GetFileAsync(@"VoiceCommands.xml");
                await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.
                    InstallCommandDefinitionsFromStorageFileAsync(storageFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Installing Voice Commands Failed: " + ex.ToString());
            }
            */

            // dynamically update phrase list will all possible TPM error codes
            /*
            Windows.ApplicationModel.VoiceCommands.VoiceCommnadDefinition.VoiceCommandSet commandSetEnUs;

            if (Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.
                    InstalledCommandSets.TryGetValue(
                    "AdventureWorksCommandSet_en-us", out commandSetEnUs))
            {
                await commandSetEnUs.SetPhraseListAsync(
                "destination", new string[] {“London”, “Dallas”, “New York”, “Phoenix”});
            }
            */
        }

        // <summary> 
        // Invoked when the application is activated.
        // </summary> 
        // <param name = "e" > Details about the launch request and process.</param> 
        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind != ActivationKind.VoiceCommand)
            {
                return;
            }

            var commandArgs = args as VoiceCommandActivatedEventArgs;
            SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

            // The commandMode is either "voice" or "text", and it indicates how the voice command was entered by the user. 
            // We should respect "text" mode by providing feedback in a silent form. 
            string commandMode = this.SemanticInterpretation("commandMode", speechRecognitionResult);

            string voiceCommand = speechRecognitionResult.RulePath[0];
            string textSpoken = speechRecognitionResult.Text;

            switch (voiceCommand)
            {
                case "decodeReturnCode":
                    textSpoken = SemanticInterpretation("errorCodeSearch", speechRecognitionResult);
                    // nothing
                    break;

                default:
                    textSpoken = "";
                    break;
            }

            EnsureRootFrame(args.PreviousExecutionState, "");
            if (!this.rootFrame.Navigate(typeof(RcDecoder), textSpoken))
            {
                throw new Exception("Failed to create voice command page");
            }
        }

        private string SemanticInterpretation(string interpretationKey, SpeechRecognitionResult speechRecognitionResult)
        {
            if (speechRecognitionResult.SemanticInterpretation.Properties.ContainsKey(interpretationKey))
            {
                return speechRecognitionResult.SemanticInterpretation.Properties[interpretationKey].FirstOrDefault();
            }
            else
            {
                return "";
            }
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
