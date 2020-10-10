import java.util.ArrayList;
import java.util.List;
import static java.lang.System.out;

/**
 * Entrance board button - there's one assigned to each floor of the building.
 * It will decide which elevator of which column to call for you.
 */
public class BoardButton
{
    //region FIELDS
    private Battery battery;
    private Integer requestedFloor;
    private String  direction;
    private Boolean isToggled;
    private Boolean isEmittingLight;
    //endregion

    //region PROPERTIES - Getters
    public Integer getRequestedFloor()
    {
        return requestedFloor;
    }

    public String getDirection()
    {
        return direction;
    }

    public Battery getBattery() {
        return battery;
    }

    //endregion

    //region PROPERTIES - Setters
    public void setRequestedFloor(Integer requestedFloor)
    {
        if (requestedFloor > Battery.NumFloors || requestedFloor < -(Battery.NumBasements))
            throw new RuntimeException("The floor value provided for the call button is invalid.");
        else
            this.requestedFloor = requestedFloor;
    }

    private void setDirection()
    {
        Integer floorDifference = this.requestedFloor - Elevator.OriginFloor;
        if (floorDifference > 0)
            this.direction = "up";
        else
            this.direction = "down";
    }
    //endregion

    //region CONSTRUCTOR
    public BoardButton(int requestedFloor, Battery battery)
    {
        this.setRequestedFloor(requestedFloor);
        this.battery = battery;
        this.isToggled = false;
        this.isEmittingLight = false;
    }
    //endregion

    //region METHODS
    // Send request to chosen elevator + return its value for further use.
    public Elevator press()
    {
        this.setDirection();

        out.println("\n\nELEVATOR REQUEST - FROM A BOARD BUTTON");
        Program.sleep();

        // Print the important details of the request
        if (this.requestedFloor > 1)
            out.printf("Someone is at RC (floor %d) and wants to go %s to floor %d. This person decides to call an elevator.", Elevator.OriginFloor, this.direction, this.requestedFloor);
        else
            out.printf("Someone is at RC (floor %d) and wants to go %s to floor B%d (floor %d). This person decides to call an elevator.", Elevator.OriginFloor, this.direction, Math.abs(this.requestedFloor), this.requestedFloor);
        Program.sleep();

        // Turn on the pressed button's light
        this.isToggled = true;
        this.controlLight();


        // Get the chosen elevator and send it the request, if at least 1 elevator has a status of 'online'
        var chosenElevator = this.chooseElevator();
        if (chosenElevator.equals(null))
            out.println("All of our elevators are currently undergoing maintenance, sorry for the inconvenience.");
        else
            chosenElevator.sendRequest(Elevator.OriginFloor, this.direction);

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

    // Choose which column to go to, based on the requested floor
    private Column chooseColumn()
    {
        for (var column : this.battery.getColumnList())
        {
            if (this.requestedFloor >= column.getLowestFloor() && this.requestedFloor <= column.getHighestFloor())
            {
                out.printf("\nChosen column: Column %d", column.getId());
                return column;
            }
        }
        throw new RuntimeException("None of columns go to that specified requested floor. Fix the floor ranges.");
    }

    // Choose which elevator should be called
    public Elevator chooseElevator()
    {
        List<Integer> elevatorScores = new ArrayList<Integer>();
        var chosenColumn = this.chooseColumn();


        for (var elevator : chosenColumn.getElevatorList())
        {
            // Initialize score to 0
            Integer score = 0;

            // Calculate floor difference differently based on whether or not elevator is already at RC or not
            // Remember: Each column has a floor range that its elevators must respect. The RC is not included in the range, so to make the calculations fair, if elevator is already at RC the floor difference will still look normal thanks to the 2nd calculation option as it will start from the column's lowest floor instead of at RC.
            Integer floorDifference;

            if (elevator.getCurrentFloor() != Elevator.OriginFloor)
                floorDifference = elevator.getCurrentFloor() - chosenColumn.getLowestFloor();
            else
                floorDifference = 0;

            // Prevents use of any offline/under-maintenance elevators
            if (elevator.getStatus() != "online")
            {
                score = -1;
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
                            nextFloorDifference = elevator.getNextFloor() - chosenColumn.getLowestFloor();
                        else
                            nextFloorDifference = 0;

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
            }
            // Send total score of elevator to the scores list
            elevatorScores.add(score);
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
                    chosenElevator = chosenColumn.getElevatorList().get(i);
                }
            }
            out.printf("\nChosen elevator of Column %d: Elevator %d", chosenColumn.getId(), chosenElevator.getId());
        }
        return chosenElevator;
    }
    //endregion

}
