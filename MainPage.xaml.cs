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
        src.DatabaseManager dataBase = new src.DatabaseManager();
        OverviewPage overviewPage = new OverviewPage();
        CustomerLookupPage customerLookupPage = new CustomerLookupPage();
        ReportsPage reportsPage = new ReportsPage();
        ManagementPage managementPage = new ManagementPage();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Windows.UI.Xaml.Controls.Button)
            {
                var button = sender as Windows.UI.Xaml.Controls.Button;

                button.Content = dataBase.hello();
            }
                
        }

        void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle button click.
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

            }
        }
    }
}
