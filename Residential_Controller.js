'use strict'
/**************************************************************************************************************************************************** 
~OBJECT DEFINITIONS~
****************************************************************************************************************************************************/
class Battery 
{

    /* FIELDS */
    id;
    numColumns;
    status = "online"; // offline|online
    columnList = [];

    /* CONSTRUCTOR */
    constructor(id, numColumns) 
    {
        this.id = id;
        this.numColumns = numColumns;
    }

    /* METHODS */

    // Initialize the battery's collection of columns
    createColumnList()
    {
        for (let columnID = 1; columnID <= this.numColumns; columnID++)
        {
            let column = new Column(columnID);
            this.columnList.push(column);
        }
    }
}

class Column
{

    /* FIELDS */
    id;
    numElevators;
    status = "online"; // offline|online
    elevatorList = [];

    /* CONSTRUCTOR */
    constructor(id, numElevators) 
    {
        this.id = id;
        this.numElevators = numElevators;

    }

    /* METHODS */

    // Initialize the column's collection of elevators
    createElevatorList()
    {
        for (let elevatorID = 1; elevatorID <= this.numElevators; elevatorID++)
        {
            let elevator = new Elevator(elevatorID);
            this.elevatorList.push(elevator);
        }
    }

    // Move chosen elevator to requested floor
    requestFloor(elevator, requestedFloor)
    {
        
    }
}

class Elevator
{
    /* FIELDS */
    id;
    status = "online"; // offline|online
    movement = "idle"; // idle|up|down
    numFloors = 10;
    originFloor = 1;
    currentFloor = originFloor;
    nextFloor = null;
    requestsQueue = [];
    floorButtons = [];
    door = new ElevatorDoor("closed");

    /* CONSTRUCTOR */
    constructor(id)
    {
        this.id = id;
    }

    /* METHODS */

    // Change properties of elevator in one line - USE ONLY FOR TESTING
    changeProperties(newCurrentFloor, newNextFloor, newMovement)
    {
        this.currentFloor = newCurrentFloor;
        this.nextFloor = newNextFloor;
        this.movement = newMovement;
    }

    // Create all floor buttons that should be inside the elevator
    createFloorButtons()
    {
        for (let numFloor = 1; numFloor <= numFloors; numFloor++)
        {
            let floorButton = new FloorButton(numFloor);
            this.floorButtons.push(floorButton);
        }
    }

    // Make elevator go to its scheduled next floor
    goToNextFloor()
    {
        let timer;
        while (this.currentFloor != this.nextFloor)
        {
            if (this.movement == "up")
               timer = setInterval(() => this.currentFloor++, 2000);
            else if (this.movement == "down")
               timer = setInterval(() => this.currentFloor--, 2000);
        }
        clearInterval(timer);
    }

    // Make elevator go to origin floor
    goToOrigin()
    {
        this.nextFloor = this.originFloor;
        this.goToNextFloor();
    }

    // Sort requests, for added efficiency
    sortRequestsQueue()
    {
        let request = this.requestsQueue[0];

        this.movement = request.direction;
        if (this.requestsQueue.length > 1)
        {
            // Sort the queue in ascending order
            this.requestsQueue.sort();

            if (this.movement == "up")
            {
                // Push any request to the end of the queue that would require a direction change
                for (request of this.requestsQueue)
                {
                    if (request.floor < this.currentFloor || request.direction != this.movement)
                        this.requestsQueue.push(this.requestsQueue.splice(this.requestsQueue.indexOf(request), 1)[0]);
                }
            }
            else if (this.movement == "down")
            {
                // Reverse the sorted queue (will now be in descending order)
                this.requestsQueue.reverse();

                // Push any request to the end of the queue that would require a direction change
                for (request of this.requestsQueue)
                {
                    if (request.floor > this.currentFloor || request.direction != this.movement)
                        this.requestsQueue.push(this.requestsQueue.splice(this.requestsQueue.indexOf(request), 1)[0]);
                }
            }
        }
    }
}

class ElevatorDoor 
{
    /* FIELDS */
    status;

    /* CONSTRUCTOR */
    constructor(status)
    {
        this.status = status;
    }

    /* METHODS */

    // Open the door of the elevator
    openDoor()
    {
        this.status = "opened";
    }

    // Close the door of the elevator
    closeDoor()
    {
        this.status = "closed";
    }
}

class FloorButton extends Elevator
{
    /* FIELDS */
    floor;
    direction = null;
    isToggled = false;
    isEmittingLight = false;
    
    /* CONSTRUCTOR */
    constructor(floor)
    {
        this.floor = floor;
    }

    /* METHODS */
    press()
    {
        this.isToggled = true;
        this.controlLight();
        this.getDirection();
        this.sendRequest();

        this.isToggled = false;
        this.controlLight();
    }

    // Light up a pressed button
    controlLight()
    {
        if (this.isToggled)
            this.isEmittingLight = true;
        else 
            this.isEmittingLight = false;
    }

    // Get what is the direction of a new request
    getDirection()
    {
        let floorDifference = super.currentFloor - this.floor;
        if (floorDifference > 0)
            this.direction = "down"
        else 
            this.direction = "up";
    }

    // Send new request to its elevator 
    sendRequest()
    {
        let request = new Request(this.floor, this.direction);
        super.requestsQueue.push(request);
    }
}

class Request // A request which is to be sent to an elevator
{
    /* FIELDS */
    floor;
    direction;

    /* CONSTRUCTOR */
    constructor(floor, direction)
    {
        this.floor = floor;
        this.direction = direction;
    }
}




