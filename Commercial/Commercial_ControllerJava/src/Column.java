import java.util.ArrayList;
import java.util.List;

public class Column
{

    //region STATIC PROPERTIES
    public static int NumElevators;
    //endregion

    //region FIELDS
    private Integer id;
    private String status;
    private Integer lowestFloor;
    private Integer highestFloor;
    private List<Elevator> elevatorList;
    private List<CallButton> callButtonsList;
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

    public Integer getLowestFloor()
    {
        return lowestFloor;
    }

    public Integer getHighestFloor()
    {
        return highestFloor;
    }

    public List<Elevator> getElevatorList()
    {
        return elevatorList;
    }

    public List<CallButton> getCallButtonsList()
    {
        return callButtonsList;
    }
    //endregion

    //region PROPERTIES - Setters
    public void setStatus(String status)
    {
        if (status.toLowerCase() != "online" && status.toLowerCase() != "offline")
            throw new RuntimeException("Invalid value for column's status. Can only be either 'online' or 'offline'.");
        else
            this.status = status;
    }

    public void setLowestFloor(Integer lowestFloor)
    {
        if (lowestFloor >= Battery.NumFloors || lowestFloor < -(Battery.NumBasements))
            throw new RuntimeException("The lowest floor value provided for the column is invalid.");
        else
            this.lowestFloor = lowestFloor;
    }

    public void setHighestFloor(Integer highestFloor)
    {
        if (highestFloor > Battery.NumFloors || highestFloor <= -(Battery.NumBasements))
            throw new RuntimeException("The highest floor value provided for the column is invalid.");
        else
            this.highestFloor = highestFloor;
    }

    public void setElevatorList(List<Elevator> elevatorList)
    {
        this.elevatorList = elevatorList;
    }

    public void setCallButtonsList(List<CallButton> callButtonsList)
    {
        this.callButtonsList = callButtonsList;
    }
    //endregion

    //region CONSTRUCTOR
    public Column(int id)
    {
        this.id = id;
        setStatus("online"); // online|offline
        setElevatorList(new ArrayList<Elevator>());
        setCallButtonsList(new ArrayList<CallButton>());

    }
    //endregion

    //region METHODS
    // Initialize the column's collection of elevators
    public void createElevatorList()
    {
        for (var elevatorID = 1; elevatorID <= NumElevators; elevatorID++)
        {
            var elevator = new Elevator(elevatorID, this);
            this.elevatorList.add(elevator);
        }
    }

    // Initialize all the call buttons, on each floor
    public void createCallButtons()
    {
        for (int numFloor = this.lowestFloor; numFloor <= this.highestFloor; numFloor++)
        {
            if (numFloor != 0)
            {
                var callBtn = new CallButton(numFloor, this);
                this.callButtonsList.add(callBtn);
            }
        }
    }

    // Request an elevator to current location (if current location is not RC)
    public void requestElevator(Integer floorNumber)
    {
        CallButton callBtnToPress = null;
        for (var btn : this.callButtonsList)
        {
            if (btn.getFloor().equals(floorNumber))
            {
                callBtnToPress = btn;
                break;
            }
        }
        Elevator chosenElevator = callBtnToPress.press();

        // Do requests until elevator has reached the final destination (RC)
        while (chosenElevator.getCurrentFloor() != Elevator.OriginFloor)
            chosenElevator.doRequests();
    }


    //endregion
}

