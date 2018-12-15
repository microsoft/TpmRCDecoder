using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TpmRcDecoder
{
    public struct TPMManufacturerDescription
    {
        public uint ID;
        public string Name;
        public string Description;

        public TPMManufacturerDescription(uint id, string name, string description)
        {
            ID = id;
            Name = name;
            Description = description;
        }
    };

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Manufacturers : Page
    {
        private readonly NavigationHelper m_NavigationHelper;
        private const string m_SettingManufacturerCode = "mfMF";
        private const string m_DefaultDescription = "TPM manufaturer description.";
        private const string m_InvalidCommandDescription = "Invalid manufacturer ID.";
        private static TPMManufacturerDescription[] m_Manufacturers =
        {
            new TPMManufacturerDescription( 0x414D4400, "AMD", "AMD"),
            new TPMManufacturerDescription( 0x41544D4C, "ATML", "Atmel"),
            new TPMManufacturerDescription( 0x4252434D, "BRCM", "Broadcom" ),
            new TPMManufacturerDescription( 0x48504500, "HPE", "HPE" ),
            new TPMManufacturerDescription( 0x49424D00, "IBM", "IBM" ),
            new TPMManufacturerDescription( 0x49465800, "IFX", "Infineon" ),
            new TPMManufacturerDescription( 0x494E5443, "INTC", "Intel" ),
            new TPMManufacturerDescription( 0x4C454E00, "LEN", "Lenovo" ),
            new TPMManufacturerDescription( 0x4D534654, "MSFT", "Microsoft" ),
            new TPMManufacturerDescription( 0x4E534D20, "NSM ", "National Semiconductor" ),
            new TPMManufacturerDescription( 0x4E545A00, "NTZ", "NationZ" ),
            new TPMManufacturerDescription( 0x4E544300, "NTC", "Nuvoton" ),
            new TPMManufacturerDescription( 0x51434F4D, "QCOM", "QCOM" ),
            new TPMManufacturerDescription( 0x534D5343, "SMSC", "SMSC" ),
            new TPMManufacturerDescription( 0x53544D20, "STM", "ST Microelectronics" ),
            new TPMManufacturerDescription( 0x534D534E, "SMSN", "Samsung" ),
            new TPMManufacturerDescription( 0x534E5300, "SNS", "Sinosun" ),
            new TPMManufacturerDescription( 0x54584E00, "TXN", "Texas Instruments" ),
            new TPMManufacturerDescription( 0x57454300, "WEC", "Winbond" ),
            new TPMManufacturerDescription( 0x524F4343, "ROCC", "Fushou Rockchip" ),
            new TPMManufacturerDescription( 0x474F4F47, "GOOG", "Google" ),
            new TPMManufacturerDescription( 0x564D5700, "VMW", "VMWare" ),
        };

        public class ManufacturerDescriptionComparer : IComparer<TPMManufacturerDescription>
        {
            public int Compare(TPMManufacturerDescription left, TPMManufacturerDescription right)
            {
                return left.ID.CompareTo(right.ID);
            }
        }

        public Manufacturers()
        {
            this.InitializeComponent();
            this.m_NavigationHelper = new NavigationHelper(this);
            this.m_NavigationHelper.LoadState += LoadState;
            this.m_NavigationHelper.SaveState += SaveState;

            Array.Sort(m_Manufacturers, new ManufacturerDescriptionComparer());

            ListOfManufacturers.SelectionChanged -= ListOfManufacturers_SelectionChanged;
            ListOfManufacturers.Items.Clear();
            foreach (TPMManufacturerDescription descr in m_Manufacturers)
            {
                ListOfManufacturers.Items.Add(descr.Name);
            }
            ListOfManufacturers.SelectionChanged += ListOfManufacturers_SelectionChanged;

            ManufacturerCode.Text = "";
            Description.Text = m_DefaultDescription;

        }

        private void SetIndex(int index, UInt32 ManufacturerCode, string commandName, string description)
        {
            bool setNotification = ListOfManufacturers.SelectedIndex != index;
            if (setNotification)
                ListOfManufacturers.SelectionChanged -= ListOfManufacturers_SelectionChanged;
            ListOfManufacturers.SelectedIndex = index;
            Description.Text = string.Format("{0} (0x{1:x} - {1})\n\n", commandName, ManufacturerCode) + description;
            if (setNotification)
                ListOfManufacturers.SelectionChanged += ListOfManufacturers_SelectionChanged;
        }

        private void ManufacturerCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inStr = ManufacturerCode.Text.Trim();
            uint manufacturerCode = ~0u;
            if (inStr.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                try
                {
                    manufacturerCode = UInt32.Parse(inStr.Substring(2).Trim(), NumberStyles.HexNumber);
                }
                catch (FormatException)
                { }
            }
            else
            {
                try
                {
                    manufacturerCode = UInt32.Parse(inStr, NumberStyles.Integer);
                }
                catch (FormatException)
                { }
            }
            if (manufacturerCode == ~0u)
            {
                SetIndex(-1, 0, "", m_InvalidCommandDescription);
                return;
            }

            int index = 0;
            foreach (TPMManufacturerDescription descr in m_Manufacturers)
            {
                if (manufacturerCode == descr.ID)
                {
                    SetIndex(index, descr.ID, descr.Name, descr.Description);
                    return;
                }
                index++;
            }

            // no matching command found, set defaults
            SetIndex(-1, 0, "", m_InvalidCommandDescription);
        }

        private void ListOfManufacturers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfManufacturers.SelectedIndex == -1)
            {
                return;
            }

            uint manufacturerCode = ~0u;
            string description = "";
            string commandName = "";
            if (ListOfManufacturers.SelectedIndex < m_Manufacturers.Length)
            {
                manufacturerCode = m_Manufacturers[ListOfManufacturers.SelectedIndex].ID;
                description = m_Manufacturers[ListOfManufacturers.SelectedIndex].Description;
                commandName = m_Manufacturers[ListOfManufacturers.SelectedIndex].Name;
            }

            ManufacturerCode.TextChanged -= ManufacturerCode_TextChanged;
            ManufacturerCode.Text = string.Format("0x{0:x}", manufacturerCode);
            Description.Text = string.Format("{0} (0x{1:x} - {1})\n\n", commandName, manufacturerCode) + description;
            ManufacturerCode.TextChanged += ManufacturerCode_TextChanged;
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
            if (SuspensionManager.SessionState.ContainsKey(m_SettingManufacturerCode))
            {
                ManufacturerCode.Text = (string)SuspensionManager.SessionState[m_SettingManufacturerCode];
                // ListOfManufacturers is automatically set by ManufacturerCode's OnChanged method
                // Description is automatically set by ManufacturerCode's OnChanged method
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
            SuspensionManager.SessionState[m_SettingManufacturerCode] = ManufacturerCode.Text;
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
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.m_NavigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
