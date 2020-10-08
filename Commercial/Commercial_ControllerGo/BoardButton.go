package main

import (
	"fmt"
	"math"
)

type BoardButton struct {

	// PROPERTIES
	Battery         *Battery
	RequestedFloor  int
	Floor           int
	Direction       string // up|down
	isToggled       bool
	isEmittingLight bool
}

// CONSTRUCTOR :-> Initialize a board button with the default values when creating a battery's board buttons list
func (btn *BoardButton) InitBoardButton(requestedFloor int, battery *Battery) {

	btn.RequestedFloor = requestedFloor
	btn.Battery = battery
	btn.Floor = OriginFloor
	btn.isToggled = false
	btn.isEmittingLight = false
}

// METHODS
// Send request to chosen elevator + return its value for further use
func (btn *BoardButton) Press() {

}

// Light up a pressed button
func (btn *BoardButton) ControlLight() {

	if btn.isToggled {
		btn.isEmittingLight = true

	} else {
		btn.isEmittingLight = false
	}
}

// Set what is the direction of the request when requesting an elevator to pick you up from RC
func (btn *BoardButton) SetDirection() {

	floorDifference := btn.RequestedFloor - btn.Floor

	if floorDifference > 0 {
		btn.Direction = "up"

	} else {
		btn.Direction = "down"
	}
}

// Choose which column to go to, based on the requested floor
func (btn *BoardButton) ChooseColumn() (Column, error) {

	for _, column := range btn.Battery.ColumnList {

		if btn.RequestedFloor >= column.LowestFloor && btn.RequestedFloor <= column.HighestFloor {

			fmt.Printf("Chosen column: Column %d", column.ID)
			return column, nil

		}
	}
	return Column{}, fmt.Errorf("None of columns go to that specified floor (floor %d). Fix the floor ranges.", btn.RequestedFloor)
}

// Choose which elevator should be called from the chosen column
func (btn *BoardButton) ChooseElevator() Elevator {

	elevatorScores := []float64{}
	chosenColumn := Column{}

	// Get the chosen column, if the requested floor is valid and within range
	if col, err := btn.ChooseColumn(); err != nil {
		chosenColumn := col

	} else {
		fmt.Println("ChooseColumn(), BoardButton.go FAILED: ", err)
	}

	for _, elevator := range chosenColumn.ElevatorList {

		// Initialize score to 0
		var score float64 = 0

		// Calculate floor difference differently based on whether or not elevator is already at RC or not
		// Remember: Each column has a floor range that its elevators must respect. The RC is not included in the range, so to make the calculations fair, if elevator is already at RC the floor difference will still look normal thanks to the 2nd calculation option as it will start from the column's lowest floor instead of at RC.
		floorDifference := 0

		if elevator.CurrentFloor != OriginFloor {
			floorDifference = elevator.CurrentFloor - chosenColumn.LowestFloor

		} else {
			floorDifference = 0
		}

		// Prevent use of any offline/under-maintenance elevators
		if elevator.Status != "online" {

			score = -1
			elevatorScores = append(elevatorScores, score)

		} else {

			// Bonify score based on floor difference
			if floorDifference == 0 {
				score += 5000

			} else {
				score += 5000 / (math.Abs(float64(floorDifference) + 1))
			}

			// Bonify score based on direction (highest priority)
			if elevator.Movement != "idle" {

				if floorDifference < 0 && btn.Direction == "down" && elevator.Movement == "down" {

					// Paths are not crossed, therefore try avoiding the use of this elevator
					score = 0
				} else if floorDifference > 0 && btn.Direction == "up" && elevator.Movement == "up" {

					// Paths are not crossed, therefore try avoiding the use of this elevator
					score = 0

				} else {

					// Paths are crossed, therefore favor this elevator
					score += 1000

					// Calculate next floor difference differently based on whether or not elevator's next floor will be at RC or not
					nextFloorDifference := 0
					if elevator.NextFloor != OriginFloor {
						nextFloorDifference = elevator.NextFloor - chosenColumn.LowestFloor
					} else {
						nextFloorDifference = 0
					}

					// Give redemption points, in worst case scenario where all elevators never cross paths
					if nextFloorDifference == 0 {
						score += 500
					} else {
						score += 500 / (math.Abs(float64(nextFloorDifference) + 1))
					}
				}
			}

			// Bonify score on request queue size (the smaller number of pre-existing requests, the faster therefore the better)
			if len(elevator.RequestsQueue) <= 3 {
				score += 1000
			} else if len(elevator.RequestsQueue) <= 7 {
				score += 250
			}

			// Send total score of elevator to the scores list
			elevatorScores = append(elevatorScores, score)
		}
	}

	// Get value of highest score
	var highestScore float64 = -1
	var highestScoreIndex int

	for i, score := range elevatorScores {

		if score > highestScore {
			highestScore = score
			highestScoreIndex = i
		}
	}

	// Get the elevator with the highest score (or NIL if all elevators were offline)
	chosenElevator := Elevator{}
	if highestScore > -1 {

		chosenElevator = chosenColumn.ElevatorList[highestScoreIndex]
		fmt.Printf("Chosen elevator of Column %d: Elevator %d", chosenColumn.ID, chosenElevator.ID)
	}
	return chosenElevator
}

// Send new request to chosen elevator
func (btn BoardButton) SendRequest(elevator Elevator) {

}
