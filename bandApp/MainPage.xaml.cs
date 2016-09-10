using System;
using System.Collections.Generic;
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

//Color
using Windows.UI;

//Screen Size
using Windows.UI.ViewManagement;
using Windows.UI.Popups;

// Change Ellipse color
using Windows.UI.Xaml.Shapes;
using Microsoft.Band;

// Use to return task and await task
using System.Threading.Tasks;


namespace bandApp
{
    public sealed partial class MainPage : Page
    {
        // Global variables
        static bool m_bIsConnected = false;
        static bool m_bInProgress = false;
        Dictionary<string, Brush> m_dctLocalTheme = new Dictionary<string, Brush>();
        Dictionary<string, Shape> m_dctGridEllipseMapping = new Dictionary<string, Shape>();

        // Global variables for the connected Band.
        private IBandClient bandClient;
        private IBandInfo band;

        public MainPage()
        {  
            this.InitializeComponent();
            fillGridEllipseMapping();
            
            // Set the minimum size of the Application window and grid.
            // I have not implemented RelativePanel, therefore windows is fixed size.
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(380, 600));
            rootGrid.MaxWidth = 360;
            rootGrid.Height = 500;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the title bar color to match the base color of the band.
            setTitleBarColor();
            await connectToBand();
            saveThemeLocally();
        }

        /* This method populates the dictionary which maps the grids to respective ellipses
         * The motive behind having this key value pair is to fill the correct ellipse, when the respective
         * Grid is tapped.
         */
        private void fillGridEllipseMapping()
        {
            m_dctGridEllipseMapping.Add("gBase", eBase);
            m_dctGridEllipseMapping.Add("gHighContrast", eHighContrast);
            m_dctGridEllipseMapping.Add("gLowLight", eLowLight);
            m_dctGridEllipseMapping.Add("gHighlight", eHighlight);
            m_dctGridEllipseMapping.Add("gMuted", eMuted);
            m_dctGridEllipseMapping.Add("gSecondary", eSecondary);
        }

        /* This is the main method to connect to the Band.
         * It detects the available Bands and connects to the first one.
         */
        private async Task<bool> connectToBand()
        {
            // Get the list of paired Bands
            startBandTask("Finding a Band");
            var pairedBands = await BandClientManager.Instance.GetBandsAsync();
            // Connect to the first Band
            band = pairedBands.FirstOrDefault();
            
            // No Band found
            if (band == null)
            {
                m_bIsConnected = false;
                endBandTask("No Band found");
            }
            // If Band found
            else
            {
                try
                {
                    // If an established connection was aborted, we dispose 
                    // established connection and create a new one.
                    if (bandClient != null)
                    {
                        bandClient.Dispose();
                        bandClient = null;
                    }
                    txtStatus.Text = "Connecting ...";
                    // Connect to the Band and get a new BandClient object
                    bandClient = await BandClientManager.Instance.ConnectAsync(band);
                    m_bIsConnected = true;
                    await getTheme();
                    endBandTask(String.Format("Successfully connected to {0}", band.Name.ToString()));

                }
                // If unable to connect to the detected Band, ask for user Input and try again.
                catch (Exception ex)
                {
                    m_bIsConnected = false;
                    if (await showDialogBox(ex.Message, "Try again ?"))
                    {
                        await connectToBand();
                    }
                    if (!m_bIsConnected)
                    {
                        endBandTask("Unable to connect to the Band");
                    }
                }
                // Save the theme locally to be used in case of reset
                // Saving it locally avoids the trip to retrieve it from the Band
                saveThemeLocally();
            }
            return m_bIsConnected;
        }


        /* This method retrieves the theme from the connected Band
         * Updates the UI according the theme retrieved
         */
        private async Task<bool> getTheme()
        {
            try
            {
                startBandTask(String.Format("Downloading Band theme from {0}", band.Name.ToString()));

                // Retrieve theme from the band
                BandTheme bandTheme = await bandClient.PersonalizationManager.GetThemeAsync();

                // Update the UI
                eBase.Fill = new SolidColorBrush(bandTheme.Base.ToColor());
                eHighContrast.Fill = new SolidColorBrush(bandTheme.HighContrast.ToColor());
                eHighlight.Fill = new SolidColorBrush(bandTheme.Highlight.ToColor());
                eLowLight.Fill = new SolidColorBrush(bandTheme.Lowlight.ToColor());
                eMuted.Fill = new SolidColorBrush(bandTheme.Muted.ToColor());
                eSecondary.Fill = new SolidColorBrush(bandTheme.SecondaryText.ToColor());
                endBandTask(String.Format("Theme retrieved successfully from {0}", band.Name.ToString()));
            }
            catch (Exception ex)
            {
                endBandTask(ex.Message);
            }
            return m_bInProgress;
        }

        /* This method uploads the theme to the connected Band
        *  Updates the Band with the UI theme the user customizes
        */
        private async Task<bool> setTheme()
        {
            try
            {
                startBandTask(String.Format("Uploading Band theme to {0}", band.Name.ToString()));
                
                // Convert user customizations into Band theme.
                BandTheme bandTheme = new BandTheme()
                {
                    Base = ((SolidColorBrush)eBase.Fill).Color.ToBandColor(),
                    HighContrast = ((SolidColorBrush)eHighContrast.Fill).Color.ToBandColor(),
                    Lowlight = ((SolidColorBrush)eLowLight.Fill).Color.ToBandColor(),
                    Highlight = ((SolidColorBrush)eHighlight.Fill).Color.ToBandColor(),
                    Muted = ((SolidColorBrush)eMuted.Fill).Color.ToBandColor(),
                    SecondaryText = ((SolidColorBrush)eSecondary.Fill).Color.ToBandColor(),
                };

                // Update Band with the theme.
                await bandClient.PersonalizationManager.SetThemeAsync(bandTheme);
                endBandTask(String.Format("Theme uploaded successfully from {0}", band.Name.ToString()));
            }
            // This kind of exception occurs when an established connection
            // was aborted by the software in your host machine
            catch (BandIOException)
            {
                // Connect to the band and try again
                if (await connectToBand())
                {
                    await setTheme();
                }
            }
            catch (Exception ex)
            {
                endBandTask(ex.Message);
            }
            // Update the locally saved theme.
            saveThemeLocally();
            return m_bInProgress;

        }

        /* Shows the progress bar
         * Updates the status message
         */
        private void startBandTask(string _status)
        {
            m_bInProgress = true;
            progressStatus.IsActive = true;
            progressStatus.Visibility = Visibility.Visible;
            txtStatus.Text = _status;
        }

        /* Hides the progress bar
         * Updates the status message
         * Updates the title bar color
         */
        private void endBandTask(string _status)
        {
            m_bInProgress = false;
            progressStatus.IsActive = false;
            progressStatus.Visibility = Visibility.Collapsed;
            txtStatus.Text = _status;
            setTitleBarColor();
        }

        /* This method saves the theme locally into a dictionary.
         * Saved theme updates the UI incase the user wants to reset his changes.
         */
        private void saveThemeLocally()
        {
            // Clear the existing theme
            if (m_dctLocalTheme.Count != 0)
                m_dctLocalTheme.Clear();

            // Update the dictionary with new theme.
            m_dctLocalTheme.Add("BaseColor", eBase.Fill);
            m_dctLocalTheme.Add("HighContrastColor",eHighContrast.Fill);
            m_dctLocalTheme.Add("LowLightColor",eLowLight.Fill);
            m_dctLocalTheme.Add("HighlightColor",eHighlight.Fill);
            m_dctLocalTheme.Add("MutedColor",eMuted.Fill);
            m_dctLocalTheme.Add("SecondaryColor",eSecondary.Fill);
        }

        /* This method helps improve the UI experience, by updating the title bar
         * with the same color as the Band base color.
         */
        private void setTitleBarColor()
        {
            // ApiInformation enables you to detect whether a specified member is present 
            // so that you can safely make API calls across a variety of devices.
            // Check if the title bar is present and update it.
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.BackgroundColor = ((SolidColorBrush)eBase.Fill).Color;
                titleBar.ForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = titleBar.BackgroundColor;
                titleBar.ButtonForegroundColor = titleBar.ForegroundColor;
            }
        }

        /* This method invokes the color dialog box and
         * updates the UI and the respective Band color class
         */
        private async void setColor(Shape e)
        {
            var dialog = new colorDialog();
            var result = await dialog.ShowAsync();
            
            // Update only if the user accepts
            if (result == ContentDialogResult.Primary)
            {
                e.Fill = dialog.colorBrush;
            }
            setTitleBarColor();
        }

        // This methods is the event handler for the save button.
        private async void save_Click(object sender, RoutedEventArgs e)
        {
            // If Band is connected and there is no band operation already in progress.
            if (m_bIsConnected && !m_bInProgress)
            {
                if(await showDialogBox("Upload theme to Band ?", "Confirm"))
                {
                    await setTheme();
                }
            }
            else
            {
                // If Band operation in progress, notify the user
                if (m_bInProgress)
                {
                    await new MessageDialog("Task already in progress", "Please wait").ShowAsync();
                }
                else
                {
                    await new MessageDialog("No Band connected", "Error").ShowAsync();
                }
            }
        }

        // This methods is the event handler for the reset button.
        private async void reset_Click(object sender, RoutedEventArgs e)
        {
            if (!m_bInProgress)
            {
                // Update the UI from the locally saved theme.
                eBase.Fill = (SolidColorBrush)m_dctLocalTheme["BaseColor"];
                eHighContrast.Fill = (SolidColorBrush)m_dctLocalTheme["HighContrastColor"];
                eHighlight.Fill = (SolidColorBrush)m_dctLocalTheme["HighlightColor"];
                eLowLight.Fill = (SolidColorBrush)m_dctLocalTheme["LowLightColor"];
                eMuted.Fill = (SolidColorBrush)m_dctLocalTheme["MutedColor"];
                eSecondary.Fill = (SolidColorBrush)m_dctLocalTheme["SecondaryColor"];
                setTitleBarColor();
                txtStatus.Text = "App UI reset to default";
            }
            else
            {
                await new MessageDialog("Task already in progress", "Please wait").ShowAsync();
            }
        }

        // This methods is the event handler for the connect button.
        private async void connect_Click(object sender, RoutedEventArgs e)
        {
            if (!m_bInProgress)
            {
                await connectToBand();
            }
            else
            {
                await new MessageDialog("Task already in progress", "Please wait").ShowAsync();
            }
        }

        // This method updated the UI based on the respective Band color class clicked/tapped
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string tag = ((Windows.UI.Xaml.FrameworkElement)sender).Tag.ToString();
            setColor(m_dctGridEllipseMapping[tag]);
        }

        /* This method displays the dialog box with appropriate 
         * messages and adds Yes No buttons to the dialog box for user input.
         */
        private async Task<bool> showDialogBox(string _message,string _title)
        {
            MessageDialog myDialog = new MessageDialog(_message, _title);
            myDialog.Options = MessageDialogOptions.None;
            myDialog.Commands.Add(new Windows.UI.Popups.UICommand("Yes") { Id = 1 });
            myDialog.Commands.Add(new Windows.UI.Popups.UICommand("No") { Id = 0 });
            var result = await myDialog.ShowAsync();
            return Convert.ToBoolean(result.Id);
        }

        // Update grid background on mouse enter
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            SolidColorBrush sb = new SolidColorBrush(Colors.Gray);
            sb.Opacity = 0.42; // #363636
            ((Windows.UI.Xaml.Controls.Panel)sender).Background = sb;
        }

        // Return grid background to normal on mouse exit
        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ((Windows.UI.Xaml.Controls.Panel)sender).Background.Opacity = 1;
            ((Windows.UI.Xaml.Controls.Panel)sender).Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
