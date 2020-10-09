using System;
using static System.Threading.Thread;
using static System.Console;

namespace Commercial_ControllerCS
{
    class ElevatorDoor
    {
        #region PROPERTIES
        public string Status { get; private set; }
        #endregion

        #region CONSTRUCTOR
        public ElevatorDoor(string status_)
        {
            Status = status_;
        }
        #endregion

        #region METHODS
        public void OpenDoor()
        {
            Status = "opened";
            WriteLine("\nDoor has opened");
            Sleep(Program.SLEEP_TIME);
        }

        public void CloseDoor()
        {
            Status = "closed";
            WriteLine("Door has closed\n");
            Sleep(Program.SLEEP_TIME);
        }
        #endregion
    }
}
