using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssignmentS2P2
{
    class ResourceSport : Order
    {
        // Instances of a Sport Facility Booking

        internal DateTime bookingDate;
        internal int facilityChoice;
        internal int bookingSlot;
        internal int bookingDuration;

        internal ResourceSport() : base(LoginWindow.session.username, Decimal.Zero) { }

        // Creates new instance of sports booking
        internal ResourceSport(DateTime _bookingDate, int _facility, int _bookingSlot, decimal _price) : base(LoginWindow.session.username, _price)
        {
            bookingDate = _bookingDate;
            facilityChoice = _facility;
            bookingSlot = _bookingSlot;
            bookingDuration = 2;
        }

        // With custom booking duration
        internal ResourceSport(DateTime _bookingDate, int _facility, int _bookingSlot, int _bookingDuration, decimal _price) : base(LoginWindow.session.username, _price)
        {
            bookingDate = _bookingDate;
            facilityChoice = _facility;
            bookingSlot = _bookingSlot;
            bookingDuration = _bookingDuration;
        }

        internal override void CreateOrder() // Add instance to cart
        {
            Cart.AddItem(this);
        }
    }
}