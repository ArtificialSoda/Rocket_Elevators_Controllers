/*****************************************************
    Author: Fabien H. Dimitrov
    Context: Codeboxx Week 2 (Odyssey)
*****************************************************/

/**************************************************************************************************************************************************** 
~OBJECT DEFINITIONS~
****************************************************************************************************************************************************/
'use strict'

class Battery 
{

    /* FIELDS */
    id;
    status = "online"; // offline|online
    isFire = false;
    isPowerOutage = false;
    isMechanicalFailure = false;
    columnList = [];

    /* CONSTRUCTOR */
    constructor(id)
    {
        this.id = id;
    }

    /* METHODS */

    // Initialize the battery's collection of columns
    createColumnList()
    {
        for (let columnID = 1; columnID <= Battery.numColumns; columnID++)
        {
            let column = new Column(columnID);
            column.createElevatorList();
            column.createCallButtons();
            
            this.columnList.push(column);
        }
    }

    // Monitor the battery's elevator system
    // In real conditions, the monitoring would be done in a near-infinite while loop
    monitorSystem()
    {
        if (this.isFire || this.isPowerOutage || this.isMechanicalFailure)
        {
            this.status = "offline";
            console.log(`Battery ${this.id} has been shut down for maintenance. Sorry for the inconvenience.`);
        }
    }

}

class Column
{

    /* FIELDS */
    id;
    numFloors = Elevator.numFloors;
    status = "online"; // offline|online
    elevatorList = [];
    upCallButtons = [];
    downCallButtons = [];

    /* CONSTRUCTOR */
    constructor(id)
    {
        this.id = id;
    }

    /* METHODS */

    // Initialize the column's collection of elevators
    createElevatorList()
    {
        for (let elevatorID = 1; elevatorID <= Column.numElevators; elevatorID++)
        {
            let elevator = new Elevator(elevatorID);
            elevator.createFloorButtons();
            this.elevatorList.push(elevator);
        }
    }

    // Initialize all the call buttons, on each floor
    createCallButtons()
    {
        for (let numFloor = 1; numFloor <= this.numFloors; numFloor++)
        {
            let upCallBtn = new CallButton(numFloor, "up", this);
            this.upCallButtons.push(upCallBtn);

            let downCallBtn = new CallButton(numFloor, "down", this);
            this.downCallButtons.push(downCallBtn);
        }
    }

    // Complete request that was sent to chosen elevator
    // Return chosen elevator, for further use
    requestElevator(requestedFloor, direction)
    {
        let callButtonToPress;
        
        if (direction == "up")
            callButtonToPress = this.upCallButtons.find(button => button.floor == requestedFloor);
        else if (direction == "down")
            callButtonToPress = this.downCallButtons.find(button => button.floor == requestedFloor);

        let chosenElevator = callButtonToPress.press();
        chosenElevator.doRequests();
        
        return chosenElevator;
    }

    // Move chosen elevator to requested floor
    requestFloor(elevator, requestedFloor)
    {
        let floorButtonToPress = elevator.floorButtons.find(button => button.floor == requestedFloor);
        floorButtonToPress.press();
        elevator.doRequests();
    }
}

class Elevator
{
    /* FIELDS */
    id;
    status = "online"; // offline|online
    movement = "idle"; // idle|up|down
    originFloor = 1;
    currentFloor = this.originFloor;
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
        for (let numFloor = 1; numFloor <= Elevator.numFloors; numFloor++)
        {
            let floorButton = new FloorButton(numFloor, this);
            this.floorButtons.push(floorButton);
        }
    }

    // Make elevator go to its scheduled next floor
    goToNextFloor()
    {
        console.log(`Elevator ${this.id}, currently at floor ${this.currentFloor}, is about to go to floor ${this.nextFloor}...`);
        console.log("=================================================================");
        
        while (this.currentFloor != this.nextFloor)
        {
            if (this.movement == "up")
                this.currentFloor++;
            else if (this.movement == "down")
                this.currentFloor--;

            console.log(`... Elevator ${this.id}'s current floor mid-travel: ${this.currentFloor} ...`);
        }
        console.log("=================================================================");
        console.log(`Elevator ${this.id} has reached its requested floor! It is now at floor ${this.currentFloor}.`);
    }

    // Make elevator go to origin floor
    goToOrigin()
    {
        this.nextFloor = this.originFloor;
        console.log(`Elevator ${this.id} going back to origin...`);
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

    // Complete the elevator requests
    doRequests()
    {
        if (this.requestsQueue.length > 0)
        {
            // Make sure queue is sorted before any request is completed
            this.sortRequestsQueue();
            let requestToComplete = this.requestsQueue[0];

            // Go to requested floor
            this.door.closeDoor();
            this.nextFloor = requestToComplete.floor;
            this.goToNextFloor();

            // Remove request after it is complete
            this.door.openDoor();
            this.requestsQueue.splice(0, 1);

            // Automatically close door
            this.door.closeDoor();
        }
        else 
        {
            // Automatically go to origin floor after certain period of inactivity (5 mins)
            this.movement = "idle";
            let wait = setTimeout(() => this.goToOrigin(), 10000); // Replace the milliseconds with 300000 to simulate 5 minutes of time
            clearTimeout(wait);
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

class FloorButton
{
    /* FIELDS */
    floor;
    elevator;
    direction = null;
    isToggled = false;
    isEmittingLight = false;
    
    /* CONSTRUCTOR */
    constructor(floor, elevator)
    {
        this.floor = floor;
        this.elevator = elevator;
    }

    /* METHODS */
    
    press()
    {
        console.log(`\nYou're currently on floor ${this.elevator.currentFloor}, inside Elevator ${this.elevator.id}. You decide to go to floor ${this.floor}.`);

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
        let floorDifference = this.elevator.currentFloor - this.floor;
        if (floorDifference > 0)
            this.direction = "down"
        else 
            this.direction = "up";
    }

    // Send new request to its elevator 
    sendRequest()
    {
        let request = new Request(this.floor, this.direction);
        this.elevator.requestsQueue.push(request);
    }
}

class CallButton
{
    /* FIELDS */
    floor;
    direction;
    column;
    isToggled = false;
    isEmittingLight = false;

    /* CONSTRUCTOR */
    constructor(floor, direction, column)
    {
        this.floor = floor;
        this.direction = direction;
        this.column = column;
    }

    /* METHODS */

    // Return elevator that was chosen, for further use
    press()
    {
        console.log(`\nYou are on floor ${this.floor}. You decide to call an elevator.`);

        this.isToggled = true;
        this.controlLight();

        let chosenElevator = this.chooseElevator();
        this.sendRequest(chosenElevator);

        this.isToggled = false;
        this.controlLight();

        return chosenElevator;
    }

    // Light up a pressed button
    controlLight()
    {
        if (this.isToggled)
            this.isEmittingLight = true;
        else 
            this.isEmittingLight = false;
    }

    // Choose which elevator to call
    chooseElevator()
    {
        let elevatorScores = [];
        
        for (let elevator of this.column.elevatorList)
        {
            // Initialize score to 0
            let score = 0;
            let floorDifference = elevator.currentFloor - this.floor;

            // Prevents use of any offline/under-maintenance elevators
            if (elevator.status != "online")
            {
                score = -1;
                elevatorScores.push(score);
            }
            else
            {
                // Bonify score based on difference in floors
                let absFloorDiff = Math.abs(floorDifference);
                if (absFloorDiff == 0)
                    score += 5000;
                else 
                    score += 5000/(absFloorDiff + 1);
                
                // Bony score based on direction (highest priority)
                if (elevator.movement != "idle") 
                {
                    if (floorDifference >= 0 && this.direction == "down" && elevator.movement == "down")
                    {
                        // Paths are crossed going down, therefore favor this elevator
                        score += 10000;
                    }
                    else if (floorDifference <= 0 && this.direction == "up" && elevator.movement == "up")
                    {
                        // Paths are crossed going up, therefore favor this elevator
                        score += 10000;
                    }
                    else 
                    {
                        // Paths are not crossed, therefore try avoiding the use of this elevator
                        score = 0;
            
                        // Give redemption points, in worst case scenario where all elevators never cross paths
                        let nextFloorDifference = elevator.nextFloor - this.floor;
                        let absNextFloorDiff = Math.abs(nextFloorDifference);

                        if (absNextFloorDiff == 0)
                            score += 500;
                        else 
                            score += 500/(absNextFloorDiff + 1);
                    }
                }

                // Bonify score on request queue size (the smaller number of pre-existing requests, the faster therefore the better)
                if (elevator.requestsQueue.length <= 3)
                    score += 1000;
                else if (elevator.requestsQueue.length <= 7)
                    score += 250;
                
                // Send total score of elevator to the scores list
                elevatorScores.push(score);
            }
        }
        
        // Get value of highest score
        let highestScore = -1;

        for (let score of elevatorScores)
        {
            if (score > highestScore)
                highestScore = score;
        }

        // Get the elevator with the highest score (or NULL if all elevators were offline)
        let chosenElevator = null;
        if (highestScore > -1)
            chosenElevator = this.column.elevatorList[elevatorScores.indexOf(highestScore)];
        
        console.log(`Chosen elevator's ID: ${chosenElevator.id}`);
        return chosenElevator;


    }

    // Send new request to chosen elevator 
    sendRequest(elevator)
    {
        let request = new Request(this.floor, this.direction)
        elevator.requestsQueue.push(request);
    }

    
}

class Request
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

/**************************************************************************************************************************************************** 
~TEST SCENARIOS~
****************************************************************************************************************************************************/
// Variables
Battery.numColumns = 1;
Column.numElevators = 2; 
Elevator.numFloors = 10;

// Instantiate the batteries, the columns, and the elevators
var battery = new Battery(1);
battery.createColumnList();
battery.monitorSystem();

// Set placeholder for column used in test scenario
var column = battery.columnList[0];

/*** SCENARIO 1 ***/
function Scenario1()
{
    console.log("**********************************************************************************************************************************");
    console.log("SCENARIO 1");
    console.log("**********************************************************************************************************************************");

    column.elevatorList[0].changeProperties(2, null, "idle");
    column.elevatorList[1].changeProperties(6, null, "idle");

    var chosenElevator = column.requestElevator(3, "up");
    column.requestFloor(chosenElevator, 7);
}

/*** SCENARIO 2 ***/
function Scenario2()
{
    console.log("**********************************************************************************************************************************");
    console.log("SCENARIO 2");
    console.log("**********************************************************************************************************************************");

    column.elevatorList[0].changeProperties(10, null, "idle");
    column.elevatorList[1].changeProperties(3, null, "idle");

    var chosenElevator = column.requestElevator(1, "up");
    column.requestFloor(chosenElevator, 6);

    console.log("=== 2 minutes later ===");

    chosenElevator = column.requestElevator(3, "up");
    column.requestFloor(chosenElevator, 5);

    console.log("=== After a bit more time ===");

    chosenElevator = column.requestElevator(9, "down");
    column.requestFloor(chosenElevator, 2);
}

Scenario1();
//Scenario2();









