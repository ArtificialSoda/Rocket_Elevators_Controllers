package main

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
