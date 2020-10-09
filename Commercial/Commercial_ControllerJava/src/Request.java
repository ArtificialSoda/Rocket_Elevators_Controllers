public class Request {

    //region FIELDS
    private int floor;
    private String direction;
    //endregion

    //region PROPERTIES - Getters
    public int getFloor() {
        return floor;
    }
    public String getDirection() {
        return direction;
    }
    //endregion

    //region PROPERTIES - Setters
    public void setFloor(int value)
    {
        if (value > Battery.NumFloors || value < -(Battery.NumBasements))
            throw new RuntimeException("The floor value provided for the request is invalid.");
        else
            floor = value;
    }
    public void setDirection(String value)
    {
        if (value.toLowerCase() != "up" && value.toLowerCase() != "down")
            throw new RuntimeException("The direction provided for the request is invalid. It can only be 'up' or 'down'.");
        else
            direction = value;
    }
    //endregion

    //region CONSTRUCTOR
    public Request(int floor, String direction)
    {
        setFloor(floor);
        setDirection(direction);
    }
    //endregion

}
