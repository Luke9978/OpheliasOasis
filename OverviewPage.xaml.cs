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

        DateTime start, end;
        IObservableMap<int, Reservation> resv;      // reservation
        IObservableMap<int, Customer> cust;     // customer
        IObservableMap<DateTime, double> ppd; // price per day


        public OverviewPage()
        {
            this.InitializeComponent();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
        private void StartDateButton_Click(object sender, RoutedEventArgs e)
        {
            // No Date is selected
            if (MainCalendar.SelectedDates.Count == 0)
            { ErrorMessage.Text = "Please select a start date"; ErrorMessage.Visibility = Visibility.Visible; }

            // Date selected is either today or in the past
            else if(MainCalendar.SelectedDates[0].Date <= DateTime.Now.Date)
            { ErrorMessage.Text = "Please select a legal start date"; ErrorMessage.Visibility = Visibility.Visible; }

            // Valid start date
            else
            {
                ErrorMessage.Visibility = Visibility.Collapsed;     // just in case it's visible
                start = MainCalendar.SelectedDates[0].Date;
                EndDateButton.IsEnabled = true;
                StartDateLabel.Text = start.ToShortDateString();
                end = DateTime.Now;                                 // just in case it had a previous value
                EndDateLabel.Text = "End Date";                     // ^
                ReservationFields.Visibility = Visibility.Collapsed;    // so they can't make a reservation with invalid dates *Check
            }
        }

        private void EndDateButton_Click(object sender, RoutedEventArgs e)
        {
            // No Date Selected
            if (MainCalendar.SelectedDates.Count == 0)
            { ErrorMessage.Text = "Please select an end date"; ErrorMessage.Visibility = Visibility.Visible; }

            // End date occurs before the start date
            else if(MainCalendar.SelectedDates[0].Date <= start.Date)
            { ErrorMessage.Text = "Please select a legal end date"; ErrorMessage.Visibility = Visibility.Visible; }

            // Valid start date (for the most part)
            else
            {
                end = MainCalendar.SelectedDates[0].Date;
                ErrorMessage.Visibility = Visibility.Collapsed;
                CreateReservationButton.IsEnabled = true;
                EndDateLabel.Text = end.ToShortDateString();
                EndDateButton.IsEnabled = false;                    // To change the end date, they need to put in a start date first
            }
        }

        private void CreateReservationButton_Click(object sender, RoutedEventArgs e)
        {
            // Checks to make sure that none of the days are fully booked
            for (var s = start.Date; s.Date < end.Date; s = s.AddDays(1)) 
            {
                var total_res = (from item in resv.Values where (item.StartDate.Date <= s.Date && item.EndDate.Date > s.Date) select item).Count();
                if(total_res == 45)
                { ErrorMessage.Text = "We are booked for " + s.Date; ErrorMessage.Visibility = Visibility.Visible; break; }
            }

            // if we are booked, then don't let this be visible -- needs to be inserted
            ReservationFields.Visibility = Visibility.Visible;

            // also need to calculate which reservations are available with the given dates... not sure how we can use the combobox.
            // Can we make items in the dropdown invisible?

        }

        private void ConfirmReservationButton_Click(object sender, RoutedEventArgs e)
        {

            Reservation newReservation = new Reservation();
            double discount = 0;
            
            // Which reservation is selected
            switch (ReservationTypeDropdown.SelectedIndex)
            {
                case 0:
                    return;
                case 1:
                    newReservation.Type = ReservationType.Prepaid; discount = 0.75; break;
                case 2:
                    newReservation.Type = ReservationType.SixtyDays; discount = 0.85; break;
                case 3:
                    newReservation.Type = ReservationType.Conventional; discount = 1; break;
                case 4:
                    newReservation.Type = ReservationType.Incentive; discount = 0.8; break;
            }

            // get the prices for each day
            for (var s = start.Date; s.Date < end.Date; s = s.AddDays(1))   { newReservation.Prices.Add(  ppd[s.Date]*discount  ); }


            //newReservation.ReservationID = resv.Count; //not sure if this works
            newReservation.StartDate = start;
            newReservation.EndDate = end;
            //newReservation.CustomerID = cust.Count; //not sure if this works
            newReservation.RoomID = -1;     // -1 until the room is assigned each day

            //resv.Add(resv.Count, newReservation); //add the reservation to database



            Customer newCustomer = new Customer();
            newCustomer.FirstName = FirstNameBox.Text;
            newCustomer.LastName = LastNameBox.Text;
            newCustomer.Phone = PhoneNumberBox.Text;
            newCustomer.Email = EmailBox.Text;
            //newCustomer.Id = cust.Count;
            //newCustomer.ReservationID = resv.Count;

            // needs an if statement to see if its 60 day or not
            CreditCard cc = new CreditCard();
            cc.CardNumbers = CreditCardBox.Text;
            cc.Name = NameOnCardBox.Text;
            cc.ExpirationDate = ExpirationDateBox.Text;
            newCustomer.CardOnFile = cc;


            // needs an if statement to see if the customer is already in the system
            //cust.Add(cust.Count,newCustomer); //add the customer to database
                      
            


        }

        private void ReservationTypeDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(!EndDateLabel.Text.Equals("End Date"))           // for some reason, this method runs right when it's created. So this is just to prevent errors. Don't delete
            {
                double discount = 0;
                if (ReservationTypeDropdown.SelectedIndex == 0)                     // Select Reservation is selected
                {
                    FirstNameBox.Visibility = Visibility.Collapsed;
                    LastNameBox.Visibility = Visibility.Collapsed;
                    PhoneNumberBox.Visibility = Visibility.Collapsed;
                    EmailBox.Visibility = Visibility.Collapsed;
                    CreditCardBox.Visibility = Visibility.Collapsed;
                    NameOnCardBox.Visibility= Visibility.Collapsed;
                    ExpirationDateBox.Visibility = Visibility.Collapsed;
                    TotalAmountLabel.Visibility = Visibility.Collapsed;
                }
                else if (ReservationTypeDropdown.SelectedIndex == 2)            // Collapse CC info when 60 day is selected
                {
                    FirstNameBox.Visibility = Visibility.Visible;
                    LastNameBox.Visibility = Visibility.Visible;
                    PhoneNumberBox.Visibility = Visibility.Visible;
                    EmailBox.Visibility = Visibility.Visible;
                    CreditCardBox.Visibility = Visibility.Collapsed;
                    NameOnCardBox.Visibility = Visibility.Collapsed;
                    ExpirationDateBox.Visibility = Visibility.Collapsed;
                    TotalAmountLabel.Visibility = Visibility.Visible;

                    discount = 0.85;
                }
                else
                {
                    FirstNameBox.Visibility = Visibility.Visible;                               // Make everything avaiable
                    LastNameBox.Visibility = Visibility.Visible;
                    PhoneNumberBox.Visibility = Visibility.Visible;
                    EmailBox.Visibility = Visibility.Visible;
                    CreditCardBox.Visibility = Visibility.Visible;
                    NameOnCardBox.Visibility = Visibility.Visible;
                    ExpirationDateBox.Visibility = Visibility.Visible;
                    TotalAmountLabel.Visibility = Visibility.Visible;

                    if (ReservationTypeDropdown.SelectedIndex == 1) discount = 0.75;
                    else if (ReservationTypeDropdown.SelectedIndex == 3) discount = 1.0;
                    else if (ReservationTypeDropdown.SelectedIndex == 4) discount = 0.8;
                }

                // This is for displaying the total price of the hotel stay
                double total = 0;
                for (var s = start.Date; s.Date < end.Date; s = s.AddDays(1))  {total += ppd[s.Date]*discount;}
                TotalAmountLabel.Text = "$" + total;
            }
            
        }





        public void setDB(src.DatabaseManager database)
        {
            resv = database.GetReservations();
            cust = database.GetCustomers();
            ppd = database.GetPricePerDay();
        }
    }
}
