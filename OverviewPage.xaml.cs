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
using Windows.UI.Popups;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OpheliasOasis
{
    public sealed partial class OverviewPage : UserControl
    {

        DateTime start, end;
        ReservationMap resv;      // reservation
        CustomerIDMap cust;     // customer
        PricePerDay ppd;        // price per day
        bool IsUpdatingValue = false;
        bool areYouSure;
        Reservation updatingRes;

        /// <summary>
        /// Default constructor
        /// </summary>
        public OverviewPage()
        {
            this.InitializeComponent();
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Populates overview with currently searched reservation
        /// </summary>
        public void LoadExisitngReservation(Reservation aRes)
        {
            IsUpdatingValue = true;
            updatingRes = aRes;
            start = aRes.StartDate;
            end = aRes.EndDate;
            StartDateLabel.Text = aRes.StartDate.Date.ToString();
            EndDateLabel.Text = aRes.EndDate.Date.ToString();

            ReservationFields.Visibility = Visibility.Visible;

            Customer aCus = cust[aRes.CustomerID];

            switch (updatingRes.Type)
            {
                case ReservationType.Conventional: SetFeildsVisiablity(3); ReservationTypeDropdown.SelectedIndex = 3; break;
                case ReservationType.Incentive:    SetFeildsVisiablity(4); ReservationTypeDropdown.SelectedIndex = 4; break;
                case ReservationType.SixtyDays:    SetFeildsVisiablity(2); ReservationTypeDropdown.SelectedIndex = 2; break;
                case ReservationType.Prepaid:      SetFeildsVisiablity(1); ReservationTypeDropdown.SelectedIndex = 1; break;
                default: break;
            }

            FirstNameBox.Text = aCus.FirstName;
            LastNameBox.Text = aCus.LastName;
            PhoneNumberBox.Text = aCus.Phone;
            EmailBox.Text = aCus.Email;
            if (aCus.CardOnFile != null)
            {
                CreditCardBox.Text = aCus.CardOnFile.CardNumbers;
                NameOnCardBox.Text = aCus.CardOnFile.Name;
                ExpirationDateBox.Text = aCus.CardOnFile.ExpirationDate;
            }
            else
            {
                CreditCardBox.Text = "";
                NameOnCardBox.Text = "";
                ExpirationDateBox.Text = "";
            }

            TotalAmountLabel.Text = aRes.Prices.Sum().ToString();
        }
        
        private async void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Yes")
            {
                areYouSure = true;
            }
            else
            {
                areYouSure = false;
            }
        }
        
        /// <summary>
        /// Updates the existing reservation with newly inserted info
        /// </summary>
        private async void UpdateReservation()
        {
            List<double> updatedPrice = new List<double>();

            // Need to rerun the price calc
            if (updatingRes.StartDate != start || updatingRes.EndDate != end)
            {
                double discount = 1.0;
                switch (updatingRes.Type)
                {
                    case ReservationType.Prepaid: discount = 0.75; break;
                    case ReservationType.SixtyDays: discount = 0.85; break;
                    case ReservationType.Conventional: discount = 1.0; break;
                    case ReservationType.Incentive: discount = 0.8; break;
                    default: break;
                }

                // get the prices for each day
                for (var s = start.Date; s.Date < end.Date; s = s.AddDays(1)) { updatedPrice.Add(ppd[s.Date] * discount); }

            }
            double diff = Math.Max(updatedPrice.Sum() - updatingRes.Prices.Sum(), 0.0);
            var popup = new MessageDialog("Are you sure? Price difference: " + diff.ToString());
            popup.Commands.Add(new UICommand(
                    "Yes",
                    new UICommandInvokedHandler(this.CommandInvokedHandler)));
            popup.Commands.Add(new UICommand(
                "Cancel",
                new UICommandInvokedHandler(this.CommandInvokedHandler)));

            popup.DefaultCommandIndex = 0;

            popup.CancelCommandIndex = 1;

            await popup.ShowAsync();

            if (areYouSure == false)
            {
                IsUpdatingValue = false;
                resetFeilds();
                return;
            }

            var list = from item in cust.Values where updatingRes.CustomerID == item.Id select item;
            
            var TargetCust = list.First();

            updatingRes.Prices = updatedPrice;

            TargetCust.FirstName = FirstNameBox.Text;
            TargetCust.LastName = LastNameBox.Text;
            TargetCust.Phone = PhoneNumberBox.Text;
            TargetCust.Email = EmailBox.Text;

            if (TargetCust.CardOnFile == null)
            {
                var newCard = new CreditCard();
                TargetCust.CardOnFile = newCard;
            }
            
            TargetCust.CardOnFile.CardNumbers = CreditCardBox.Text;
            TargetCust.CardOnFile.Name = NameOnCardBox.Text;
            TargetCust.CardOnFile.ExpirationDate = ExpirationDateBox.Text;

            resv[updatingRes.ReservationID] = updatingRes;
            cust[TargetCust.Id] = TargetCust;

            IsUpdatingValue = false;
            resetFeilds();
        }

        /// <summary>
        /// Blackout and color specific dates based off occupancy
        /// </summary>
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

        /// <summary>
        /// Sets the start date for reservation
        /// </summary>
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
                start = MainCalendar.SelectedDates[0].Date;
                EndDateButton.IsEnabled = true;
                StartDateLabel.Text = start.ToShortDateString();
                // Not when updating exisitng Reservation
                if (!IsUpdatingValue)
                {
                    ReservationFields.Visibility = Visibility.Collapsed;    // so they can't make a reservation with invalid dates *Check
                    end = DateTime.Now;                                 // just in case it had a previous value
                    EndDateLabel.Text = "End Date";                     // ^
                }
            }
        }

        /// <summary>
        /// Sets the end date for reservation
        /// </summary>
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

        /// <summary>
        /// Allows reservation fields to be shown if checks pass
        /// </summary>
        private void CreateReservationButton_Click(object sender, RoutedEventArgs e)
        {
            // Checks to make sure that none of the days are fully booked
            int[] available = new int[46]; available[0] = 1;
            for (var s = start.Date; s.Date < end.Date; s = s.AddDays(1))
            {
                var total_res = from item in resv.Values where (item.StartDate.Date <= s.Date && item.EndDate.Date > s.Date) select item;
                
                if (total_res.Count() == 45)
                { ErrorMessage.Text = "We are booked on " + s.Date.Day; ErrorMessage.Visibility = Visibility.Visible; return; }

            }

            // if we are booked, then don't let this be visible -- needs to be inserted
            ReservationFields.Visibility = Visibility.Visible;
            ReservationTypeDropdown.SelectedIndex = 0;
        }

        /// <summary>
        /// Creates the reservation and sends to the DB
        /// </summary>
        private void ConfirmReservationButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstNameBox.Text.Length == 0 || LastNameBox.Text.Length == 0 || PhoneNumberBox.Text.Length == 0) 
            {
                var msg = new MessageDialog("Invalid input");
                msg.Content = "You can't leave first name, last name, or phone number blank.";
                msg.Commands.Add(new UICommand("Okay"));
                msg.ShowAsync();
                return;
            }
            else if (ReservationTypeDropdown.SelectedIndex != 2 && (CreditCardBox.Text.Length == 0 || NameOnCardBox.Text.Length == 0 || ExpirationDateBox.Text.Length == 0))
            {
                var msg = new MessageDialog("Invalid input");
                msg.Content = "Fill in all credit card fields.";
                msg.Commands.Add(new UICommand("Okay"));
                msg.ShowAsync();
                return;
            }
            else if (ReservationTypeDropdown.SelectedIndex == 2 && EmailBox.Text.Length == 0)
            {
                var msg = new MessageDialog("Invalid input");
                msg.Content = "Fill in your email.";
                msg.Commands.Add(new UICommand("Okay"));
                msg.ShowAsync();
                return;
            }

            if (IsUpdatingValue)
            {
                UpdateReservation();
            }

            //ReservationTypeDropdown.SelectedIndex(1)
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

            resetFeilds();
            PaymentMessage.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Sets the visibility of fields
        /// </summary>
        private void SetFeildsVisiablity(int selection)
        {
            if (selection == 0)               // Select Reservation is selected
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
            else if (selection == 2)            // Collapse CC info when 60 day is selected
            {
                FirstNameBox.Visibility = Visibility.Visible;
                LastNameBox.Visibility = Visibility.Visible;
                PhoneNumberBox.Visibility = Visibility.Visible;
                EmailBox.Visibility = Visibility.Visible;
                CreditCardBox.Visibility = Visibility.Collapsed;
                NameOnCardBox.Visibility = Visibility.Collapsed;
                ExpirationDateBox.Visibility = Visibility.Collapsed;
                TotalAmountLabel.Visibility = Visibility.Visible;
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
            }
        }

        /// <summary>
        /// Contains many checks for legal reservation types when a new one is selected
        /// </summary>
        private void ReservationTypeDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!EndDateLabel.Text.Equals("End Date"))           // for some reason, this method runs right when it's created. So this is just to prevent errors. Don't delete
            {
                double discount = 0;
                int selectedIndex = ReservationTypeDropdown.SelectedIndex;
                DateTime today = DateTime.Now.Date;
                double averageOccupancy = 0;
                int totalOccupancy = 0;
                int numDays = 0;
                for (DateTime i = start; i < end; i = i.AddDays(1))
                {
                    //var res_today = from item in resv.Values where item.StartDate.Date >= i.Date && item.EndDate.Date <= i.Date select item;
                    var res_today = from item in resv.Values where item.StartDate.Date == i.Date select item;
                    totalOccupancy += res_today.Count();
                    numDays++;
                }
                averageOccupancy = totalOccupancy / numDays;
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

                SetFeildsVisiablity(selectedIndex);

                switch (selectedIndex)
                {
                    case 1: discount = 0.75; break;
                    case 2: discount = 0.85; break;
                    case 3: discount = 1.0;  break;
                    case 4: discount = 0.8;  break;
                    default: break;
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
                TotalAmountLabel.Text = "$" + total.ToString("0.00");
            }

        }

        /// <summary>
        /// Resets a variety of fields to empty
        /// </summary>
        private void resetFeilds()
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

            StartDateLabel.Text = "Start Date";
            EndDateLabel.Text = "End Date";
            start = DateTime.Now;
            end = DateTime.Now;

            ConfirmReservationButton.IsEnabled = false;
            ErrorMessage.Visibility = Visibility.Collapsed;
            ConfirmReservationButton.IsEnabled = false;
            ReservationTypeDropdown.SelectedIndex = 0;
        }

        private void CancelReservationButton_Click(object sender, RoutedEventArgs e)
        {
            resetFeilds();
            IsUpdatingValue = false;
        }

        /// <summary>
        /// Sets the DB
        /// </summary>
        public void setDB(src.DatabaseManager database)
        {
            resv = database.GetReservations();
            cust = database.GetCustomers();
            ppd = database.GetPricePerDay();
        }
    }
}
