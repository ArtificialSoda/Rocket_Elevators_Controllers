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
            for (let column of this.columnList)
            {
                column.status = "offline"
                for (let elevator of column.elevatorList)
                {
                    elevator.status = "offline";
                }
            }
            throw new Error(`Battery ${this.id} has been shut down for maintenance. Sorry for the inconvenience.`);
        }
    }

}

class Column
{

    /* FIELDS */
    id;
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
        for (let numFloor = 1; numFloor <= Elevator.numFloors; numFloor++)
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
    maxWeightKG = 1000;
    requestsQueue = [];
    floorButtons = [];
    door = new ElevatorDoor("closed");
    floorDisplay = new FloorDisplay(this);

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
            {
                this.currentFloor++;
            }
            else if (this.movement == "down")
            {
                this.currentFloor--;
            }

            this.floorDisplay.displayFloor();
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

    // Get what should be the movement direction of the elevator for its upcoming request
    getMovement()
    {
        let floorDifference = this.currentFloor - this.requestsQueue[0].floor;
        if (floorDifference > 0)
            this.movement = "down";
        else 
            this.movement = "up";
    }

    // Sort requests, for added efficiency
    sortRequestsQueue()
    {
        let request = this.requestsQueue[0];
        this.getMovement();

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

        // Automatically go idle temporarily if 0 requests or at the end of request
        this.movement = "idle";
    }
    
    // Check if elevator is at full capacity
    checkWeight(currentWeightKG)
    {
        // currentWeightKG calculated thanks to weight sensors
        if (currentWeightKG > this.maxWeightKG)
        {
            // Display 10 warnings
            for (let i = 0; i < 10; i++)
            {
                console.log(`\nALERT: Maximum weight capacity reached on Elevator ${this.id}`);
            }

            // Freeze elevator until weight goes back to normal
            if (this.movement != "idle")
                this.movement = "idle";
            this.door.openDoor()
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

    openDoor()
    {
        this.status = "opened";
    }

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
        console.log(`\nFLOOR REQUEST`);
        console.log(`Someone is currently on floor ${this.elevator.currentFloor}, inside Elevator ${this.elevator.id}. The person decides to go to floor ${this.floor}.`);

        this.isToggled = true;
        this.controlLight();
        
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

    // Send new request to its elevator 
    sendRequest()
    {
        let request = new Request(this.floor, this.direction);
        this.elevator.requestsQueue.push(request);
    }
}

class FloorDisplay
{
    /* FIELDS */
    elevator;

    /* CONSTRUCTOR */
    constructor(elevator)
    {
        this.elevator = elevator;
    }

    /* METHODS */

    // Displays current floor of elevator as it travels
    displayFloor()
    {
        console.log(`... Elevator ${this.elevator.id}'s current floor mid-travel: ${this.elevator.currentFloor} ...`);
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
        console.log(`\nELEVATOR REQUEST`);
        console.log(`Someone is on floor ${this.floor}. The person decides to call an elevator.`);

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
            let floorDifference = Math.abs(elevator.currentFloor - this.floor);

            // Prevents use of any offline/under-maintenance elevators
            if (elevator.status != "online")
            {
                score = -1;
                elevatorScores.push(score);
            }
            else
            {
                // Bonify score based on difference in floors
                if (floorDifference == 0)
                    score += 5000;
                else 
                    score += 5000/(floorDifference + 1);
                
                // Bonify score based on direction (highest priority)
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
                        let nextFloorDifference = Math.abs(elevator.nextFloor - this.floor);
                        if (nextFloorDifference == 0)
                            score += 500;
                        else 
                            score += 500/(nextFloorDifference + 1);
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
function scenario1()
{
    console.log("**********************************************************************************************************************************");
    console.log("SCENARIO 1");
    console.log("**********************************************************************************************************************************");

    console.log(column)
    column.elevatorList[0].changeProperties(2, null, "idle");
    column.elevatorList[1].changeProperties(6, null, "idle");

    let chosenElevator = column.requestElevator(3, "up");
    column.requestFloor(chosenElevator, 7);
}

/*** SCENARIO 2 ***/
function scenario2()
{
    console.log("\n\n");
    console.log("**********************************************************************************************************************************");
    console.log("SCENARIO 2");
    console.log("**********************************************************************************************************************************");

    column.elevatorList[0].changeProperties(10, null, "idle");
    column.elevatorList[1].changeProperties(3, null, "idle");

    let chosenElevator = column.requestElevator(1, "up");
    column.requestFloor(chosenElevator, 6);

    console.log("\n\n\n=== 2 MINUTES LATER ===\n\n");

    chosenElevator = column.requestElevator(3, "up");
    column.requestFloor(chosenElevator, 5);

    console.log("\n\n\n=== AFTER A BIT MORE TIME ===\n\n");

    chosenElevator = column.requestElevator(9, "down");
    column.requestFloor(chosenElevator, 2);
    
}

/*** SCENARIO 3 ***/
function scenario3()
{
    console.log("\n\n");
    console.log("**********************************************************************************************************************************");
    console.log("SCENARIO 3");
    console.log("**********************************************************************************************************************************");

    column.elevatorList[0].changeProperties(10, null, "idle");
    column.elevatorList[1].changeProperties(3, 6, "up");

    let chosenElevator = column.requestElevator(3, "down");

    column.requestFloor(chosenElevator, 2);
    column.requestFloor(column.elevatorList[1], 6);

    console.log("\n\n\n=== 5 MINUTES LATER ===\n\n");

    chosenElevator = column.requestElevator(10, "down");
    column.requestFloor(chosenElevator, 3);
}

scenario1();
scenario2();
scenario3();










