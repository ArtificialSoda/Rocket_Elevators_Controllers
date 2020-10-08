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

// METHODS
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
	if elevator, err := boardBtnToPress.Press(); err != nil {
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

	r := Request{requestedFloor, newDirection}
	chosenElevator.RequestsQueue = append(chosenElevator.RequestsQueue, r)

	// Do requests until elevator has reached requested floor
	for chosenElevator.CurrentFloor != requestedFloor {
		chosenElevator.DoRequests()
	}
}
