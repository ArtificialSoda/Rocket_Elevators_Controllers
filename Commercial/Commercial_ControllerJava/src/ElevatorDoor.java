import static java.lang.System.out;

public class ElevatorDoor
{

    //region FIELDS
    private String status;
    //endregion

    //region PROPERTIES - Getters
    public String getStatus()
    {
        return status;
    }
    //endregion

    //region CONSTRUCTOR
    public ElevatorDoor(String status)
    {
        this.status = status;
    }
    //endregion

    //region METHODS
    public void openDoor()
    {
        status = "opened";
        out.println("\nDoor has opened");
        Program.sleep();
    }

    public void closeDoor()
    {
        status = "closed";
        out.println("\nDoor has closed");
        Program.sleep();
    }
    //endregion
}
