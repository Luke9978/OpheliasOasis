using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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
    public sealed partial class ManagementPage : UserControl
    {
        private src.DatabaseManager DatabaseManager = null;
        private PricePerDay priceMap;

        public ManagementPage()
        {
            this.InitializeComponent();
        }
        public void SetDatabase(src.DatabaseManager db)
        {
            DatabaseManager = db;
            priceMap = db.GetPricePerDay();
            RateDatePicker.Date = DateTime.Now;
            var startOfMonth = new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: 1);
            renderTextBox(startOfMonth);
        }

        private void renderTextBox(DateTime TargetDate)
        {
            listBox.Text = "";
            string prices = "";
            var EndOfMonth = new DateTime(TargetDate.Year, TargetDate.Month, DateTime.DaysInMonth(TargetDate.Year, TargetDate.Month));
            for (var s = TargetDate.Date; s.Date <= EndOfMonth.Date; s = s.AddDays(1))
            {

                prices += s.Date.ToString("dd/MM/yyy") + ": $";
                try
                {
                    prices += priceMap[s.Date].ToString() + "\n";
                }
                catch (KeyNotFoundException)
                {
                    prices += "N/A\n";
                }
            }
            listBox.Text = prices;
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name == "LoadDB")
            {
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.List;
                openPicker.FileTypeFilter.Add(".db");

                StorageFile file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    await DatabaseManager.LoadDBAsync(file);
                }
            }
            if ((sender as Button).Name == "SaveDB")
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.FileTypeChoices.Add("SQL Database", new List<string>() { ".db" });
                savePicker.SuggestedFileName = "Database" + DateTime.Now.ToString("_yyyy_MM_dd") + ".db";
                var file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    await DatabaseManager.SaveDBAsync(file);
                }
            }
        }

        private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            var startOfMonth = new DateTime(year: e.NewDate.Year, month: e.NewDate.Month, day: 1);
            renderTextBox(startOfMonth);
            PriceDateCalendar.SetDisplayDate(startOfMonth);
        }

        private void Button_ClearCalendar(object sender, RoutedEventArgs e)
        {
            PriceDateCalendar.SelectedDates.Clear();
        }

        private async void Button_Set_Rate(object sender, RoutedEventArgs e)
        {
            double rate = -1;
            string errorReason = "";
            try
            {
                rate = double.Parse(wantedPrice.Text);
            }
            catch (FormatException ex)
            {
                errorReason = ex.Message;
            }

            if (rate < 0.0)
            {
                errorReason = "Number can't be equal or less than 0.";
            }

            if (errorReason != "")
            {
                var msg = new MessageDialog("Invalid input");
                msg.Content = errorReason;
                msg.Commands.Add(new UICommand("Okay"));
                await msg.ShowAsync();
                return;
            }

            foreach (var date in PriceDateCalendar.SelectedDates)
            {
                priceMap[date.Date] = rate;
            }
            
            var startOfMonth = new DateTime(year: RateDatePicker.Date.Year, month: RateDatePicker.Date.Month, day: 1);
            renderTextBox(startOfMonth);
            PriceDateCalendar.SelectedDates.Clear();
        }

        private void PriceDateCalendar_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            // Render basic day items.
            if (args.Phase == 0)
            {
                // Register callback for next phase.
                args.RegisterUpdateCallback(PriceDateCalendar_CalendarViewDayItemChanging);
            }
            else if (args.Phase == 1)
            {
                if (args.Item.Date.Date < DateTime.Now.Date)
                {
                    args.Item.IsBlackout = true;
                }
                args.RegisterUpdateCallback(PriceDateCalendar_CalendarViewDayItemChanging);
            }
        }
    }
}
