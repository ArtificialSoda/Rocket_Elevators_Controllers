using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using static System.Console;

namespace Commercial_Controller
{
    class Program
    {
        public const int SLEEP_TIME = 1000;

        static void Main(string[] args)
        {
            // Variables
            Battery.NumColumns = 4;
            Battery.NumFloors = 60;
            Battery.NumBasements = 6;
            Column.NumElevators = 5;

            // Instantiate the batteries, the columns and the elevators
            var battery = new Battery(1);
            battery.CreateColumnList();
            battery.CreateBoardButtons();
            WriteLine(battery.BoardButtonList.Count);
            WriteLine(battery.BoardButtonList[6].RequestedFloor);
            battery.MonitorSystem();
        }
        static void Scenario1()
        {

        }
    }
}
