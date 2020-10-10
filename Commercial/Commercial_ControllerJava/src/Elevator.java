import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import static java.lang.System.out;

public class Elevator implements Comparator<Request>
{
    //region STATIC PROPERTIES
    public static int OriginFloor = 1;
    public static int MaxWeightKG = 2500;
    //endregion

    //region FIELDS
    private Integer id;
    private String status;
    private String movement;
    private Integer currentFloor;
    private Integer nextFloor;
    private Column column;
    private FloorDisplay floorDisplay;
    private List<Request> requestsQueue;
    private ElevatorDoor door;
    //endregion

    //region PROPERTIES - Getters
    public Integer getId()
    {
        return id;
    }

    public String getStatus()
    {
        return status;
    }

    public String getMovement()
    {
        return movement;
    }

    public Integer getCurrentFloor()
    {
        return currentFloor;
    }

    public Integer getNextFloor()
    {
        return nextFloor;
    }

    public Column getColumn()
    {
        return column;
    }

    public FloorDisplay getFloorDisplay()
    {
        return floorDisplay;
    }

    public List<Request> getRequestsQueue()
    {
        return requestsQueue;
    }

    public ElevatorDoor getDoor()
    {
        return door;
    }
    //endregion

    //region PROPERTIES - Setters
    public void setStatus(String status)
    {
        if (status.toLowerCase() != "online" && status.toLowerCase() != "offline")
            throw new RuntimeException("Invalid value for elevator's status. Can only be either 'online' or 'offline'.");
        else
            this.status = status;
    }

    public void setMovement(String movement)
    {
        if (movement.toLowerCase() != "up" && movement.toLowerCase() != "down" && movement.toLowerCase() != "idle")
            throw new RuntimeException("Invalid value for elevator's movement. Can only be either 'up', 'down', or 'idle'.");
        else
            this.movement = movement;
    }

    public void setCurrentFloor(Integer currentFloor)
    {
        if (currentFloor > Battery.NumFloors || currentFloor < -(Battery.NumBasements))
            throw new RuntimeException("The current floor value provided for the elevator is invalid.");
        else
            this.currentFloor = currentFloor;

    }

    public void setNextFloor(Integer nextFloor)
    {
        if (nextFloor > Battery.NumFloors || nextFloor < -(Battery.NumBasements))
            throw new RuntimeException("The next floor value provided for the elevator is invalid.");
        else
            this.nextFloor = nextFloor;
    }

    public void setColumn(Column column)
    {
        this.column = column;
    }

    public void setFloorDisplay(FloorDisplay floorDisplay)
    {
        this.floorDisplay = floorDisplay;
    }

    public void setRequestsQueue(List<Request> requestsQueue)
    {
        this.requestsQueue = requestsQueue;
    }

    public void setDoor(ElevatorDoor door)
    {
        this.door = door;
    }
    //endregion

    //region CONSTRUCTOR
    public Elevator(int elevatorId, Column column)
    {
        id = elevatorId;
        setColumn(column);
        setStatus("online");
        setMovement("idle");
        setCurrentFloor(OriginFloor);
        setFloorDisplay(new FloorDisplay(this));
        setRequestsQueue(new ArrayList<Request>());
        setDoor(new ElevatorDoor("closed"));
    }
    //endregion

    //region METHODS
    // Change properties of elevator in one line - USE ONLY FOR TESTING
    public void changeProperties(Integer newCurrentFloor, Integer newNextFloor)
    {
        setCurrentFloor(newCurrentFloor);
        setNextFloor(newNextFloor);

        if (currentFloor > nextFloor)
            setMovement("down");
        else
            setMovement("up");

        getRequestsQueue().add(new Request(nextFloor, movement));
    }

    public void changeProperties(Integer newCurrentFloor)
    {
        setCurrentFloor(newCurrentFloor);
        setMovement("idle");
    }

    // Make elevator go to its scheduled next floor
    public void goToNextFloor()
    {
        if (currentFloor != nextFloor)
        {
            if (getCurrentFloor() > 0)
                out.printf("\nElevator %d of Column %d, currently at floor %d, is about to go to floor %d...", id, column.getId(), currentFloor, nextFloor);
            else if (getNextFloor() < 0)
                out.printf("\nElevator %d of Column %d, currently at floor B%d, is about to go to floor B%d...", id, column.getId(), Math.abs(currentFloor), Math.abs(nextFloor));
            else if (getNextFloor() > 0)
                out.printf("\nElevator %d of Column %d, currently at floor B%d, is about to go to floor B%d...", id, column.getId(), Math.abs(currentFloor), nextFloor);

            out.println("\n=================================================================");

            floorDisplay.displayFloor();

            // Traverse through the floors
            while (currentFloor != nextFloor)
            {
                // Do not display floors that are not part of the column's range
                if (movement.toLowerCase() == "up")
                {
                    if (currentFloor + 1 < column.getLowestFloor())
                    {
                        out.printf("\n\n... Quickly traversing through the floors not in column %d's usual elevator range ...\n", column.getId()); ;
                        currentFloor = column.getLowestFloor();
                    }
                    else
                        currentFloor++;
                }
                else
                {
                    if (currentFloor - 1 < column.getLowestFloor())
                    {
                        out.printf("\n\n... Quickly traversing through the floors not in column %d's usual elevator range ...\n", column.getId());
                        currentFloor = OriginFloor;
                    }
                    else
                        currentFloor--;
                }
                floorDisplay.displayFloor();
            }

            out.println("\n=================================================================");
            if (currentFloor > 0)
                out.printf("\nElevator %d of Column %d has reached its requested floor! It is now at floor %d.", id, column.getId(), currentFloor);
            else
                out.printf("\nElevator %d of Column %d has reached its requested floor! It is now at floor B%d.", id, column.getId(), Math.abs(currentFloor));
        }
    }

    // Make elevator go to origin floor
    public void goToOrigin()
    {
        this.nextFloor = OriginFloor;
        out.printf("\nElevator %d of Column %d going back to RC (floor %d)...", this.id, this.column.getId(), OriginFloor);
        goToNextFloor();
    }

    // Set what should be the movement direction of the elevator for its upcoming request
    private void setMovement()
    {
        int floorDifference = this.currentFloor - this.requestsQueue.get(0).getFloor();

        if (floorDifference > 0)
            this.movement = "down";
        else if (floorDifference < 0)
            this.movement = "up";
        else
            this.movement = "idle";
    }

    // Send new request to its request queue
    public void sendRequest(Integer stopFloor, String btnDirection)
    {
        var request = new Request(stopFloor, btnDirection);
        this.requestsQueue.add(request);
    }

    // Sort requests, for added efficiency
    private void sortRequestsQueue()
    {
        // Store the requests that might need to be moved or removed for efficiency purposes
        var requestsToDiscard = new ArrayList<Request>();

        // Remove any requests which are useless i.e. requests that are already on their desired floor
        for (var req : this.requestsQueue)
        {
            if (req.getFloor().equals(this.currentFloor))
                requestsToDiscard.add(req);
        }
        for (var req : requestsToDiscard)
        {
            this.requestsQueue.remove(req);
        }
        requestsToDiscard.clear();

        // Decide if elevator is going up, down or is staying idle
        this.setMovement();

        // Sort
        if (this.requestsQueue.size() > 1)
        {
            // Sort the queue in ascending order
            this.requestsQueue.sort(this);

            if (this.movement == "up")
            {
                // Push any request to the end of the queue that would require a direction change
                for (var req : this.requestsQueue)
                {
                    if (req.getDirection() != this.movement || req.getFloor() < this.currentFloor)
                        requestsToDiscard.add(req);
                }
                for (var req : requestsToDiscard)
                {
                    this.requestsQueue.remove(req);
                    this.requestsQueue.add(req);
                }
                requestsToDiscard.clear();
            }

            else
            {
                // Reverse the sorted queue (will now be in descending order)
                Collections.reverse(this.requestsQueue);

                // Push any request to the end of the queue that would require a direction change
                requestsToDiscard = new ArrayList<Request>();
                for (var req : this.requestsQueue)
                {
                    if (req.getDirection() != this.movement || req.getFloor() > this.currentFloor)
                        requestsToDiscard.add(req);
                }
                for (var req : requestsToDiscard)
                {
                    this.requestsQueue.remove(req);
                    this.requestsQueue.add(req);
                }
                requestsToDiscard.clear();
            }

        }
    }

    // Complete the elevator requests
    public void doRequests()
    {
        if (this.requestsQueue.size() > 0)
        {
            // Make sure queue is sorted before any request is completed
            this.sortRequestsQueue();
            Request requestToComplete = this.requestsQueue.get(0);

            // Go to requested floor
            if (this.door.getStatus() != "closed")
                this.door.closeDoor();
            this.nextFloor = requestToComplete.getFloor();
            this.goToNextFloor();

            // Remove request after it is complete
            this.door.openDoor();
            this.requestsQueue.remove(requestToComplete);

            // Automatically close door
            this.door.closeDoor();
        }
        // Automatically go idle temporarily if 0 requests or at the end of request
        this.movement = "idle";
    }

    // Check if elevator is at full capacity
    public void checkWeight(Integer currentWeightKG)
    {
        // currentWeightKG calculated thanks to weight sensors
        if (currentWeightKG > MaxWeightKG)
        {
            // Display 10 warnings
            for (var i = 0; i < 10; i++)
            {
                out.printf("\nALERT: Maximum weight capacity reached on Elevator %d of Column %d", this.id, this.column.getId());
            }

            // Freeze elevator until weight goes back to normal
            this.movement = "idle";
            this.door.openDoor();
        }
    }
    //endregion

    //region OVERRIDES
    // Sorts the elevator's requests queue by their floor
    @Override
    public int compare(Request o1, Request o2) {
        return o1.getFloor().compareTo(o2.getFloor());
    }
    //endregion
}
