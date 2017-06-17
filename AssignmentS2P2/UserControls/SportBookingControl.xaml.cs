using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AssignmentS2P2
{
    /// <summary>
    /// Interaction logic for SportBookingControl.xaml
    /// </summary>
    public partial class SportBookingControl : UserControl
    {
        private BookingSystemDBEntities context;

        public SportBookingControl()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            // Load resources into controls
            LoadControlResource();
            // Default Selections
            LoadDefaultSelections();
        }

        private void LoadControlResource() // Load resources into controls
        {
            this.comboBoxSportFacilityChoice.ItemsSource = new ObservableCollection<string>(ResourceCollection.sportFacilityTypes);
            this.comboBoxSportTimeSlot.ItemsSource = new ObservableCollection<string>(ResourceCollection.sportTimeSlots);
            this.comboBoxSportBookingDuration.ItemsSource = new ObservableCollection<string>(ResourceCollection.durationList);
        }
        
        private void LoadDefaultSelections() // Loads control default selections
        {
            this.datepickerSport.SelectedDate = DateTime.Today.AddDays(1);
            this.comboBoxSportFacilityChoice.SelectedIndex = 0;
            this.comboBoxSportTimeSlot.SelectedIndex = 0;
            this.checkBoxSportBookingDuration.IsChecked = false;
            this.comboBoxSportBookingDuration.IsEnabled = false;
        }

        private void buttonSportAddToCart_Click(object sender, RoutedEventArgs e) // Add to cart button
        {
            // =================== PANEL Date ===================
            // Get booking date
            DateTime bookingDate = datepickerSport.SelectedDate.GetValueOrDefault();
            if (bookingDate < DateTime.Today)
            {
                MessageBox.Show("Booking date cannot be earlier than today's date!", "Error: You're not a time traveller", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (bookingDate == DateTime.Today)
            {
                MessageBox.Show("You must book one day in advance!", "Error: You can just cheat like that", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // =================== PANEL FACILITY CHOICE ===================
            int facilityChoice = comboBoxSportFacilityChoice.SelectedIndex + 1;
            int timeSlotChoice = comboBoxSportTimeSlot.SelectedIndex + 1;

            int duration = 2;
            if (comboBoxSportBookingDuration.IsEnabled)
            {
                duration = Int32.Parse(comboBoxSportBookingDuration.SelectedItem.ToString());
            }

            // ============================= CONFLICT CHECKING =============================
            using (context = new BookingSystemDBEntities()) // Check database
            {
                // Checks for row that matches given user choices in the SportBookings Table in database
                // Null return => Resource is available
                var resource = (from rs in context.SportBookings where rs.FacilityId == facilityChoice && rs.TimeSlot == timeSlotChoice && rs.BookingDate == bookingDate select rs).FirstOrDefault();
                if (resource != null)
                {
                    MessageBox.Show(String.Format("The sports facility \"{0}\" at time slot {1} is not available.", resource.FacilityId.ConvertIndexToString(resource, 1, (resource.FacilityId - 1)), resource.TimeSlot.ConvertIndexToString(resource, 2, (resource.TimeSlot - 1))),
                        "Sports Booking", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            ResourceSport cartItem = Cart.userCart.OfType<ResourceSport>().Where(o => o.facilityChoice.Equals(facilityChoice) && o.bookingSlot.Equals(timeSlotChoice) && o.bookingDate.Equals(bookingDate)).FirstOrDefault();
            if (cartItem != null) // Check user cart. Null return means no conflict found in cart
            {
                MessageBox.Show(String.Format("The conflicted with an item(s) in your cart.{0}The facility \"{3}\" is already booked on {1} at time slot {2}",
                            Environment.NewLine, cartItem.bookingDate.ToString("dd/MM/yyyy"), cartItem.bookingSlot.ConvertIndexToString(cartItem, 2, (cartItem.bookingSlot - 1)), cartItem.facilityChoice.ConvertIndexToString(cartItem, 1, cartItem.facilityChoice - 1),
                            "Hotel Rooms Booking", MessageBoxButton.OK, MessageBoxImage.Information));
                return;
            }

            decimal currentPrice = Price.CalculateSportBookingPrice(bookingDate, facilityChoice, timeSlotChoice, duration);
            MessageBoxResult confirmAdd = MessageBox.Show(String.Format("Your current selection costs {0:C2}{1}Confirm add to cart?"
                , currentPrice, Environment.NewLine), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmAdd == MessageBoxResult.Yes)
            {
                if (!comboBoxSportBookingDuration.IsEnabled)
                    Order.orderObject = new ResourceSport(bookingDate, facilityChoice, timeSlotChoice, currentPrice);
                else
                    Order.orderObject = new ResourceSport(bookingDate, facilityChoice, timeSlotChoice, duration, currentPrice);

                Order.orderObject.CreateOrder();
            }
        }

        private void checkBoxSportBookingDuration_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxSportBookingDuration.IsEnabled = true;
        }
        private void checkBoxSportBookingDuration_Unchecked(object sender, RoutedEventArgs e)
        {
            comboBoxSportBookingDuration.IsEnabled = false;
        }

        private void buttonSportClearSelection_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("Clear all selection?", "Clear Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r.Equals(MessageBoxResult.Yes))
                LoadDefaultSelections();
        }

        private void buttonSportAvailability_Click(object sender, RoutedEventArgs e)
        {
            int selectedFacility = this.comboBoxSportFacilityChoice.SelectedIndex + 1;
            string selectedFacilityText = this.comboBoxSportFacilityChoice.Items.GetItemAt(this.comboBoxSportFacilityChoice.SelectedIndex).ToString();

            using (context = new BookingSystemDBEntities())
            {
                // Get list of booking for selected room ; (rs.CheckInDate >= DateTime.Today) to get only relevant results
                List<SportBooking> bookingList = (from rs in context.SportBookings where rs.FacilityId == selectedFacility && rs.BookingDate >= DateTime.Today select rs).ToList();
                List<ResourceSport> cartBookingList = Cart.userCart.OfType<ResourceSport>().Where(rs => rs.facilityChoice == selectedFacility).ToList();
                string output = String.Format("Booked Periods for {1}:{0}{0}", Environment.NewLine, selectedFacilityText);
                int count = 1;
                if (bookingList.Count != 0)
                {
                    foreach (SportBooking bk in bookingList)
                    {
                        output += String.Format("{3}. Facility is unavailable on {1} at time slot {2}{0}{0}", Environment.NewLine, bk.BookingDate.ToString("dd MMMM yyyy"), bk.TimeSlot.ConvertIndexToString(bk, 2, bk.TimeSlot - 1), count++);
                    }
                }
                if (cartBookingList.Count != 0)
                {
                    foreach (ResourceSport rs in cartBookingList)
                    {
                        output += String.Format("{3}.(Item in cart) Facility is unavailable on {1} at time slot {2}{0}{0}", Environment.NewLine, rs.bookingDate.ToString("dd MMMM yyyy"), rs.bookingSlot.ConvertIndexToString(rs, 2, rs.bookingSlot - 1), count++);
                    }
                }
                if ((bookingList.Count == 0) && (cartBookingList.Count == 0))
                {
                    output = String.Format("The facility \"{0}\" has not been booked by any user and is fully available for booking.", selectedFacilityText);
                }
                MessageBox.Show(output, "Sport Facility Availability", MessageBoxButton.OK, MessageBoxImage.None);
            }
        }
    }
}