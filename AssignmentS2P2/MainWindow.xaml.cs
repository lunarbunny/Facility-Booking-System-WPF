using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AssignmentS2P2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // User's username. Prevents change during active session.
        internal readonly string username;
        private BookingSystemDBEntities context;

        private Thread cartAutoUpdater, bookingHistoryAutoUpdater;
        internal MainWindow(string _user, bool _admin)
        {
            InitializeComponent();
            this.username = _user;

            // Disable booking summary button if user is not an admin
            if (!_admin)
            {
                this.buttonViewSummary.IsEnabled = false;
                this.buttonViewSummary.ToolTip = "You need to be an administrator to view summary.";
            }

            // Show logged in user and tooltip
            this.labelSessionUser.Content = this.labelSessionUser.ToolTip = this.username;

            // Create a new transaction for post-checkout receipt processing (Tracks session date & time, time spent)
            UserTransaction.transactionSession = new UserTransaction();

            // Loads price of resources / Control Resources
            Price.LoadPrice();
            ResourceCollection.LoadHotelControlResources();
            ResourceCollection.LoadSportControlResources();

            // Auto update threads
            cartAutoUpdater = new Thread(new ThreadStart(CartAutoUpdate));
            bookingHistoryAutoUpdater = new Thread(new ThreadStart(BookingHistoryAutoUpdate));
            cartAutoUpdater.Start();
            bookingHistoryAutoUpdater.Start();
        }

        // ========================= Navigation =========================
        private Uri uriHotel, uriSport;
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_HotelTab.IsSelected)
            {
                if (uriHotel == null) uriHotel = new Uri(@"UserControls\HotelBookingControl.xaml", UriKind.Relative);
                if (_HotelNavFrame.Visibility == Visibility.Hidden) _HotelNavFrame.Visibility = Visibility.Visible;
                _SportNavFrame.Visibility = Visibility.Hidden;
                this._HotelNavFrame.Navigate(uriHotel);
            }
            if (_SportTab.IsSelected)
            {
                if (uriSport == null) uriSport = new Uri(@"UserControls\SportBookingControl.xaml", UriKind.Relative);
                if (_SportNavFrame.Visibility == Visibility.Hidden) _SportNavFrame.Visibility = Visibility.Visible;
                _HotelNavFrame.Visibility = Visibility.Hidden;
                this._SportNavFrame.Navigate(uriSport);
            }
        }

        // ========================= Threading Methods =========================
        // Auto update cart/booking history window every 5/30 seconds respectively as background thread
        // Also updates total price shown for cart
        private void CartAutoUpdate()
        {
            while (cartAutoUpdater.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(5000);
                Dispatcher.Invoke(new Action(() =>
                {
                    buttonUpdateCart_Click(this, new RoutedEventArgs());
                    this.labelCartTotalPrice.Content = String.Format("Total Price: {0:C2}", Cart.GetCartTotalPrice());
                    if (this.listBoxCart.SelectedIndex == -1 || !Cart.userCart.Any()) // Clear detail window if no order is selected
                        this.listBoxCartItemInfo.ItemsSource = null;
                }));
            }
        }

        private void BookingHistoryAutoUpdate()
        {
            while (bookingHistoryAutoUpdater.ThreadState != ThreadState.Stopped)
            {
                Thread.Sleep(30000);
                Dispatcher.Invoke(new Action(() =>
                {
                    buttonRetrieveBookingHistory_Click(this, new RoutedEventArgs());
                    if (this.listBoxBookingHistory.SelectedIndex == -1) // Clear receipt window if no order is selected
                        this.listBoxBookingHistoryDetail.ItemsSource = null;
                }));
            }
        }

        // ========================= Main Event Handlers =========================
        private void buttonCheckout_Click(object sender, RoutedEventArgs e) // Checkout button
        {
            if (Cart.GetCartCount() == 0)
            {
                MessageBox.Show("Your cart is empty.", "Checkout", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBoxResult r = MessageBox.Show(String.Format("You have {1} items in your cart totaling {2:C2}{0}Continue checkout?", Environment.NewLine, Cart.GetCartCount(), Cart.GetCartTotalPrice()),
                "Checkout", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (r == MessageBoxResult.Yes)
            {
                Cart.Checkout();
                Cart.Clear();

                buttonRetrieveBookingHistory_Click(this, new RoutedEventArgs());
                MessageBox.Show("Thank you for your order!" + Environment.NewLine + "You can view your bookings and receipts at \"My Account\" Tab.",
                    "Checkout", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void buttonClearCart_Click(object sender, RoutedEventArgs e) // Clear cart button
        {
            MessageBoxResult r = MessageBox.Show("Confirm removal of all items in your cart?",
                "Cart", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
                Cart.Clear();
        }

        private void buttonLogout_Click(object sender, RoutedEventArgs e) // Logout button
        {
            MessageBoxResult r = MessageBox.Show("Confirm Logout?",
                "Cart", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
            {
                Cart.Clear();

                // Stop autoupdate threads
                cartAutoUpdater.Abort();
                bookingHistoryAutoUpdater.Abort();

                if (summary != null) summary.Close();
                this.Hide();
                LoginWindow.session = null;
                LoginWindow.login = new LoginWindow();
                LoginWindow.login.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e) // Close(X) button
        {
            // Stop autoupdate threads & Exit
            cartAutoUpdater.Abort();
            bookingHistoryAutoUpdater.Abort();
            Application.Current.Shutdown();
        }

        // ========================= Cart Tab Event Handlers =========================
        private bool preventSelectionEvent; // Stop all selection change event in cart and booking history window when true

        private void buttonUpdateCart_Click(object sender, RoutedEventArgs e) // Update Cart button
        {
            preventSelectionEvent = true; // Updating cart triggers selection changed event after clearing listbox
            int i = this.listBoxCart.SelectedIndex; // Save selected item else update will remove selection

            this.listBoxCart.Items.Clear();
            if (Cart.userCart.Any())
            {
                foreach (Order x in Cart.userCart)
                {
                    ListBoxItem listItem = new ListBoxItem();
                    listItem.Content = x.GetOrderCount(); // Extension to count order index in cart (Eg. 1st item is Order 1, 2nd is Order 2 and so on)
                    this.listBoxCart.Items.Add(listItem);
                }
            }
            else
            {
                this.listBoxCart.Items.Add(new ListBoxItem { Content = "Your cart is empty." });
            }

            listBoxCart.SelectedIndex = i;
            preventSelectionEvent = false;
        }

        private void listBoxCart_SelectionChanged(object sender, SelectionChangedEventArgs e) // Cart item selection change event to see item detail
        {
            if (preventSelectionEvent || Cart.userCart.Count == 0)
                return;
            int index = this.listBoxCart.SelectedIndex;
            var selectedOrder = Cart.userCart[index];
            this.listBoxCartItemInfo.ItemsSource = selectedOrder.GetOrderInfo(true); // Extension to get data of an order instance
        }

        private void buttonRemoveItem_Click(object sender, RoutedEventArgs e) // Remove cart item button
        {
            if (this.listBoxCart.SelectedIndex == -1)
            {
                MessageBox.Show("Please choose an item from your cart to remove.");
                return;
            }
            Order item = Cart.userCart[this.listBoxCart.SelectedIndex];
            MessageBoxResult r = MessageBox.Show(String.Format("Confirm removal of {0}?", item.GetOrderCount()),
                "Cart", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.Yes)
            {
                Cart.RemoveItem(item);
                buttonUpdateCart_Click(this, new RoutedEventArgs());
            }
        }

        // ========================= My Account Tab Event Handlers =========================

        List<HotelBooking> hotelHistory;
        List<SportBooking> sportHistory;
        private void buttonRetrieveBookingHistory_Click(object sender, RoutedEventArgs e) // Retrieve user booking history button
        {
            preventSelectionEvent = true; // Updating cart triggers selection changed event after clearing listbox
            int i = this.listBoxBookingHistory.SelectedIndex; // Save selected item else update will remove selection

            this.listBoxBookingHistory.Items.Clear();
            using (context = new BookingSystemDBEntities())
            {
                hotelHistory = (from bk in context.HotelBookings where bk.Booking_User == this.username select bk).ToList();
                sportHistory = (from bk in context.SportBookings where bk.Booking_User == this.username select bk).ToList();
                int count = 1;
                if (hotelHistory.Any())
                {
                    foreach (HotelBooking x in hotelHistory)
                    {
                        ListBoxItem listItem = new ListBoxItem();
                        listItem.Content = String.Format("Transaction {0} (Hotel Order)", count++);
                        this.listBoxBookingHistory.Items.Add(listItem);
                    }
                }
                if (sportHistory.Any())
                {
                    foreach (SportBooking y in sportHistory)
                    {
                        ListBoxItem listItem = new ListBoxItem();
                        listItem.Content = String.Format("Transaction {0} (Sport Order)", count++);
                        this.listBoxBookingHistory.Items.Add(listItem);
                    }
                }
                if (!hotelHistory.Any() && !sportHistory.Any())
                    this.listBoxBookingHistory.Items.Add(new ListBoxItem { Content = "You have no booking history." });
            }

            listBoxBookingHistory.SelectedIndex = i;
            preventSelectionEvent = false;
        }

        private void listBoxBookingHistory_SelectionChanged(object sender, SelectionChangedEventArgs e) // Booking history selection change
        {
            if (preventSelectionEvent || (hotelHistory.Count == 0 && sportHistory.Count == 0))
                return;
            int index = this.listBoxBookingHistory.SelectedIndex;
            string endOfReceipt = "===== Thank you for your purchase! =====";
            using (context = new BookingSystemDBEntities())
            {
                if (listBoxBookingHistory.Items.GetItemAt(index).ToString().Contains("Hotel Order"))
                {
                    HotelBooking selectedOrder = hotelHistory.ElementAt(index);
                    Transaction selectedOrderTransaction = (from trans in context.Transactions where trans.TransactionID == selectedOrder.TransactionID select trans).FirstOrDefault();
                    List<string> detailList = selectedOrder.GetOrderInfo(); // Extension to get data of an order instance

                    detailList.Add(String.Empty);
                    detailList.Add(String.Format("Transaction Date: {0}", selectedOrderTransaction.TransactionDate.ToString("dd/MM/yyyy")));
                    detailList.Add(String.Empty);
                    detailList.Add(String.Format("Transaction Started: {0}", selectedOrderTransaction.SessionStart.ToString("hh:mm:ss tt")));
                    detailList.Add(String.Format("Transaction Ended: {0}", selectedOrderTransaction.SessionEnd.ToString("hh:mm:ss tt")));
                    detailList.Add(String.Format("Transaction Time Eclapsed: {0}", selectedOrderTransaction.SessionDuration.ToReadableString()));
                    detailList.Add(String.Empty);
                    detailList.Add(String.Format("Subtotal: {0:C2}", selectedOrder.Price));
                    detailList.Add(String.Empty);
                    detailList.Add(endOfReceipt);

                    this.listBoxBookingHistoryDetail.ItemsSource = detailList;
                }
                else if (listBoxBookingHistory.Items.GetItemAt(index).ToString().Contains("Sport Order"))
                {
                    index -= hotelHistory.Count; // Subtracted to get the correct index because listbox's index consists of both Hotel and Sport while sportHistory collection only has sport bookings
                    SportBooking selectedOrder = sportHistory.ElementAt(index);
                    Transaction selectedOrderTransaction = (from trans in context.Transactions where trans.TransactionID == selectedOrder.TransactionID select trans).FirstOrDefault();
                    List<string> detailList = selectedOrder.GetOrderInfo(); // Extension to get data of an order instance

                    detailList.Add(String.Empty);
                    detailList.Add(String.Format("Transaction Date: {0}", selectedOrderTransaction.TransactionDate.ToString("dd/MM/yyyy")));
                    detailList.Add(String.Empty);
                    detailList.Add(String.Format("Transaction Started: {0}", selectedOrderTransaction.SessionStart.ToString("hh:mm:ss tt")));
                    detailList.Add(String.Format("Transaction Ended: {0}", selectedOrderTransaction.SessionEnd.ToString("hh:mm:ss tt")));
                    detailList.Add(String.Format("Transaction Time Eclapsed: {0}", selectedOrderTransaction.SessionDuration.ToReadableString()));
                    detailList.Add(String.Empty);
                    detailList.Add(String.Format("Subtotal: {0:C2}", selectedOrder.Price));
                    detailList.Add(String.Empty);
                    detailList.Add(endOfReceipt);

                    this.listBoxBookingHistoryDetail.ItemsSource = detailList;
                }
            }
        }

        private SummaryWindow summary;
        private void buttonViewSummary_Click(object sender, RoutedEventArgs e) // View Summary Button
        {
            summary = new SummaryWindow();
            summary.Show();
        }

        private void buttonPrintReceipt_Click(object sender, RoutedEventArgs e) // Print Receipt Button
        {
            if (listBoxBookingHistory.SelectedIndex.Equals(-1))
            {
                MessageBox.Show("Please choose a transaction to print", "Print Receipt", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            MessageBoxResult cfmPrint = MessageBox.Show("Print receipt? (It's advisable to click No)", "Print Receipt", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (cfmPrint == MessageBoxResult.Yes)
            {
                string printOutput = String.Empty;
                foreach (var x in listBoxBookingHistoryDetail.Items)
                {
                    printOutput += x.ToString() + Environment.NewLine;
                }
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrintPage += delegate (object _sender, PrintPageEventArgs _e)
                {
                    _e.Graphics.DrawString(printOutput, new Font("Segoe UI", 14), Brushes.Black, 10, 25);
                };
                printDocument.Print();
            }
        }
    }
}