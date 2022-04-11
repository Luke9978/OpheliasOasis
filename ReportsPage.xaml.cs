


using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Windows;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OpheliasOasis
{
    public sealed partial class ReportsPage : UserControl
    {

        



        public ReportsPage()
        {
            this.InitializeComponent();
        }
        

        void DA_button(object sender, RoutedEventArgs e)
        {
            DailyArrivalsButton.BorderThickness = new Thickness(5.0);
        }

        void DO_button(object sender, RoutedEventArgs e)
        {
            DailyArrivalsButton.BorderThickness = new Thickness(5.0);
        }

        void EO_button(object sender, RoutedEventArgs e)
        {
            DailyArrivalsButton.BorderThickness = new Thickness(5.0);
        }

        void ERI_button(object sender, RoutedEventArgs e)
        {
            DailyArrivalsButton.BorderThickness = new Thickness(5.0);
        }

        void I_button(object sender, RoutedEventArgs e)
        {
            DailyArrivalsButton.BorderThickness = new Thickness(5.0);
        }

        void Print_button(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
