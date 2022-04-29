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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OpheliasOasis
{
    public sealed partial class OverviewPage : UserControl
    {
        public OverviewPage()
        {
            this.InitializeComponent();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        private void StartDateButton_Click(object sender, RoutedEventArgs e)
        {
            ReservationFields.Visibility = Visibility.Collapsed;
        }

        private void EndDateButton_Click(object sender, RoutedEventArgs e)
        {
            ReservationFields.Visibility = Visibility.Visible;
        }

        private void ReservationTypeDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (ReservationTypeDropdown.SelectedItem.Content.equals("Sixty-Days-in-Advanced"))
            //{
            //    CreditCardBox.Visibility = Visibility.Collapsed;              // Attempt to collapse credit card info when 60 day is selected
            //    NameOnCardBox.Visibility = Visibility.Collapsed;
            //    ExpirationDateBox.Visibility = Visibility.Collapsed;
            //}
        }
    }
}
