package main

import (
	"fmt"
	"os"
)

// STATIC PROPERTIES (in Golang, it's treated as a global)
var NumFloors int
var NumBasements int
var NumColumns int

type Battery struct {

	// PROPERTIES
	ID                  int
	Status              string // online|offline
	ColumnList          []Column
	BoardButtonList     []BoardButton
	isFire              bool
	isPowerOutage       bool
	isMechanicalFailure bool
}

// CONSTRUCTOR :-> Initialize a column with the default values when creating a battery's columns list
func (battery *Battery) InitBattery(id int) {

	battery.ID = id
	battery.Status = "online" // online|offline
	battery.ColumnList = []Column{}
	battery.BoardButtonList = []BoardButton{}
	battery.isFire = false
	battery.isPowerOutage = false
	battery.isMechanicalFailure = false
}

// METHODS
// Run all initialiazing/startup Battery methods
func (battery *Battery) Run() {

	battery.CreateColumnList()
	battery.CreateBoardButtons()
}

// Initialize the battery's collection of columns
func (battery *Battery) CreateColumnList() {

	for columnID := 1; columnID <= NumColumns; columnID++ {

		column := Column{}
		column.InitColumn(columnID)

		// Set up allowed floor ranges
		if NumBasements > 0 {

			if columnID == 1 {

				// Column takes care of basement floors
				column.LowestFloor = -(NumBasements)
				column.HighestFloor = -1

			} else {

				// Column takes care of above-ground floors
				column.LowestFloor = 1 + NumFloors/(NumColumns-1)*(columnID-2)
				column.HighestFloor = NumFloors / (NumColumns - 1) * (columnID - 1)

			}

		} else {

			// No basement floors - therefore all floors are above-ground
			column.LowestFloor = 1 + NumFloors/NumColumns*(columnID-1)
			column.HighestFloor = NumFloors / NumColumns * columnID
		}

		column.CreateElevatorList()
		column.CreateCallButtons()
		battery.ColumnList = append(battery.ColumnList, column)
	}
}

// Initialize the battery's board display's buttons
func (battery *Battery) CreateBoardButtons() {

	// Board buttons for basements floors
	for numBasement := -(NumBasements); numBasement < 0; numBasement++ {

		button := BoardButton{}
		button.InitBoardButton(numBasement, battery)
		battery.BoardButtonList = append(battery.BoardButtonList, button)
	}

	// Board buttons for non-basement floors
	for numFloor := OriginFloor; numFloor <= NumFloors; numFloor++ {

		button := BoardButton{}
		button.InitBoardButton(numFloor, battery)
		battery.BoardButtonList = append(battery.BoardButtonList, button)
	}
}

// Monitor the battery's elevator system
// In real conditions, the monitoring would be done in a near-infinite while loop
func (battery *Battery) MonitorSystem() {

	shouldExit := false

	if battery.isFire || battery.isPowerOutage || battery.isMechanicalFailure {

		battery.Status = "offline"

		for i := range battery.ColumnList {

			battery.ColumnList[i].Status = "offline"
			for j := range battery.ColumnList[i].ElevatorList {

				battery.ColumnList[i].ElevatorList[j].Status = "offline"
			}
		}
		fmt.Printf("Battery %d has been shut down for maintenance. Sorry for the inconvenience.", battery.ID)
		shouldExit = true
	}

	// Stop execution of script if something is wrong
	if shouldExit {
		os.Exit(-1)
	}
}

// Assigns a column and elevator to use when making a request from RC via the board buttons
func (battery *Battery) AssignElevator(requestedFloor int) {

	// Select the call button to press
	var boardBtnToPress BoardButton
	for i := range battery.BoardButtonList {

		if battery.BoardButtonList[i].RequestedFloor == requestedFloor {
			boardBtnToPress = battery.BoardButtonList[i]
			break
		}
	}

	// Select the chosen elevator
	var chosenElevator Elevator
	if elevator, err := boardBtnToPress.Press(); err == nil {
		chosenElevator = elevator

	} else {
		fmt.Println("AssignElevator(), Battery.go FAILED: ", err)
	}

	// Do requests until elevator has reached the floor where the call was made (RC floor)
	for chosenElevator.CurrentFloor != OriginFloor {
		chosenElevator.DoRequests()
	}

	// Set a request for the elevator to go to requested floor, once picked up
	newDirection := ""
	if chosenElevator.CurrentFloor < requestedFloor {
		newDirection = "up"

	} else {
		newDirection = "down"
	}

	chosenElevator.SendRequest(requestedFloor, newDirection)

	// Do requests until elevator has reached requested floor
	for chosenElevator.CurrentFloor != requestedFloor {
		chosenElevator.DoRequests()
	}
}
