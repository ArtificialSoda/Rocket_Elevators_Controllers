import static java.lang.System.out;

public class FloorDisplay
{
    //region FIELDS
    private Elevator elevator;
    //endregion

    //region CONSTRUCTOR
    public FloorDisplay(Elevator elevator)
    {
        this.elevator = elevator;
    }
    //endregion

    //region METHODS
    public void displayFloor()
    {
        Program.sleep();

        if (elevator.getCurrentFloor() > 0)
        {
            out.printf("\n... Elevator %d of column %d's current floor mid-travel: %d ...", elevator.getId(), elevator.getColumn().getId(), elevator.getCurrentFloor());
        }
        else if (elevator.getCurrentFloor() < 0)
            out.printf("\n... Elevator %d of column %d's current floor mid-travel: B%d ...", elevator.getId(), elevator.getColumn().getId(), Math.abs(elevator.getCurrentFloor()));

    }
    //endregion
}
