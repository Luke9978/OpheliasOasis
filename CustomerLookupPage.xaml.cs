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
    public sealed partial class CustomerLookupPage : UserControl
    {

        ReservationMap resv;      // reservation
        CustomerIDMap cust;     // customer
        PricePerDay ppd;        // price per day

        public CustomerLookupPage()
        {
            this.InitializeComponent();
        }

        private void LookupCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            /*var existingCustomer = from item in cust.Values where (FirstNameBox.Text == item.FirstName && LastNameBox.Text == item.LastName) select item;
            Customer ec;

            if (existingCustomer.Count() == 0)
            { CustLookErrorLabel.Text = "Customer Not Found"; return; }
            else if (existingCustomer.Count() > 1)
            {
                foreach (var item in existingCustomer)
                {
                    if (item.Phone == PhoneNumberBox.Text)
                    { ec = item; break; }
                }
                CustLookErrorLabel.Text = "Please fill out more data";
                return;
            }
            else
                ec = existingCustomer.First();*/


            string[] row = { "First Name: ", "Last Name" };//ec.FirstName, "\t\tLast Name: ", ec.LastName };
            //var li = new ListViewItem(row);
            //SearchResults.Items.Add(item: new ListViewItem(row));

        }


        public void setDB(src.DatabaseManager database)
        {
            resv = database.GetReservations();
            cust = database.GetCustomers();
            ppd = database.GetPricePerDay();
        }
    }
}
