using System;
using System.Linq;
using System.Windows;

namespace AssignmentS2P2
{
    /// <summary>
    /// Interaction logic for SummaryWindow.xaml
    /// </summary>
    public partial class SummaryWindow : Window
    {
        public SummaryWindow()
        {
            InitializeComponent();
        }

        // All labels with *FULL CAPS* content in designer means it should be overridden in Window_Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (BookingSystemDBEntities context = new BookingSystemDBEntities())
            {
                // ===== Weekly Tab =====
                // Current Week - Present
                DateTime currentWeekStartDate = DateTime.Today.AddDays(-((int)DateTime.Today.DayOfWeek));
                int weeklyHotelTotalBookings = (from bk in context.HotelBookings
                                                where bk.Transaction.TransactionDate >= currentWeekStartDate
                                                select bk).ToList().Count;
                double weeklyHotelAvgBookings = weeklyHotelTotalBookings / 7.0;

                this.labelWeeklyCurrentWeek.Content = String.Format("Start of Week - Present ({0} - {1})", currentWeekStartDate.ToString("dd MMMM yyyy"), DateTime.Today.ToString("dd MMMM yyyy"));
                this.labelWeeklyTotalBooking.Content = String.Format("Total Bookings: ");
                this.labelWeeklyAverageBooking.Content = String.Format("Average Bookings: ");
                this.labelWeeklyTotalBooking_Value.Content = String.Format("{0} Bookings", weeklyHotelTotalBookings);
                this.labelWeeklyAverageBooking_Value.Content = String.Format("{0:N2} Bookings/Day (Total {1} Days)", weeklyHotelAvgBookings, (int)DateTime.Today.DayOfWeek);

                // Previous Week - Full week
                DateTime previousWeekStartDate = currentWeekStartDate.AddDays(-7);
                DateTime previousWeekEndDate = currentWeekStartDate.AddDays(-1);
                int weeklyPreviousHotelTotalBookings = (from bk in context.HotelBookings
                                                        where (bk.Transaction.TransactionDate >= previousWeekStartDate && bk.Transaction.TransactionDate <= previousWeekEndDate)
                                                        select bk).ToList().Count;
                double weeklyPreviousHotelAvgBookings = weeklyPreviousHotelTotalBookings / 7.0;

                this.labelWeeklyPreviousWeek.Content = String.Format("Previous Week ({0} - {1})", previousWeekStartDate.ToString("dd MMMM yyyy"), previousWeekEndDate.ToString("dd MMMM yyyy"));
                this.labelWeeklyPreviousTotalBooking.Content = String.Format("Total Bookings: ");
                this.labelWeeklyPreviousAverageBooking.Content = String.Format("Average Bookings: ");
                this.labelWeeklyPreviousTotalBooking_Value.Content = String.Format("{0} Bookings", weeklyPreviousHotelTotalBookings);
                this.labelWeeklyPreviousAverageBooking_Value.Content = String.Format("{0:N2} Bookings/Day (Total 7 Days)", weeklyPreviousHotelAvgBookings);

                // ===== Monthly Tab =====
                // Current Month to Present
                int daysInCurrentMonth = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month);
                DateTime currentMonthStartDate = DateTime.Today.AddDays(-(DateTime.Today.AddDays(-1).Day));
                int monthlyHotelTotalBookings = (from bk in context.HotelBookings
                                                 where (bk.Transaction.TransactionDate >= currentMonthStartDate)
                                                 select bk).ToList().Count;
                double monthlyHotelAvgBookings = monthlyHotelTotalBookings / 7.0;

                this.labelMonthlyCurrentMonth.Content = String.Format("Start of Month - Present ({0} - {1})", currentMonthStartDate.ToString("dd MMMM yyyy"), DateTime.Today.ToString("dd MMMM yyyy"));
                this.labelMonthlyTotalBooking.Content = String.Format("Total Bookings: ");
                this.labelMonthlyAverageBooking.Content = String.Format("Average Bookings :");
                this.labelMonthlyTotalBooking_Value.Content = String.Format("{0} Bookings", monthlyHotelTotalBookings);
                this.labelMonthlyAverageBooking_Value.Content = String.Format("{0:N2} Bookings/Day (Total {1} Days)", monthlyHotelAvgBookings, DateTime.Today.ToString("dd"));

                // Previous Month - Full month
                int previousMonthToCheck = DateTime.Today.Month - 1;
                int year = DateTime.Today.Year;
                if (previousMonthToCheck == 0) // If current month is January, previous is 12(December), not 0.
                {
                    previousMonthToCheck = 12;
                    year -= 1;
                }
                int daysInPreviousMonth = DateTime.DaysInMonth(year, previousMonthToCheck);
                DateTime previousMonthStartDate = currentMonthStartDate.AddDays(-(daysInPreviousMonth));
                DateTime previousMonthEndDate = previousMonthStartDate.AddDays((daysInPreviousMonth - 1));
                int monthlyPreviousHotelTotalBookings = (from bk in context.HotelBookings
                                                         where ((bk.Transaction.TransactionDate >= previousMonthStartDate) && (bk.Transaction.TransactionDate <= previousMonthEndDate))
                                                         select bk).ToList().Count;
                double monthlyPreviousHotelAvgBookings = monthlyPreviousHotelTotalBookings / 7.0;

                this.labelMonthlyPreviousMonth.Content = String.Format("Month of {0} ({1} - {2})", previousMonthStartDate.ToString("MMMM"), previousMonthStartDate.ToString("dd MMMM yyyy"), previousMonthEndDate.ToString("dd MMMM yyyy"));
                this.labelMonthlyPreviousTotalBooking.Content = String.Format("Total Bookings: ");
                this.labelMonthlyPreviousAverageBooking.Content = String.Format("Average Bookings: ");
                this.labelMonthlyPreviousTotalBooking_Value.Content = String.Format("{0} Bookings", monthlyPreviousHotelTotalBookings);
                this.labelMonthlyPreviousAverageBooking_Value.Content = String.Format("{0:N2} Bookings/Day (Total {1} Days)", monthlyPreviousHotelAvgBookings, daysInPreviousMonth);

                // ===== Revenue Tab =====
                // Weekly Revenue
                decimal weeklyHotelBookingsTotalRevenue = (from bk in context.HotelBookings
                                                              where bk.Transaction.TransactionDate >= currentWeekStartDate
                                                              select bk.Price).ToList().Sum();
                decimal weeklyPreviousHotelBookingsTotalRevenue = (from bk in context.HotelBookings
                                                                     where ((bk.Transaction.TransactionDate >= previousWeekStartDate) && bk.Transaction.TransactionDate <= previousWeekEndDate)
                                                                     select bk.Price).ToList().Sum();

                this.labelRevenueCurrentWeek.Content = String.Format("Current Week Revenue ({0} - {1}): ", currentWeekStartDate.ToString("dd MMMM yyyy"), DateTime.Today.ToString("dd MMMM yyyy"));
                this.labelRevenuePreviousWeek.Content = String.Format("Previous Week Revenue ({0} - {1}): ", previousWeekStartDate.ToString("dd MMMM yyyy"), previousWeekEndDate.ToString("dd MMMM yyyy"));
                this.labelRevenueCurrentWeek_Value.Content = String.Format("{0:C2}", weeklyHotelBookingsTotalRevenue);
                this.labelRevenuePreviousWeek_Value.Content = String.Format("{0:C2}", weeklyPreviousHotelBookingsTotalRevenue);

                // Monthly Revenue
                decimal monthlyHotelBookingsTotalRevenue = (from bk in context.HotelBookings
                                                            where (bk.Transaction.TransactionDate >= currentMonthStartDate)
                                                            select bk.Price).ToList().Sum();
                decimal monthlyPreviousHotelBookingsTotalRevenue = (from bk in context.HotelBookings
                                                                    where ((bk.Transaction.TransactionDate >= previousMonthStartDate) && (bk.Transaction.TransactionDate <= previousMonthEndDate))
                                                                    select bk.Price).ToList().Sum();

                this.labelRevenueCurrentMonth.Content = String.Format("Current Month Revenue ({0} - {1}): ", currentMonthStartDate.ToString("dd MMMM yyyy"), DateTime.Today.ToString("dd MMMM yyyy"));
                this.labelRevenuePreviousMonth.Content = String.Format("Previous Month Revenue ({0} - {1}): ", previousMonthStartDate.ToString("dd MMMM yyyy"), previousMonthEndDate.ToString("dd MMMM yyyy"));
                this.labelRevenueCurrentMonth_Value.Content = String.Format("{0:C2}", monthlyHotelBookingsTotalRevenue);
                this.labelRevenuePreviousMonth_Value.Content = String.Format("{0:C2}", monthlyPreviousHotelBookingsTotalRevenue);
            }
        }
    }
}