using System;
using static System.Threading.Thread;
using static System.Console;

namespace Commercial_Controller
{
    class FloorDisplay
    {
        #region FIELDS
        private readonly Elevator _elevator;
        #endregion

        #region CONSTRUCTOR
        public FloorDisplay(Elevator elevator_)
        {
            _elevator = elevator_;
        }
        #endregion

        #region METHODS
        public void DisplayFloor()
        {
            Sleep(Program.SLEEP_TIME);
            WriteLine($"... Elevator {_elevator.ID} of column {_elevator.Column.ID}'s current floor mid-travel: {_elevator.CurrentFloor} ...");
        }
        #endregion
    }
}
