using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TpmRcDecoder.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool m_IgnoreNextChange = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string input = e.Parameter as string;
            if (!string.IsNullOrEmpty(input))
            {
                Input.Text = input;

                Decoder decoder = new Decoder();
                Output.Text = decoder.Decode(input);
            }
        }

        private void Decode_Click(object sender, RoutedEventArgs e)
        {
            Decoder decoder = new Decoder();
            Output.Text = decoder.Decode(Input.Text);
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_IgnoreNextChange)
            {
                m_IgnoreNextChange = false;
                return;
            }

            string inStr = Input.Text;
            if (inStr.Trim().StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                inStr = inStr.Trim().Substring(2);
            }
            UInt32 input = 0;
            try
            {
                input = UInt32.Parse(inStr, NumberStyles.HexNumber);
            }
            catch (FormatException)
            { }

            m_IgnoreNextChange = true;
            InputDec.Text = string.Format("{0}", input);
        }

        private void InputDec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_IgnoreNextChange)
            {
                m_IgnoreNextChange = false;
                return;
            }

            UInt32 input = 0;
            try
            {
                input = UInt32.Parse(InputDec.Text.Trim(), NumberStyles.Integer);
            }
            catch (FormatException)
            { }

            m_IgnoreNextChange = true;
            Input.Text = string.Format("{0:x}", input);
        }

        private void MainPage_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                    Decode_Click(sender, e);
                    break;
            }
        }
    }
}
