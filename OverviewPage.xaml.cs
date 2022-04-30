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
using Windows.UI;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OpheliasOasis
{
    public sealed partial class OverviewPage : UserControl
    {

        DateTime start, end;
        ReservationMap resv;      // reservation
        CustomerIDMap cust;     // customer
        PricePerDay ppd;        // price per day


        public OverviewPage()
        {
            this.InitializeComponent();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void CalendarView_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            var res_start_today = from item in resv.Values where item.StartDate.Date == DateTime.Now.Date select item;
            // Render basic day items.
            if (args.Phase == 0)
            {
                // Register callback for next phase.
                args.RegisterUpdateCallback(CalendarView_CalendarViewDayItemChanging);
            }
            // Set blackout dates.
            else if (args.Phase == 1)
            {
                // Blackout dates in the past and dates that are fully booked.
                if (args.Item.Date < DateTimeOffset.Now.Date ||
                    res_start_today.Count() == 45)
                {
                    args.Item.IsBlackout = true;
                }
                // Register callback for next phase.
                args.RegisterUpdateCallback(CalendarView_CalendarViewDayItemChanging);
            }
            // Set density bars.
            else if (args.Phase == 2)
            {
                // You don't need to set bars on past dates.
                if (args.Item.Date >= DateTimeOffset.Now)
                {
                    List<Color> densityColors = new List<Color>();
                    // Set a density bar color for each of the days bookings.
                    // It's assumed that there can't be more than 45 bookings in a day. Otherwise,
                    // further processing is needed to fit within the max of 10 density bars.
                    if (res_start_today.Count() <= 22)
                    {
                        densityColors.Add(Colors.Green);
                    }
                    else if (res_start_today.Count() < 45)
                    {
                        densityColors.Add(Colors.Yellow);
                    }
                    else if (res_start_today.Count() == 45)
                    {
                        densityColors.Add(Colors.Red);
                    }
                    args.Item.SetDensityColors(densityColors);
                }
            }
        }
        private void StartDateButton_Click(object sender, RoutedEventArgs e)
        {
            // No Date is selected
            if (MainCalendar.SelectedDates.Count == 0)
            { ErrorMessage.Text = "Please select a start date"; ErrorMessage.Visibility = Visibility.Visible; }

            // Date selected is in the past
            else if (MainCalendar.SelectedDates[0].Date < DateTime.Now.Date)
            { ErrorMessage.Text = "Please select a legal start date"; ErrorMessage.Visibility = Visibility.Visible; }

            // Valid start date
            else
            {
                ErrorMessage.Visibility = Visibility.Collapsed;     // just in case it's visible
                ReservationFields.Visibility = Visibility.Collapsed;    // so they can't make a reservation with invalid dates *Check
                start = MainCalendar.SelectedDates[0].Date;
                EndDateButton.IsEnabled = true;
                StartDateLabel.Text = start.ToShortDateString();
                end = DateTime.Now;                                 // just in case it had a previous value
                EndDateLabel.Text = "End Date";                     // ^
            }
        }

        private void EndDateButton_Click(object sender, RoutedEventArgs e)
        {
            // No Date Selected
            if (MainCalendar.SelectedDates.Count == 0)
            { ErrorMessage.Text = "Please select an end date"; ErrorMessage.Visibility = Visibility.Visible; }

            // End date occurs before the start date
            else if (MainCalendar.SelectedDates[0].Date <= start.Date)
            { ErrorMessage.Text = "Please select a legal end date"; ErrorMessage.Visibility = Visibility.Visible; }

            // Valid start date (for the most part)
            else
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
                end = MainCalendar.SelectedDates[0].Date;
                EndDateLabel.Text = end.ToShortDateString();
                EndDateButton.IsEnabled = false;                    // To change the end date, they need to put in a start date first
                CreateReservationButton.IsEnabled = true;
            }
        }

        private void CreateReservationButton_Click(object sender, RoutedEventArgs e)
        {
            // Checks to make sure that none of the days are fully booked
            for (var s = start.Date; s.Date < end.Date; s = s.AddDays(1))
            {
                var total_res = (from item in resv.Values where (item.StartDate.Date <= s.Date && item.EndDate.Date > s.Date) select item).Count();
                if (total_res == 45)
                { ErrorMessage.Text = "We are booked on " + s.Date.Day; ErrorMessage.Visibility = Visibility.Visible; break; }
            }

            // if we are booked, then don't let this be visible -- needs to be inserted
            ReservationFields.Visibility = Visibility.Visible;
            ReservationTypeDropdown.SelectedIndex = 0;

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
            for (var s = start.Date; s.Date < end.Date; s = s.AddDays(1)) { newReservation.Prices.Add(ppd[s.Date] * discount); }

            newReservation.StartDate = start;
            newReservation.EndDate = end;
            newReservation.RoomID = -1;     // -1 until the room is assigned each day




            Customer newCustomer = new Customer();
            newCustomer.FirstName = FirstNameBox.Text;
            newCustomer.LastName = LastNameBox.Text;
            newCustomer.Phone = PhoneNumberBox.Text;
            newCustomer.Email = EmailBox.Text;

            var existingCustomer = from item in cust.Values where (newCustomer.FirstName == item.FirstName && newCustomer.LastName == item.LastName && newCustomer.Phone == item.Phone) select item;

            if (existingCustomer.Count() == 0)
                cust.Add(newCustomer);                      // add the customer to database
            else if (existingCustomer.Count() == 1)
                newCustomer = existingCustomer.First();     // newCustomer now equals existing customer

            if (ReservationTypeDropdown.SelectedIndex == 2)
            {
                newReservation.Status = PaymentStatus.NotPaid;
            }
            else
            {
                CreditCard cc = new CreditCard();
                cc.CardNumbers = CreditCardBox.Text;
                cc.Name = NameOnCardBox.Text;
                cc.ExpirationDate = ExpirationDateBox.Text;
                newCustomer.CardOnFile = cc;
                newReservation.Status = PaymentStatus.Paid;
            }


            resv.Add(newReservation); //add the reservation to database

            // Update the DB
            newReservation.CustomerID = newCustomer.Id;
            newCustomer.Id = newReservation.CustomerID;
            resv[newReservation.ReservationID] = newReservation;
            cust[newCustomer.Id] = newCustomer;

        }

        private void ReservationTypeDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!EndDateLabel.Text.Equals("End Date"))           // for some reason, this method runs right when it's created. So this is just to prevent errors. Don't delete
            {
                double discount = 0;
                int selectedIndex = ReservationTypeDropdown.SelectedIndex;
                DateTime today = DateTime.Now.Date;
                double averageOccupancy = 0;
                double numDays = 0;
                for (DateTime i = start; i < end; i = i.AddDays(1))
                {
                    var res_today = from item in resv.Values where item.StartDate.Date >= i.Date && item.EndDate.Date <= i.Date select item;
                    averageOccupancy += res_today.Count();
                    numDays++;
                }
                averageOccupancy /= numDays;
                ErrorMessage.Visibility = Visibility.Collapsed;
                ConfirmReservationButton.IsEnabled = true;
                if (selectedIndex == 1 && today.AddDays(90) > start.Date)
                {
                    ErrorMessage.Visibility = Visibility.Visible;
                    ErrorMessage.Text = "Prepaid must be set at least 90 days in advanced.";
                    ConfirmReservationButton.IsEnabled = false;
                }
                else if (selectedIndex == 2 && today.AddDays(60) > start.Date)
                {
                    ErrorMessage.Visibility = Visibility.Visible;
                    ErrorMessage.Text = "60-Day must be set at least 60 days in advanced.";
                    ConfirmReservationButton.IsEnabled = false;
                }
                else if (selectedIndex == 4 && today.AddDays(30) <= start.Date)
                {
                    ErrorMessage.Visibility = Visibility.Visible;
                    ErrorMessage.Text = "Incentive must be set within 30 days from today.";
                    ConfirmReservationButton.IsEnabled = false;
                }
                else if (selectedIndex == 4 && averageOccupancy > 27.0)
                {
                    ErrorMessage.Visibility = Visibility.Visible;
                    ErrorMessage.Text = "The average occupancy is too high to select incentive.";
                    ConfirmReservationButton.IsEnabled = false;
                }

                if (selectedIndex == 0)               // Select Reservation is selected
                {
                    FirstNameBox.Visibility = Visibility.Collapsed;
                    LastNameBox.Visibility = Visibility.Collapsed;
                    PhoneNumberBox.Visibility = Visibility.Collapsed;
                    EmailBox.Visibility = Visibility.Collapsed;
                    CreditCardBox.Visibility = Visibility.Collapsed;
                    NameOnCardBox.Visibility = Visibility.Collapsed;
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
                for (var s = start.Date; s.Date < end.Date; s = s.AddDays(1))
                {
                    try
                    {
                        total += ppd[s.Date] * discount;
                    }
                    catch (KeyNotFoundException)
                    {
                        ErrorMessage.Visibility = Visibility.Visible;
                        ErrorMessage.Text = $"Date selected does not have a rate. Date: {s.Date.ToString()}";
                        FirstNameBox.Visibility = Visibility.Collapsed;
                        LastNameBox.Visibility = Visibility.Collapsed;
                        PhoneNumberBox.Visibility = Visibility.Collapsed;
                        EmailBox.Visibility = Visibility.Collapsed;
                        CreditCardBox.Visibility = Visibility.Collapsed;
                        NameOnCardBox.Visibility = Visibility.Collapsed;
                        ExpirationDateBox.Visibility = Visibility.Collapsed;
                        TotalAmountLabel.Visibility = Visibility.Collapsed;
                    }
                }
                TotalAmountLabel.Text = "$" + total;
            }

        }

        private void CancelReservationButton_Click(object sender, RoutedEventArgs e)
        {
            FirstNameBox.Visibility = Visibility.Collapsed;
            LastNameBox.Visibility = Visibility.Collapsed;
            PhoneNumberBox.Visibility = Visibility.Collapsed;
            EmailBox.Visibility = Visibility.Collapsed;
            CreditCardBox.Visibility = Visibility.Collapsed;
            NameOnCardBox.Visibility = Visibility.Collapsed;
            ExpirationDateBox.Visibility = Visibility.Collapsed;
            TotalAmountLabel.Visibility = Visibility.Collapsed;

            FirstNameBox.Text = "";
            LastNameBox.Text = "";
            PhoneNumberBox.Text = "";
            EmailBox.Text = "";
            CreditCardBox.Text = "";
            NameOnCardBox.Text = "";
            ExpirationDateBox.Text = "";
            TotalAmountLabel.Text = "";

            ConfirmReservationButton.IsEnabled = false;
            ErrorMessage.Visibility = Visibility.Collapsed;
            ConfirmReservationButton.IsEnabled = false;
            ReservationTypeDropdown.SelectedIndex = 0;
        }

        public void setDB(src.DatabaseManager database)
        {
            resv = database.GetReservations();
            cust = database.GetCustomers();
            ppd = database.GetPricePerDay();
        }
    }
}
