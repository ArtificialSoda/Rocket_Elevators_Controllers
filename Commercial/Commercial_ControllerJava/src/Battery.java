import java.util.ArrayList;
import java.util.List;
import static java.lang.System.out;

public class Battery
{

    //region STATIC PROPERTIES
    public static Integer NumFloors;
    public static Integer NumBasements;
    public static Integer NumColumns;
    //endregion

    //region FIELDS
    private Integer id;
    private String status;
    private List<Column> columnList;
    private List<BoardButton> boardButtonList;
    private Boolean isFire;
    private Boolean isPowerOutage;
    private Boolean isMechanicalFailure;
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

    public List<Column> getColumnList() {
        return columnList;
    }

    public List<BoardButton> getBoardButtonList() {
        return boardButtonList;
    }
    //endregion

    //region PROPERTIES - Setters
    public void setStatus(String status)
    {
        if (status.toLowerCase() != "online" && status.toLowerCase() != "offline")
            throw new RuntimeException("Invalid value for battery's status. Can only be either 'online' or 'offline'.");
        else
            this.status = status;
    }

    public void setColumnList(List<Column> columnList) {
        this.columnList = columnList;
    }

    public void setBoardButtonList(List<BoardButton> boardButtonList) {
        this.boardButtonList = boardButtonList;
    }
    //endregion

    //region CONSTRUCTOR
    public Battery(Integer id)
    {
        this.id = id;
        setStatus("online"); // online|offline
        setColumnList(new ArrayList<Column>());
        setBoardButtonList(new ArrayList<BoardButton>());
        this.isFire = false;
        this.isPowerOutage = false;
        this.isMechanicalFailure = false;
    }
    //endregion

    //region STATIC METHODS
    // Quickly change the Battery's static properties
    public static void ChangeProperties(int numColumns, int numFloors, int numBasements)
    {
        NumColumns = numColumns;
        NumFloors = numFloors;
        NumBasements = numBasements;
    }
    //endregion

    //region METHODS
    // Run all initializing/startup Battery methods
    public void run()
    {
        this.createColumnList();
        this.createBoardButtons();
    }

    // Initialize the battery's collection of columns
    public void createColumnList()
    {
        for (int columnID = 1; columnID <= NumColumns; columnID++)
        {
            var column = new Column(columnID);

            // Set up allowed floor ranges
            if (NumBasements > 0)
            {
                if (columnID == 1)
                {
                    // Column takes care of basement floors
                    column.setLowestFloor(-(NumBasements));
                    column.setHighestFloor(-1);
                }
                else
                {
                    // Column takes care of above-ground floors
                    column.setLowestFloor(1 + NumFloors / (NumColumns - 1) * (columnID - 2));
                    column.setHighestFloor(NumFloors / (NumColumns - 1) * (columnID - 1));
                }
            }
            else
            {
                // No basement floors - therefore all floors are above-ground
                column.setLowestFloor(1 + NumFloors / NumColumns * (columnID - 1));
                column.setHighestFloor(NumFloors / NumColumns * columnID);
            }

            column.createElevatorList();
            column.createCallButtons();
            this.columnList.add(column);
        }
    }

    // Initialize the battery's board display's buttons
    public void createBoardButtons()
    {
        // Board buttons for basements floors
        for (int numBasement = -(NumBasements); numBasement < 0; numBasement++)
        {
            var button = new BoardButton(numBasement, this);
            boardButtonList.add(button);
        }

        // Board buttons for non-basement floors
        for (int numFloor = Elevator.OriginFloor; numFloor <= NumFloors; numFloor++)
        {
            var button = new BoardButton(numFloor, this);
            boardButtonList.add(button);
        }

    }

    // Monitor the battery's elevator system
    // In real conditions, the monitoring would be done in a near-infinite while loop
    public void monitorSystem()
    {
        if (isFire || isPowerOutage || isMechanicalFailure)
        {
            this.status = "offline";
            for (var column : this.columnList)
            {
                column.setStatus("offline");
                for (var elevator : column.getElevatorList())
                {
                    elevator.setStatus("offline");
                }
            }

            // Stop execution of script
            out.printf("Battery %d has been shut down for maintenance. Sorry for the inconvenience.", this.id);
            System.exit(-1);
        }
    }

    // Assigns a column and elevator to use when making a request from RC via the board buttons
    public void AssignElevator(int requestedFloor)
    {
        BoardButton boardBtnToPress = null;
        for (var btn : this.boardButtonList)
        {
            if (btn.getRequestedFloor().equals(requestedFloor))
            {
                boardBtnToPress = btn;
                break;
            }
        }
        Elevator chosenElevator = boardBtnToPress.press();

        // Do requests until elevator has reached the floor where the call was made (RC floor)
        while (chosenElevator.getCurrentFloor() != Elevator.OriginFloor)
            chosenElevator.doRequests();

        // Set a request for the elevator to go to requested floor, once picked up
        String newDirection = (chosenElevator.getCurrentFloor() < requestedFloor) ? "up" : "down";
        chosenElevator.sendRequest(requestedFloor, newDirection);

        // Do requests until elevator has reached requested floor
        while (chosenElevator.getCurrentFloor() != requestedFloor)
            chosenElevator.doRequests();
    }
    //endregion

}
