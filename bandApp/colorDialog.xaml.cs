using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Convert String to Brush
using System.Reflection;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace bandApp
{
    public sealed partial class colorDialog : ContentDialog
    {
       
        bool bCanClose = false;
        static List<PropertyInfo> lstColors = new List<PropertyInfo>();

        public colorDialog()
        {
            this.InitializeComponent();
            loadColors();
        }

        public SolidColorBrush colorBrush
        {  get; set;   }

        private void loadColors()
        {
            if(lstColors.Count == 0)
            { 
                lstColors = typeof(Colors).GetProperties().ToList<PropertyInfo>();
            }
            cmbColors.ItemsSource = lstColors;
            if (cmbColors.ItemsSource != null)
            {
                cmbColors.SelectedIndex = 0;
            }

        }
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                if (rbColorName.IsChecked == true)
                {
                    var selectedColor = (PropertyInfo)cmbColors.SelectedValue;
                    Color c = (Color)selectedColor.GetValue(null);
                    colorBrush = new SolidColorBrush(c);
                    bCanClose = true;
                }

                if (rbCustomColor.IsChecked == true)
                {
                    string hexCode = txtHex.Text.Replace("#", "");
                    if(hexCode.Length == 6)
                    { 
                        colorBrush = hexToBrush(hexCode);
                        bCanClose = true;
                    }
                    else
                    {
                        txtStatus.Text = "Invalid hex value";
                    }
                }
            }
            catch (Exception ex)
            {
                bCanClose = false;
                txtStatus.Text = ex.Message ;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            bCanClose = true;
        }

        private void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (!bCanClose)
                args.Cancel = true;

        }

        public SolidColorBrush hexToBrush(string hex)
        {
            
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            return new SolidColorBrush(Color.FromArgb(255, r, g, b));
        }

        private void rbColorName_Checked(object sender, RoutedEventArgs e)
        {
            txtHex.Visibility = Visibility.Collapsed;
            cmbColors.Visibility = Visibility.Visible;
        }

        private void rbRGB_Checked(object sender, RoutedEventArgs e)
        {
            txtHex.Visibility = Visibility.Visible;
            cmbColors.Visibility = Visibility.Collapsed;
        }
    }
}
