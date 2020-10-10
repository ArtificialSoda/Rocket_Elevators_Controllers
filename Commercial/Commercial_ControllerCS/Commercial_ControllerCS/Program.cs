using System;
using static System.Console;
using static System.Threading.Thread;

namespace Commercial_ControllerCS
{
    class Program
    {
        public const int SLEEP_TIME = 1000;

        static void Main(string[] args)
        {
            // Variables
            Battery.ChangeProperties(4, 60, 6);
            Column.NumElevators = 5;

            // Instantiate the batteries, the columns and the elevators
            var battery = new Battery(1);
            battery.Run();
            battery.MonitorSystem();

            Run(battery);
        }
        
        // Run the full test program
        static void Run(Battery battery)
        {
            while (true)
            {
                Console.Clear();
                Write("Hello! Choose which scenario you'd like to emulate [1, 2, 3, 4]: ");
                int.TryParse(ReadLine(), out int chosenScenario);
                Console.Clear();

                if (chosenScenario == 1)
                    Scenario1(battery, battery.ColumnList[1]); // Covers 02 to 20 + RC
                else if (chosenScenario == 2)
                    Scenario2(battery, battery.ColumnList[2]); // Covers 21 to 40 + RC
                else if (chosenScenario == 3)
                    Scenario3(battery.ColumnList[3]);   // Covers 41 to 60 + RC
                else if (chosenScenario == 4)
                    Scenario4(battery.ColumnList[0]);   // Covers B6 to B1 + RC
                else
                    continue;

                Console.ForegroundColor = ConsoleColor.Green;
                WriteLine("\n***** SCENARIO SUCCESSFULLY TESTED *****");
                Console.ResetColor();

                Write("Do you wish to re-run one of the scenarios? Type 'Y' if yes, or type anything else to exit the program: ");
                var restart = ReadLine();
                if (restart.ToUpper() == "Y")
                    continue;
                break;

            }
        }
        static void Scenario1(Battery battery, Column column)
        {
            WriteLine("**********************************************************************************************************************************\n" +
                      "SCENARIO 1\n" +
                      "**********************************************************************************************************************************"
                      );
            Sleep(SLEEP_TIME);

            column.ElevatorList[0].ChangeProperties(20, 5);
            column.ElevatorList[1].ChangeProperties(3, 15);
            column.ElevatorList[2].ChangeProperties(13, 1);
            column.ElevatorList[3].ChangeProperties(15, 2);
            column.ElevatorList[4].ChangeProperties(6, 1);

            battery.AssignElevator(20);
            Sleep(SLEEP_TIME);
        }
        static void Scenario2(Battery battery, Column column)
        {
            WriteLine("**********************************************************************************************************************************\n" +
                      "SCENARIO 2\n" +
                      "**********************************************************************************************************************************"
                      );
            Sleep(SLEEP_TIME);

            column.ElevatorList[0].ChangeProperties(1, 21);
            column.ElevatorList[1].ChangeProperties(23, 28);
            column.ElevatorList[2].ChangeProperties(33, 1);
            column.ElevatorList[3].ChangeProperties(40, 24);
            column.ElevatorList[4].ChangeProperties(39, 1);

            battery.AssignElevator(36);
            Sleep(SLEEP_TIME);
        }
        static void Scenario3(Column column)
        {
            WriteLine("**********************************************************************************************************************************\n" +
                      "SCENARIO 3\n" +
                      "**********************************************************************************************************************************"
                      );
            Sleep(SLEEP_TIME);

            column.ElevatorList[0].ChangeProperties(58, 1);
            column.ElevatorList[1].ChangeProperties(50, 60);
            column.ElevatorList[2].ChangeProperties(46, 58);
            column.ElevatorList[3].ChangeProperties(1, 54);
            column.ElevatorList[4].ChangeProperties(60, 1);

            column.RequestElevator(54);
            Sleep(SLEEP_TIME);
        }
        static void Scenario4(Column column)
        {
            WriteLine("**********************************************************************************************************************************\n" +
                      "SCENARIO 4\n" +
                      "**********************************************************************************************************************************"
                      );
            Sleep(SLEEP_TIME);

            column.ElevatorList[0].ChangeProperties(-4);
            column.ElevatorList[1].ChangeProperties(1);
            column.ElevatorList[2].ChangeProperties(-3, -5);
            column.ElevatorList[3].ChangeProperties(-6, 1);
            column.ElevatorList[4].ChangeProperties(-1, -6);

            column.RequestElevator(-3);
            Sleep(SLEEP_TIME);
        }

    }
}
