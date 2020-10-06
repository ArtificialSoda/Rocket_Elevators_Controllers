using System;
using static System.Console;

namespace Commercial_Controller
{
    /// <summary>
    /// Entrance board button - there's one assigned to each floor of the building. It will decide which elevator of which column to call for you.
    /// </summary>
    class BoardButton
    {
        #region FIELDS
        private Battery _battery;
        private int _requestedFloor;
        private int _floor;
        private string _direction;
        private bool _isToggled;
        private bool _isEmittingLight;
        #endregion

        #region PROPERTIES
        public int RequestedFloor
        {
            get { return _requestedFloor; }
            set
            {
                if (value > Battery.NumFloors || value < -(Battery.NumBasements))
                    throw new Exception("The floor value provided for the board button is invalid.");
                else
                    _requestedFloor = value;
            }
        }
        public string Direction
        {
            get { return _direction; }
            private set
            {
                if (value.ToLower() != "up" || value.ToLower() != "down")
                    throw new Exception("The direction provided for the board button is invalid. It can only be 'up' or 'down'.");
                else
                    _direction = value;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public BoardButton(int requestedFloor_, Battery battery_)
        {
            _battery = battery_;
            _isToggled = false;
            _isEmittingLight = false;
        }
        #endregion
    }
}
