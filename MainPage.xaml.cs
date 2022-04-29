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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OpheliasOasis
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        src.DatabaseManager dataBase           = new src.DatabaseManager();
        OverviewPage        overviewPage       = new OverviewPage();
        CustomerLookupPage  customerLookupPage = new CustomerLookupPage();
        ReportsPage         reportsPage        = new ReportsPage();
        ManagementPage      managementPage     = new ManagementPage();
        MainPage            mainpage;

        public MainPage()
        {
            this.InitializeComponent();
            reportsPage.setDB(dataBase);
            overviewPage.setDB(dataBase);
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer;
            switch (item.Name)
            {
                case "Overview":
                    ContentFrame.Content = overviewPage;
                    break;
                case "CustomerLookup":
                    ContentFrame.Content = customerLookupPage;
                    break;
                case "Reports":
                    ContentFrame.Content = reportsPage;
                    break;
                case "Management":
                    ContentFrame.Content = managementPage;
                    break;
                case "Login":
                    if(PasswordBox.Text == "man")
                    {

                    Login.Visibility = Visibility.Collapsed;
                    Login.IsEnabled = false;
                    Logout.Visibility = Visibility.Visible;
                    Logout.IsEnabled = true;
                    Overview.Visibility = Visibility.Visible;
                    Overview.IsEnabled = true;
                    CustomerLookup.Visibility = Visibility.Visible;
                    CustomerLookup.IsEnabled = true;
                    Reports.Visibility = Visibility.Visible;
                    Reports.IsEnabled = true;
                    Management.Visibility = Visibility.Visible;
                    Management.IsEnabled = true;
                    ContentFrame.Content = null;
                    }
                    else if (PasswordBox.Text == "emp")
                    {
                        Login.Visibility = Visibility.Collapsed;
                        Login.IsEnabled = false;
                        Logout.Visibility = Visibility.Visible;
                        Logout.IsEnabled = true;
                        Overview.Visibility = Visibility.Visible;
                        Overview.IsEnabled = true;
                        CustomerLookup.Visibility = Visibility.Visible;
                        CustomerLookup.IsEnabled = true;
                        Reports.Visibility = Visibility.Visible;
                        Reports.IsEnabled = true;
                        Management.Visibility = Visibility.Collapsed;
                        Management.IsEnabled = false;
                        ContentFrame.Content = null;
                    }
                    break;
                case "Logout":
                    Logout.Visibility = Visibility.Collapsed;
                    Logout.IsEnabled = false;
                    Login.Visibility = Visibility.Visible;
                    Login.IsEnabled = true;
                    Overview.Visibility = Visibility.Collapsed;
                    Overview.IsEnabled = false;
                    CustomerLookup.Visibility = Visibility.Collapsed;
                    CustomerLookup.IsEnabled = false;
                    Reports.Visibility = Visibility.Collapsed;
                    Reports.IsEnabled = false;
                    Management.Visibility = Visibility.Collapsed;
                    Management.IsEnabled = false;
                    ContentFrame.Content = null;
                    System.Environment.Exit(0);
                    break;
            }
        }

        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
