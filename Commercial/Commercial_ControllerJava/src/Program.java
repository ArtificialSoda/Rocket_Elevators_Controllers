import java.util.Scanner;

import static java.lang.System.out;

public class Program {

    public static void main(String[] args)
    {
        // Variables
        Battery.ChangeProperties(4, 60, 6);
        Column.NumElevators = 5;

        // Instantiate the batteries, the columns and the elevators
        var battery = new Battery(1);
        battery.run();
        battery.monitorSystem();

        run(battery);
    }

    //  Run the full test program
    public static void run(Battery battery)
    {
        while (true)
        {
            Scanner input = new Scanner(System.in);
            out.println("Hello! Choose which scenario you'd like to emulate [1, 2, 3, 4]: ");

            String chosenScenario = input.nextLine();
            if (chosenScenario.equals("1"))
                Scenario1(battery, battery.getColumnList().get(1)); // Covers 02 to 20 + RC

            else if (chosenScenario.equals("2"))
                Scenario2(battery, battery.getColumnList().get(2)); // Covers 21 to 40 + RC

            else if (chosenScenario.equals("3"))
                Scenario3(battery.getColumnList().get(3)); // Covers 41 to 60 + RC

            else if (chosenScenario.equals("4"))
                Scenario4(battery.getColumnList().get(0)); // Covers B6 to B1 + RC

            else
                continue;

            out.println("\n***** SCENARIO SUCCESSFULLY TESTED *****");
            out.println("\nDo you wish to re-run one of the scenarios? Type 'Y' if yes, or type anything else to exit the program: ");
            String restart = input.nextLine();
            if (restart.toUpperCase().equals("Y"))
                continue;
            break;
        }
    }

    public static void Scenario1(Battery battery, Column column)
    {
        out.println("**********************************************************************************************************************************\n");
        out.println("SCENARIO 1\n");
        out.println("**********************************************************************************************************************************\n");
        sleep();

        column.getElevatorList().get(0).changeProperties(20, 5);
        column.getElevatorList().get(1).changeProperties(3, 15);
        column.getElevatorList().get(2).changeProperties(13, 1);
        column.getElevatorList().get(3).changeProperties(15, 2);
        column.getElevatorList().get(4).changeProperties(6, 1);

        battery.AssignElevator(20);
        sleep();
    }

    public static void Scenario2(Battery battery, Column column)
    {
        out.println("**********************************************************************************************************************************\n");
        out.println("SCENARIO 2\n");
        out.println("**********************************************************************************************************************************\n");
        sleep();

        column.getElevatorList().get(0).changeProperties(1, 21);
        column.getElevatorList().get(1).changeProperties(23, 28);
        column.getElevatorList().get(2).changeProperties(33, 1);
        column.getElevatorList().get(3).changeProperties(40, 24);
        column.getElevatorList().get(4).changeProperties(39, 1);

        battery.AssignElevator(36);
        sleep();
    }

    public static void Scenario3(Column column)
    {
        out.println("**********************************************************************************************************************************\n");
        out.println("SCENARIO 3\n");
        out.println("**********************************************************************************************************************************\n");
        sleep();

        column.getElevatorList().get(0).changeProperties(58, 1);
        column.getElevatorList().get(1).changeProperties(50, 60);
        column.getElevatorList().get(2).changeProperties(46, 58);
        column.getElevatorList().get(3).changeProperties(1, 54);
        column.getElevatorList().get(4).changeProperties(60, 1);

        column.requestElevator(54);
        sleep();
    }

    public static void Scenario4(Column column)
    {
        out.println("**********************************************************************************************************************************\n");
        out.println("SCENARIO 4\n");
        out.println("**********************************************************************************************************************************\n");
        sleep();

        column.getElevatorList().get(0).changeProperties(-4);
        column.getElevatorList().get(1).changeProperties(1);
        column.getElevatorList().get(2).changeProperties(-3, -5);
        column.getElevatorList().get(3).changeProperties(-6, 1);
        column.getElevatorList().get(4).changeProperties(-1, 6);

        column.requestElevator(-3);
        sleep();
    }

    // Sleeper function
    public static void sleep()
    {
        Integer ms = 1000;
        try
        {
            Thread.sleep(ms);
        }
        catch(InterruptedException ex)
        {
            Thread.currentThread().interrupt();
        }
    }
}
