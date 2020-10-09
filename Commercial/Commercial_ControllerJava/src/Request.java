public class Request
{

    //region FIELDS
    private Integer floor;
    private String direction;
    //endregion

    //region PROPERTIES - Getters
    public Integer getFloor() {
        return floor;
    }
    public String getDirection() {
        return direction;
    }
    //endregion

    //region PROPERTIES - Setters
    public void setFloor(Integer floor)
    {
        if (floor > Battery.NumFloors || floor < -(Battery.NumBasements))
            throw new RuntimeException("The floor value provided for the request is invalid.");
        else
            this.floor = floor;
    }
    public void setDirection(String direction)
    {
        if (direction.toLowerCase() != "up" && direction.toLowerCase() != "down")
            throw new RuntimeException("The direction provided for the request is invalid. It can only be 'up' or 'down'.");
        else
            this.direction = direction;
    }
    //endregion

    //region CONSTRUCTOR
    public Request(Integer floor, String direction)
    {
        setFloor(floor);
        setDirection(direction);
    }
    //endregion

}
