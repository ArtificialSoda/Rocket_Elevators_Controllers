using System;

namespace Commercial_Controller
{
    class Request
    {
        #region FIELDS
        private int _floor;
        private string _direction;
        #endregion

        #region PROPERTIES
        public int Floor 
        {
            get { return _floor; }
            private set
            {
                if (value > Battery.NumFloors || value < -(Battery.NumBasements))
                    throw new Exception("The floor value provided for the request is invalid.");
                else
                    _floor = value;
            }
        }
        public string Direction
        {
            get { return _direction; }
            private set
            {
                if (value.ToLower() != "up" || value.ToLower() != "down")
                    throw new Exception("The direction provided for the request is invalid. It can only be 'up' or 'down'.");
                else
                    _direction = value;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public Request(int floor_, string direction_)
        {
            Floor = floor_;
            Direction = direction_;
        }
        #endregion
    }
}
