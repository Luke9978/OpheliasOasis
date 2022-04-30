using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

        Customer ec;

        public CustomerLookupPage()
        {
            this.InitializeComponent();
        }

        private void LookupCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            var existingCustomer = from item in cust.Values where (FirstNameBox.Text == item.FirstName && LastNameBox.Text == item.LastName) select item;

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
                ec = existingCustomer.First();

            //Fill in data
            CustNamesLabel.Text = "First Name: " + ec.FirstName + "\t\tLast Name: " + ec.LastName;
            CustPhoneEmailLabel.Text = "Phone Number: " + ec.Phone + "\t\tEmail Address: " + ec.Email;
            CustIDresIDLabel.Text = "CustomerID: " + ec.Id;

            var existingReservation = from item in resv.Values where (ec.ReservationID == item.ReservationID && item.Status != PaymentStatus.Completed) orderby item.StartDate select item;

            if(existingReservation.Count() > 0)
            {
                ReservationTypeDateLabel.Text = "Type: " + existingReservation.First().Type.ToString() + "\tCheck-in: " + existingReservation.First().StartDate.ToShortDateString() + "\tCheck-out: " + existingReservation.First().EndDate.ToShortDateString();
                double t = 0;
                foreach(var item in existingReservation.First().Prices)
                {
                    t += item;
                }
                ReservationPricesStatusLabel.Text = "Price: " + t.ToString("0.00") + "\t\tStatus: " + existingReservation.First().Status.ToString() + "\n";

            }
        }


        private void CheckInButton_Click(object sender, RoutedEventArgs e)
        {
            var findReservation = from item in resv.Values where item.ReservationID == ec.ReservationID select item;
            int[] available = new int[46]; available[0] = 1;
            int room = 0;
            
            var resF = from item in resv.Values where (item.StartDate.Date <= DateTime.Now.Date && item.EndDate.Date > DateTime.Now.Date) orderby item.RoomID select item;
            foreach(var item in resF)
            {
                for (int i = 1; i < 46; i++)
                {
                    if (item.RoomID == i)
                        available[i] = 1;
                }
            }
            
            foreach(var i in available)
            {
                if (available[i] == 0)
                    room = i;
            }

            findReservation.First().RoomID = room;
        }

      
        private void CheckOutButton_Click(object sender, RoutedEventArgs e)
        {
            _ = accomidationBill();
        }

        private async Task accomidationBill()
        {
            // This will bring up the File Explorer, user needs to Select .txt file
            var pick = new Windows.Storage.Pickers.FileOpenPicker
            {
                // we want thumbnail viewing mode and also to start in the Documents Library

                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            pick.FileTypeFilter.Add(".txt");        // looking for a .txt file

            StorageFile file = await pick.PickSingleFileAsync();
            if (file == null || !file.DisplayName.Equals("CheckOutReceipt"))
            {
                CustLookErrorLabel.Text = "Select Correct File";
                return;
            }
            var findReservation = from item in resv.Values where (item.ReservationID == ec.ReservationID) orderby item.StartDate select item;

            await FileIO.WriteTextAsync(file, "Ophelia Oasis Receipt\nDate: " + DateTime.Now + "\n\n");
            await FileIO.AppendTextAsync(file, ec.LastName + ", " + ec.FirstName+"\n");
            await FileIO.AppendTextAsync(file, "Room ID: " + findReservation.First().RoomID + "\n");
            await FileIO.AppendTextAsync(file, "Arrival Date: " + findReservation.First().StartDate + "\n");
            await FileIO.AppendTextAsync(file, "Departure Date: " + findReservation.First().EndDate + "\n");
            await FileIO.AppendTextAsync(file, "Number of Nights: " + findReservation.First().Prices.Count() + "\n");
            double tot = 0;
            foreach(var item in findReservation.First().Prices)
            {
                tot += item;
            }
            await FileIO.AppendTextAsync(file, "Total Charge: $" + tot.ToString("0.00"));

            findReservation.First().Status = PaymentStatus.Completed;
        }


        public void setDB(src.DatabaseManager database)
        {
            resv = database.GetReservations();
            cust = database.GetCustomers();
            ppd = database.GetPricePerDay();
        }

    }
}
