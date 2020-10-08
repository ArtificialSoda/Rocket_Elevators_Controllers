package main

import "fmt"

// STATIC PROPERTIES (in Golang, it's treated as a global)
var NumElevators int = 1

type Column struct {

	// PROPERTIES
	ID             int
	Status         string // online|offline
	LowestFloor    int
	HighestFloor   int
	ElevatorList   []Elevator
	CallButtonList []CallButton
}

// METHODS
// Initialize the column's collection of elevators
func (column *Column) CreateElevatorList() {

	for elevatorID := 1; elevatorID <= NumElevators; elevatorID++ {

		elevator := Elevator{}
		elevator.InitElevator(elevatorID, column)
		column.ElevatorList = append(column.ElevatorList, elevator)
	}
}

// Initialize all the call buttons, on each floor
func (column *Column) CreateCallButtons() {

	for numFloor := column.LowestFloor; numFloor <= column.HighestFloor; numFloor++ {

		button := CallButton{}
		button.InitCallButton(numFloor, column)
		column.CallButtonList = append(column.CallButtonList, button)
	}
}

// Request an elevator to current location (if current location is not RC)
func (column *Column) RequestElevator(floorNumber int) {

	// Select the call button to press
	var callBtnToPress CallButton
	for i := range column.CallButtonList {

		if column.CallButtonList[i].Floor == floorNumber {
			callBtnToPress = column.CallButtonList[i]
			break
		}
	}

	// Select the chosen elevator
	var chosenElevator Elevator
	if elevator, err := callBtnToPress.Press(); err != nil {
		chosenElevator = elevator

	} else {
		fmt.Println("RequestElevator(), Column.go FAILED: ", err)
	}

	// Do requests until elevator has reached the final destination (RC)
	for chosenElevator.CurrentFloor != OriginFloor {
		chosenElevator.DoRequests()
	}
}
