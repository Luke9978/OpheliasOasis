using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        private void CalendarView_CalendarViewDayItemChanging(CalendarView sender,
                               CalendarViewDayItemChangingEventArgs args)
        {
            // Render basic day items.
            if (args.Phase == 0)
            {
                // Register callback for next phase.
                args.RegisterUpdateCallback(CalendarView_CalendarViewDayItemChanging);
            }
            // Set blackout dates.
            else if (args.Phase == 1)
            {
                // Blackout dates in the past, and days not in the month.
                if (args.Item.Date < DateTimeOffset.Now)
                {
                    args.Item.IsBlackout = true;
                }
                // Register callback for next phase.
                args.RegisterUpdateCallback(CalendarView_CalendarViewDayItemChanging);
            }
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name == "LoadDB")
            {
                
            }
            if ((sender as Button).Name == "SaveDB")
            {

            }
        }

        private void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            var startOfMonth = new DateTime(year: e.NewDate.Year, month: e.NewDate.Month, day: 1);
            renderTextBox(startOfMonth);
        }
    }
}
