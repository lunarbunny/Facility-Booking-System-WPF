using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AssignmentS2P2
{
    /// <summary>
    /// Interaction logic for HotelBookingControl.xaml
    /// </summary>
    public partial class HotelBookingControl : UserControl
    {
        private BookingSystemDBEntities context;
        public HotelBookingControl()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e) // On window loaded
        {
            // Load resources into controls
            LoadControlResource();
            // Set default selections
            LoadDefaultSelections();
        }

        private void LoadControlResource() // Load resources into controls
        {
            this.comboBoxHotelRoomCategory.ItemsSource = new ObservableCollection<string>(ResourceCollection.hotelRoomTypes);
            this.comboBoxHotelBed.ItemsSource = new ObservableCollection<string>(ResourceCollection.hotelBedTypes);
            this.comboBoxViewType.ItemsSource = new ObservableCollection<string>(ResourceCollection.hotelRoomViews);
        }

        private bool preventSelectionEvent;
        private void LoadDefaultSelections() // Loads control default selections
        {
            preventSelectionEvent = true;
            this.datepickerCheckIn.SelectedDate = DateTime.Today;
            this.datepickerCheckOut.SelectedDate = DateTime.Today.AddDays(1);
            this.comboBoxHotelRoomCategory.SelectedIndex = 0;
            this.comboBoxHotelBed.SelectedIndex = 0;
            this.comboBoxViewType.SelectedIndex = 0;
            this.checkBoxExServiceWifi.IsChecked = false;
            this.checkBoxExServiceRoomService.IsChecked = false;
            this.checkBoxExServiceHouseKeeping.IsChecked = false;
            this.checkBoxExServiceExpressQueue.IsChecked = false;
            preventSelectionEvent = false;
        }

        private void buttonHotelAddToCart_Click(object sender, RoutedEventArgs e) // Add to cart button
        {
            // =================== DATE ===================
            // Get check in/out date
            DateTime checkInDate = datepickerCheckIn.SelectedDate.GetValueOrDefault();
            DateTime checkOutDate = datepickerCheckOut.SelectedDate.GetValueOrDefault().AddDays(1);
            int daysOfStay = ((int)Math.Ceiling((checkOutDate - checkInDate).TotalDays) + 1);
            if (checkInDate > checkOutDate)
            {
                MessageBox.Show("Check Out date cannot be earlier than check in date!", "Error: You're not a time traveller", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (checkInDate < DateTime.Today)
            {
                MessageBox.Show("Check In date cannot be earlier than today's date!", "Error: You're not a time traveller", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // =================== ROOM & BED & VIEW OPTION ===================
            int roomChoice = comboBoxHotelRoomCategory.SelectedIndex + 1;
            int bedChoice = comboBoxHotelBed.SelectedIndex + 1;
            int viewChoice = comboBoxViewType.SelectedIndex + 1;
            // =================== SERVICES ===================
            bool[] services = { checkBoxExServiceWifi.IsChecked.GetValueOrDefault(), checkBoxExServiceRoomService.IsChecked.GetValueOrDefault()
                    , checkBoxExServiceHouseKeeping.IsChecked.GetValueOrDefault(), checkBoxExServiceExpressQueue.IsChecked.GetValueOrDefault() };
            // =================== USER ROOM CHOICE ===================
            int roomIDChoice = 0;
            if (comboBoxHotelAvailableRooms.SelectedIndex != -1)
                roomIDChoice = (int)comboBoxHotelAvailableRooms.SelectedItem;

            // =====================================================================================================
            // ====================================== CONFLICT CHECKINGS ===========================================
            // =====================================================================================================
            using (context = new BookingSystemDBEntities())
            {
                // =======================================================================================
                // STEP 1: ASSIGNING USER A RANDOM AVAILABLE ROOM IF USER DID NOT CHOOSE ANY
                // Assigns a random room with user specified room class and view type to user

                if (roomIDChoice.Equals(0)) // User didn't choose a room number specifically
                {
                    // Get list of available room with given choices
                    List<HotelRoom> availableList = (from rs in context.HotelRooms where rs.TypeID == roomChoice && rs.ViewID == viewChoice select rs).ToList();
                    
                    // Randomly picks a room that matches given choices and get the particular instance
                    Random random = new Random();
                    int randomIndex = random.Next(0, availableList.Count);
                    HotelRoom resource = availableList.ElementAt(randomIndex);

                    // Get RoomID of assigned room
                    roomIDChoice = resource.RoomID;
                }

                // =======================================================================================
                // STEP 2: CONFLICT CHECKING AGAINST DATABASE & CART
                // Get list of booking made on the particular Room using RoomID gotten from above
                // This is to find the days where the room is booked to prevent date conflicts for the same room in different booking

                // Checks conflict in database ; "(checkInDate < rs.CheckOutDate)" is to display new and relevant results only
                List<HotelBooking> bookedList = (from rs in context.HotelBookings where rs.RoomID == roomIDChoice && (checkInDate < rs.CheckOutDate) select rs).ToList();
                if (bookedList.Count != 0)
                {
                    foreach (HotelBooking booking in bookedList) // For every booking made on the particular room
                    {
                        if (checkInDate >= booking.CheckInDate && checkInDate <= booking.CheckOutDate) // Check date with each booking if it conflicts
                        {
                            MessageBox.Show(String.Format("The selected room is unavailable. It has already been booked from {1} to {2}." +
                                "{0}{0}If this happens too many times, you are advised to choose a room number yourself and look up the booked period instead of relying on the system's assignment.",
                                        Environment.NewLine,booking.CheckInDate.ToString("dd MMMM yyyy"), booking.CheckOutDate.ToString("dd MMMM yyyy")), 
                                        "Hotel Rooms Booking", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }
                }
                // Checks conflict in user's cart
                List<ResourceHotel> cartList = Cart.userCart.OfType<ResourceHotel>().Where(o => o.roomID.Equals(roomIDChoice)).ToList();
                if (cartList.Count != 0)
                {
                    foreach (ResourceHotel order in cartList) // Same way of checking as above
                    {
                        if (checkInDate >= order.checkInDate && checkOutDate <= order.checkOutDate)
                        {
                            MessageBox.Show(String.Format("The selected room has a date conflict with item(s) in your cart.{0}It has already been booked from {1} to {2}." +
                                "{0}{0}If this happens too many times, you are advised to choose a room number yourself and look up the booked period instead of relying on the system's assignment.",
                                        Environment.NewLine, order.checkInDate.ToString("dd MMMM yyyy"), order.checkOutDate.ToString("dd MMMM yyyy")), 
                                        "Hotel Rooms Booking", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            return;
                        }
                    }
                }
            } // END OF CONFLICT CHECKING

            decimal currentPrice = Price.CalculateHotelBookingPrice(daysOfStay, roomChoice, bedChoice, viewChoice, services);
            MessageBoxResult confirmAdd = MessageBox.Show(String.Format("Your current selection costs {0:C2}{1}Confirm add to cart?"
                , currentPrice, Environment.NewLine), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmAdd == MessageBoxResult.Yes)
            {
                Order.orderObject = new ResourceHotel(checkInDate, checkOutDate, roomIDChoice, daysOfStay, roomChoice, bedChoice, viewChoice, services, currentPrice);
                Order.orderObject.CreateOrder();
            }
        }

        private void buttonHotelClearSelection_Click(object sender, RoutedEventArgs e) // Clear selection button
        {
            MessageBoxResult r = MessageBox.Show("Clear all selection?", "Clear Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r.Equals(MessageBoxResult.Yes))
                LoadDefaultSelections();
        }
        
        private void buttonHotelGetAvailableRooms_Click(object sender, RoutedEventArgs e) // Get available rooms button
        {
            using (context = new BookingSystemDBEntities())
            {
                this.comboBoxHotelAvailableRooms.Items.Clear();
                this.comboBoxHotelAvailableRooms.Text = "Choose a Room";

                // Get list of available room with given choice
                List<HotelRoom> availableList = (from rs in context.HotelRooms where
                    rs.TypeID == (comboBoxHotelRoomCategory.SelectedIndex + 1) && rs.ViewID == (comboBoxViewType.SelectedIndex + 1) select rs).ToList();
                foreach (HotelRoom res in availableList) // Add each room instance's RoomId to combobox
                {
                    this.comboBoxHotelAvailableRooms.Items.Add(res.RoomID);
                }
            }
        }

        private void buttonRoomAvailability_Click(object sender, RoutedEventArgs e) // Room Availability Button (Get days where a specific room is booked)
        {
            if (this.comboBoxHotelAvailableRooms.SelectedIndex.Equals(-1))
            {
                MessageBox.Show("You need to choose a room number first to get it's availability.", "Availability", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int selectedRoom = (int)this.comboBoxHotelAvailableRooms.Items.GetItemAt(this.comboBoxHotelAvailableRooms.SelectedIndex);

            using (context = new BookingSystemDBEntities())
            {
                // Get list of booking for selected room ; (rs.CheckInDate >= DateTime.Today) to get only relevant results
                List<HotelBooking> bookingList = (from rs in context.HotelBookings where rs.RoomID == selectedRoom && rs.CheckInDate >= DateTime.Today select rs).ToList();
                List<ResourceHotel> cartBookingList = Cart.userCart.OfType<ResourceHotel>().Where(rs => rs.roomID == selectedRoom).ToList();
                string output = String.Format("Booked Periods for Room {1}:{0}{0}", Environment.NewLine, selectedRoom);
                int count = 1;
                if (bookingList.Count != 0)
                {
                    foreach (HotelBooking bk in bookingList)
                    {
                        output += String.Format("{3}. Room is unavailable from {1} to {2}{0}{0}", Environment.NewLine, bk.CheckInDate.ToString("dd MMMM yyyy"), bk.CheckOutDate.ToString("dd MMMM yyyy"), count++);
                    }
                }
                if (cartBookingList.Count != 0)
                {
                    foreach (ResourceHotel rs in cartBookingList)
                    {
                        output += String.Format("{3}.(Item in cart) Room is unavailable from {1} to {2}{0}{0}", Environment.NewLine, rs.checkInDate.ToString("dd MMMM yyyy"), rs.checkOutDate.ToString("dd MMMM yyyy"), count++);
                    }
                }
                if ((bookingList.Count == 0) && (cartBookingList.Count == 0))
                {
                    output = String.Format("The room {0} has not been booked by any user and is fully available for booking.", selectedRoom);
                }
                MessageBox.Show(output, "Room Availability", MessageBoxButton.OK, MessageBoxImage.None);
            }
        }

        private void comboBoxHotel_SelectionChanged(object sender, SelectionChangedEventArgs e) // Refreshes available rooms when selection changed
        {
            if (preventSelectionEvent)
                return;
            comboBoxHotelAvailableRooms.Items.Clear();
            comboBoxHotelAvailableRooms.Text =  "← Click to list rooms";
        }
    }
}