using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TpmRcDecoder
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RcDecoder : Page
    {
        private readonly NavigationHelper m_NavigationHelper;
        private const string m_SettingReturnCode = "rcRC";
        private const string m_InvalidNumberFormat = "Could not parse return code. Please use either decimal or hexadecimal format.\n"
            + "For instance: \"143\" or \"0x8f\"\n";

        public RcDecoder()
        {
            this.InitializeComponent();
            this.m_NavigationHelper = new NavigationHelper(this);
            this.m_NavigationHelper.LoadState += LoadState;
            this.m_NavigationHelper.SaveState += SaveState;
        }

        private void GenerateOutput()
        {
            string inStr = Input.Text.Trim();
            uint input = ~0u;
            if (inStr.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    input = UInt32.Parse(inStr.Substring(2).Trim(), NumberStyles.HexNumber);
                }
                catch (FormatException)
                { }
            }
            else
            {
                try
                {
                    input = UInt32.Parse(inStr, NumberStyles.Integer);
                }
                catch (FormatException)
                { }
            }
            if (input == ~0u)
            {
                Output.Text = m_InvalidNumberFormat;
                return;
            }

            Decoder decoder = new Decoder();
            Output.Text = decoder.Decode(string.Format("{0:x}", input));
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                    Input_TextChanged(sender, null);
                    break;
            }
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            GenerateOutput();
        }

        #region Save and Restore state

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void LoadState(object sender, LoadStateEventArgs e)
        {
            if (SuspensionManager.SessionState.ContainsKey(m_SettingReturnCode))
            {
                Input.Text = (string)SuspensionManager.SessionState[m_SettingReturnCode];
                GenerateOutput();
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache. Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void SaveState(object sender, SaveStateEventArgs e)
        {
            SuspensionManager.SessionState[m_SettingReturnCode] = Input.Text;
        }

        #endregion

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        /// 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.m_NavigationHelper.OnNavigatedTo(e);

            string input = e.Parameter as string;
            if (!string.IsNullOrEmpty(input))
            {
                Input.Text = input;

                Decoder decoder = new Decoder();
                Output.Text = decoder.Decode(input);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.m_NavigationHelper.OnNavigatedFrom(e);
        }

        #endregion

    }
}
