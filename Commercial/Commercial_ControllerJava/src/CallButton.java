import java.util.ArrayList;
import java.util.List;

import static java.lang.System.out;

/**
 *  Elevator call button - one on each floor, on each column, for each elevator of that column.
 *  Brings you to the ground floor (RC).
 */
public class CallButton
{
    //region FIELDS
    private Integer floor;
    private Column column;
    private String direction;
    private Boolean isToggled;
    private Boolean isEmittingLight;
    //endregion

    //region PROPERTIES - Getters
    public Integer getFloor()
    {
        return floor;
    }

    public String getDirection()
    {
        return direction;
    }

    public Column getColumn()
    {
        return column;
    }
    //endregion

    //region PROPERTIES - Setters
    public void setFloor(Integer floor)
    {
        if (floor > Battery.NumFloors || floor < -(Battery.NumBasements))
            throw new RuntimeException("The floor value provided for the call button is invalid.");
        else
            this.floor = floor;
    }

    private void setDirection()
    {
        if (this.floor > 1)
            this.direction = "down";
        else
            this.direction = "up";
    }

    public void setColumn(Column column)
    {
        this.column = column;
    }


    //endregion\

    //region CONSTRUCTOR
    public CallButton(int floor, Column column)
    {
        setFloor(floor);
        setColumn(column);
        this.isToggled = false;
        this.isEmittingLight = false;
    }
    //endregion

    //region METHODS
    // Send request to chosen elevator + return its value for further use.
    public Elevator press()
    {
        this.setDirection();

        out.println("\n\nELEVATOR REQUEST - FROM A CALL BUTTON");
        Program.sleep();

        // Print the important details of the request
        if (this.floor > 1)
            out.printf("Someone is on floor %d and will now have to go %s to RC (floor %d). This person decides to call an elevator.", this.floor, this.direction, Elevator.OriginFloor);
        else
            out.printf("Someone is on floor B%d (floor %d) and will now have to go %s to RC (floor %d). This person decides to call an elevator.", Math.abs(this.floor), this.floor, this.direction, Elevator.OriginFloor);
        Program.sleep();

        // Turn on the pressed button's light
        this.isToggled = true;
        this.controlLight();


        // Get the chosen elevator and send it the request, if at least 1 elevator has a status of 'online'
        var chosenElevator = this.chooseElevator();
        if (chosenElevator.equals(null))
            out.println("All of our elevators are currently undergoing maintenance, sorry for the inconvenience.");
        else
            chosenElevator.sendRequest(this.floor, this.direction);

        // Turn off the pressed button's light
        this.isToggled = false;
        this.controlLight();

        return chosenElevator;
    }

    // Light up a pressed button
    private void controlLight()
    {
        this.isEmittingLight = this.isToggled;
    }

    // Choose which elevator should be called
    public Elevator chooseElevator()
    {
        List<Integer> elevatorScores = new ArrayList<Integer>();

        for (var elevator : this.getColumn().getElevatorList())
        {
            // Initialize score to 0
            Integer score = 0;

            // Calculate floor difference differently based on whether or not elevator is already at RC or not
            // Remember: Each column has a floor range that its elevators must respect. The RC is not included in the range, so to make the calculations fair, if elevator is already at RC the floor difference will still look normal thanks to the 2nd calculation option as it will start from the column's lowest floor instead of at RC.
            Integer floorDifference;

            if (elevator.getCurrentFloor() != Elevator.OriginFloor)
                floorDifference = elevator.getCurrentFloor() - this.floor;
            else
                floorDifference = this.column.getLowestFloor() - this.floor;

            // Prevents use of any offline/under-maintenance elevators
            if (elevator.getStatus() != "online")
            {
                score = -1;
                elevatorScores.add(score);
            }
            else
            {
                // Bonify score based on floor difference
                if (floorDifference.equals(0))
                    score += 5000;
                else
                    score += 5000 / (Math.abs(floorDifference) + 1);

                // Bonify score based on direction (highest priority)
                if (elevator.getMovement() != "idle")
                {
                    if (floorDifference >= 0 && this.direction.equals("down") && elevator.getMovement().equals("down"))
                    {
                        // Paths are crossed going down, therefore favor this elevator
                        score += 10000;
                    }
                    else if (floorDifference <= 0 && this.direction.equals("up") && elevator.getMovement().equals("up"))
                    {
                        // Paths are crossed going up, therefore favor this elevator
                        score += 10000;
                    }
                    else
                    {
                        // Paths are not crossed, therefore try avoiding the use of this elevator
                        score = 0;

                        // Calculate next floor difference differently based on whether or not elevator's next floor will be at RC or not
                        Integer nextFloorDifference;
                        if (elevator.getNextFloor() != Elevator.OriginFloor)
                            nextFloorDifference = elevator.getNextFloor() - this.floor;
                        else
                            nextFloorDifference = this.column.getLowestFloor() - this.floor;

                        // Give redemption points, in worst case scenario where all elevators never cross paths
                        if (nextFloorDifference.equals(0))
                            score += 500;
                        else
                            score += 500 / (Math.abs(nextFloorDifference) + 1);
                    }
                }

                // Bonify score on request queue size (the smaller number of pre-existing requests, the faster therefore the better)
                if (elevator.getRequestsQueue().size() <= 3)
                    score += 1000;
                else if (elevator.getRequestsQueue().size() <= 7)
                    score += 250;

                // Send total score of elevator to the scores list
                elevatorScores.add(score);
            }
        }

        // Get value of highest score
        Integer highestScore = -1;

        for (int score : elevatorScores)
        {
            if (score > highestScore)
                highestScore = score;
        }

        // Get the elevator with the highest score (or NULL if all elevators were offline)
        Elevator chosenElevator = null;
        if (highestScore > -1)
        {
            for (int i = 0; i < elevatorScores.size(); i++)
            {
                if (elevatorScores.get(i).equals(highestScore))
                {
                    chosenElevator = this.column.getElevatorList().get(i);
                }
            }
            out.printf("\nChosen elevator of Column %d: Elevator %d", column.getId(), chosenElevator.getId());
        }
        return chosenElevator;
    }
    //endregion

}
