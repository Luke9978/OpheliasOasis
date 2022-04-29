
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using System.Linq;



// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OpheliasOasis
{
    public partial class ReportsPage : UserControl
    {

        SolidColorBrush selected = new SolidColorBrush(Windows.UI.Colors.DarkGreen);
        SolidColorBrush not_selected = new SolidColorBrush(Windows.UI.Colors.Gray);

        IObservableMap<int, Reservation> resv;      // reservation
        IObservableMap<int, Customer> cust;     // customer
        //IObservableMap<DateTime, double> ppd; // price per day

        List<Button> buttons = new List<Button>();      // list will contain all the report buttons
        int lastButton = 0;                             // number 0-4 and contains the most recent button clicked
        Boolean file_selected = false;                  // false if the .txt file hasn't been selected yet, true if selected
        StorageFile file;                               // the file that will contain the report data
        bool isManager;

        public ReportsPage()
        {
            this.InitializeComponent();
            buttons.Add(DailyArrivalsButton);                       // Add all the buttons
            buttons.Add(DailyOccupancyButton);
            buttons.Add(ExpectedOccupancyButton);
            buttons.Add(ExpectedRoomIncomeButton);
            buttons.Add(IncentiveButton);
        }

        //public void setIsManagerVisible(bool isManager)
        //{
        //    this.isManager = isManager;
        //}


        void DA_button(object sender, RoutedEventArgs e)        // Daily Arrivals Button
        {
            buttons[lastButton].Background = not_selected;
            DailyArrivalsButton.Background = selected;
            lastButton = 0;
        }

        void DO_button(object sender, RoutedEventArgs e)        // Daily Occupancy Button
        {
            buttons[lastButton].Background = not_selected;
            DailyOccupancyButton.Background = selected;
            lastButton = 1;
        }

        void EO_button(object sender, RoutedEventArgs e)        // Expected Occupancy Button
        {
            buttons[lastButton].Background = not_selected;
            ExpectedOccupancyButton.Background = selected;
            lastButton = 2;
        }

        void ERI_button(object sender, RoutedEventArgs e)       // Expected Room Income Button
        {
            buttons[lastButton].Background = not_selected;
            ExpectedRoomIncomeButton.Background = selected;
            lastButton = 3;
        }

        void I_button(object sender, RoutedEventArgs e)         // Incentive Button
        {
            buttons[lastButton].Background = not_selected;
            IncentiveButton.Background = selected;
            lastButton = 4;
        }

        void Print_button(object sender, RoutedEventArgs e)     // Print Button
        {
            _ = AddText();
        }

        public async Task AddText()
        {
            if (file_selected == false)
            {
                // This will bring up the File Explorer, user needs to Select .txt file
                var pick = new Windows.Storage.Pickers.FileOpenPicker
                {
                    // we want thumbnail viewing mode and also to start in the Documents Library

                    ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
                };
                pick.FileTypeFilter.Add(".txt");        // looking for a .txt file

                file = await pick.PickSingleFileAsync();
                if (file == null)
                {
                    // tell the label that a file was not selected
                    return;
                }
                file_selected = true;

            }

            
            if(lastButton == 0)
            {
                await FileIO.WriteTextAsync(file, "Daily Arrivals Report\nDate: "+DateTime.Now+"\n\n");
                await FileIO.AppendTextAsync(file, "Last Name, First Name           Reservation Type            Room#           Departure Date\n");
                await FileIO.AppendTextAsync(file, "------------------------------------------------------------------------------------------\n\n");

                var res_start_today = from item in resv.Values where item.StartDate.Date == DateTime.Now.Date select item;
                var cust_ordered = from item in cust.Values orderby item.LastName select item;

                foreach (var i in res_start_today)
                {
                    var customerF = from j in cust.Values where j.Id == i.CustomerID select j;      // Customer Found using this Query
                    await FileIO.AppendTextAsync(file, customerF.First().LastName + ", " + customerF.First().FirstName + "          " + i.Type + "          " + i.RoomID + "           " + i.EndDate + "\n");
                }

            }


            else if(lastButton == 1)
            {
                await FileIO.WriteTextAsync(file, "Daily Occupancy Report\nDate: " + DateTime.Now + "\n\n");
                await FileIO.AppendTextAsync(file, "Room#           Last Name, First Name           Departure Date (* if leaving today)\n");
                await FileIO.AppendTextAsync(file, "-----------------------------------------------------------------------------------\n\n");

                var resF = from item in resv.Values where (item.StartDate.Date < DateTime.Now.Date && item.EndDate.Date >= DateTime.Now.Date) orderby item.RoomID select item;

                foreach (var i in resF)
                {
                    var customerF = from j in cust.Values where j.Id == i.CustomerID select j;
                    if (i.EndDate.Day == DateTime.Now.Day)
                        await FileIO.AppendTextAsync(file, i.RoomID + "           " + customerF.First().LastName + ", " + customerF.First().FirstName + "           " + "*\n");
                    else
                        await FileIO.AppendTextAsync(file, i.RoomID + "           " + customerF.First().LastName + ", " + customerF.First().FirstName + "           " + i.EndDate + "\n");
                }

            }


            else if(lastButton == 2)
            {
                await FileIO.WriteTextAsync(file, "Expected Occupancy Report\nDate: " + DateTime.Now + "\n\n");
                await FileIO.AppendTextAsync(file, "Date            Prepaid            60-day          Conventional            Incentive           Total\n");
                await FileIO.AppendTextAsync(file, "----------------------------------------------------------------------------------------------------\n\n");

                DateTime thirty = DateTime.Now.Date.AddDays(30);

                var resF = from item in resv.Values where ((item.StartDate.Date > DateTime.Now.Date || item.StartDate.Date < thirty) || (item.EndDate.Date > DateTime.Now.Date || item.EndDate.Date < thirty)) orderby item.StartDate select item;

                int[] total = new int[120];

                int hi = 0;

                foreach (var i in resF)
                {
                    hi++;
                    int difference = i.StartDate.Subtract(DateTime.Now.Date).Days;

                    foreach (var j in i.Prices)
                    {
                        
                        if (difference < 0)
                            difference++;
                        else if (difference < 30)
                        {
                            if (i.Type == ReservationType.Prepaid)
                                total[difference * 4]++;
                            else if (i.Type == ReservationType.SixtyDays)
                                total[difference * 4 + 1]++;
                            else if (i.Type == ReservationType.Conventional)
                                total[difference * 4 + 2]++;
                            else if (i.Type == ReservationType.Incentive)
                                total[difference * 4 + 3]++;

                            difference++;
                        }
                    }
                }
                double tot = 0;
                for (int i = 1; i <= 120 ; i+=4)
                {
                    int t = total[i - 1] + total[i] + total[i + 1] + total[i + 2];
                    await FileIO.AppendTextAsync(file, DateTime.Now.Date.AddDays((i-1)/4) + "         " + total[i-1] + "            " + total[i] + "            " + total[i+1] + "            " + total[i+2] + "            "+ hi+"\n");
                    tot += t;
                }
                await FileIO.AppendTextAsync(file, "Average Expected Occupancy Rate:  " + tot / 30);


            }


            else if(lastButton == 3)
            {
                await FileIO.WriteTextAsync(file, "Expected Room Income Report\nDate: " + DateTime.Now + "\n\n");
                await FileIO.AppendTextAsync(file, "Date                    Income\n");
                await FileIO.AppendTextAsync(file, "------------------------------\n\n");

                DateTime thirty = DateTime.Now.Date.AddDays(30);

                var resF = from item in resv.Values where ((item.StartDate.Date > DateTime.Now.Date || item.StartDate.Date < thirty) || (item.EndDate.Date > DateTime.Now.Date || item.EndDate.Date < thirty)) orderby item.StartDate select item;

                double[] total = new double[30];

                foreach(var i in resF)
                {
                    int difference = i.StartDate.Subtract(DateTime.Now.Date).Days;
                    
                    foreach(var j in i.Prices)
                    {
                        if (difference < 0)
                            difference++;
                        else if (difference < 30)
                        {
                            total[difference] = total[difference] + j;
                            difference++;
                        }
                    }
                }
                double tot = 0;
                for(int i =0; i < 30; i++)
                {
                    await FileIO.AppendTextAsync(file, DateTime.Now.Date.AddDays(i) + "                 " + total[i] + "\n");
                    tot += total[i];
                }
                await FileIO.AppendTextAsync(file, "Total Income:  $" + tot + "\n");
                await FileIO.AppendTextAsync(file, "Average Income:  $" + tot / 30);

            }


            else if(lastButton == 4)
            {
                await FileIO.WriteTextAsync(file, "Incentive Report\nDate: " + DateTime.Now + "\n\n");
                await FileIO.AppendTextAsync(file, "Date                Total Incentive Discount\n");
                await FileIO.AppendTextAsync(file, "--------------------------------------------\n\n");

                DateTime thirty = DateTime.Now.Date.AddDays(30);

                var resF = from item in resv.Values where ((item.Type.Equals(ReservationType.Incentive) && (item.StartDate.Date > DateTime.Now.Date || item.StartDate.Date < thirty)) || (item.EndDate.Date > DateTime.Now.Date || item.EndDate.Date < thirty)) orderby item.StartDate select item;

                double[] total = new double[30];

                foreach(var i in resF)
                {
                    int difference = i.StartDate.Subtract(DateTime.Now.Date).Days;

                    foreach(var j in i.Prices)
                    {
                        if (difference < 0)
                            difference++;
                        else if(difference < 30)
                        {
                            total[difference] += (j / 0.8) - j;  // normal_price - (normal_price*0.2) = incentive_price -> normal_price = incentive_price/0.8
                            difference++;
                        }
                    }
                }
                double tot = 0;
                for(int i=0; i<30; i++)
                {
                    await FileIO.AppendTextAsync(file, DateTime.Now.Date.AddDays(i) + "             " + total[i] + "\n");
                    tot += total[i];
                }
                await FileIO.AppendTextAsync(file, "Total Incentive Discount:  $" + tot + "\n");
                await FileIO.AppendTextAsync(file, "Average Incentive Discount:  $" + tot / 30);
            }

        }





        public void setDB(src.DatabaseManager database)
        {
            resv = database.GetReservations();
            cust = database.GetCustomers();
            //ppd = database.GetPricePerDay();
        }
    }
}
