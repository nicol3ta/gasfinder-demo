using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace GasFinder
{
    
    public sealed partial class SearchPage : Page
    {

        public SearchPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }


        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            FuelType fuelType = (FuelType) Enum.Parse(typeof(FuelType),((ComboBoxItem)FuelComboBox.SelectedItem).Content.ToString());    
            Criteria criteria = (Criteria) Enum.Parse(typeof(Criteria),((ComboBoxItem)CriteriaComboBox.SelectedItem).Content.ToString());

 
            SearchCriteria searchCriteria = new SearchCriteria(criteria, fuelType);
            //Find the next gas station and go back to the map view
            Type navType = typeof(MainPage);
            if (this.Frame != null)
            {
                this.Frame.Navigate(navType, searchCriteria);
            }
        }
    }
}
