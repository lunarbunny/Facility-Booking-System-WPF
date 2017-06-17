using System;

namespace AssignmentS2P2
{
    class ResourceHotel : Order
    {
        // Instances of a Hotel Room Booking

        internal DateTime checkInDate;
        internal DateTime checkOutDate;
        internal int roomID;
        internal int daysOfStay;
        internal int roomChoice;
        internal int bedChoice;
        internal int viewChoice;
        internal bool[] services;

        internal ResourceHotel() : base(LoginWindow.session.username, Decimal.Zero) { }

        // Creates new instance of hotel booking
        internal ResourceHotel(DateTime _checkInDate, DateTime _checkOutDate, int _roomID, int _daysOfStay, int _roomChoice, int _bedChoice, int _viewChoice, bool[] _services, decimal _price) : base(LoginWindow.session.username, _price)
        {
            checkInDate = _checkInDate;
            checkOutDate = _checkOutDate;
            roomID = _roomID;
            daysOfStay = _daysOfStay;
            roomChoice = _roomChoice;
            bedChoice = _bedChoice;
            viewChoice = _viewChoice;
            services = _services;
        }

        internal override void CreateOrder() // Add instance to cart
        {
            Cart.AddItem(this);
        }
    }
}