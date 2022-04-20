
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;



// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace OpheliasOasis
{
    public partial class ReportsPage : UserControl
    {

        SolidColorBrush selected = new SolidColorBrush(Windows.UI.Colors.DarkGreen);
        SolidColorBrush not_selected = new SolidColorBrush(Windows.UI.Colors.Gray);

        List<Button> buttons = new List<Button>();      // list will contain all the report buttons
        int lastButton = 0;                             // number 0-4 and contains the most recent button clicked

        public ReportsPage()
        {
            this.InitializeComponent();
            buttons.Add(DailyArrivalsButton);                       // Add all the buttons
            buttons.Add(DailyOccupancyButton);
            buttons.Add(ExpectedOccupancyButton);
            buttons.Add(ExpectedRoomIncomeButton);
            buttons.Add(IncentiveButton);
        }
        

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

            // This will bring up the File Explorer, user needs to Select .txt file
            var pick = new Windows.Storage.Pickers.FileOpenPicker
            {
                // we want thumbnail viewing mode and also to start in the Documents Library

                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            pick.FileTypeFilter.Add(".txt");        // looking for a .txt file

            StorageFile file = await pick.PickSingleFileAsync();
            
            if(lastButton == 0)
            {
                await FileIO.WriteTextAsync(file, "Daily Arrivals Report");
            }


            else if(lastButton == 1)
            {
                await FileIO.WriteTextAsync(file, "Daily Occupancy Report");
            }


            else if(lastButton == 2)
            {
                await FileIO.WriteTextAsync(file, "Expected Occupancy Report");
            }


            else if(lastButton == 3)
            {
                await FileIO.WriteTextAsync(file, "Expected Room Income Report");
            }


            else if(lastButton == 4)
            {
                await FileIO.WriteTextAsync(file, "Incentive Report");
            }

        }
    }
}
