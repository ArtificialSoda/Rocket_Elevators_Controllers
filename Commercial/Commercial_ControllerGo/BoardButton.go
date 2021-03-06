package main

import (
	"errors"
	"fmt"
	"math"
	"time"
)

/* Entrance board button - there's one assigned to each floor of the building. It will decide which elevator of which column to call for you */
type BoardButton struct {

	// PROPERTIES
	Battery         *Battery
	RequestedFloor  int
	Direction       string // up|down
	isToggled       bool
	isEmittingLight bool
}

// CONSTRUCTOR :-> Initialize a board button with the default values when creating a battery's board buttons list
func (btn *BoardButton) InitBoardButton(requestedFloor int, battery *Battery) {

	btn.RequestedFloor = requestedFloor
	btn.Battery = battery
	btn.isToggled = false
	btn.isEmittingLight = false
}

// METHODS
// Send request to chosen elevator + return its value for further use
func (btn *BoardButton) Press() (Elevator, error) {

	btn.SetDirection()

	fmt.Println("\n\nELEVATOR REQUEST - FROM A BOARD BUTTON")
	time.Sleep(time.Second)

	// Print the important details of the request
	if btn.RequestedFloor > 1 {
		fmt.Printf("Someone is at RC (floor %d) and wants to go %s to floor %d. This person decides to call an elevator.",
			OriginFloor, btn.Direction, btn.RequestedFloor)

	} else {
		fmt.Printf("Someone is at RC (floor %d) and wants to go %s to B%d (floor %d). This person decides to call an elevator.",
			OriginFloor, btn.Direction, int64(math.Abs(float64(btn.RequestedFloor))), btn.RequestedFloor)
	}
	time.Sleep(time.Second)

	// Turn on the pressed button's light
	btn.isToggled = true
	btn.ControlLight()

	// Get the chosen elevator and send it the request, if at least 1 elevator has a status of 'online'
	var chosenElevator Elevator
	if elevator, err := btn.ChooseElevator(); err == nil {

		chosenElevator = elevator
		chosenElevator.SendRequest(OriginFloor, btn.Direction)

		// Turn off the pressed button's light
		btn.isToggled = false
		btn.ControlLight()

		return chosenElevator, nil

	} else {

		fmt.Println("\nChooseElevator(), BoardButton.go FAILED: ", err)

		// Turn off the pressed button's light
		btn.isToggled = false
		btn.ControlLight()

		return Elevator{}, errors.New("\nNo elevator can take the board button's request at this moment.")
	}
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

	floorDifference := btn.RequestedFloor - OriginFloor

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

			fmt.Printf("\nChosen column: Column %d", column.ID)
			return column, nil

		}
	}
	return Column{}, fmt.Errorf("\nNone of columns go to that specified floor (floor %d). Fix the floor ranges.", btn.RequestedFloor)
}

// Choose which elevator should be called from the chosen column
func (btn *BoardButton) ChooseElevator() (Elevator, error) {

	elevatorScores := []float64{}
	chosenColumn := Column{}

	// Get the chosen column, if the requested floor is valid and within range
	if col, err := btn.ChooseColumn(); err == nil {
		chosenColumn = col

	} else {
		fmt.Println("\nChooseColumn(), BoardButton.go FAILED: ", err)
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
				score += 5000 / (math.Abs(float64(floorDifference)) + 1)
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
						score += 500 / (math.Abs(float64(nextFloorDifference)) + 1)
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
	var chosenElevator Elevator
	if highestScore > -1 {

		chosenElevator = chosenColumn.ElevatorList[highestScoreIndex]
		fmt.Printf("\nChosen elevator of Column %d: Elevator %d", chosenColumn.ID, chosenElevator.ID)
		return chosenElevator, nil
	}
	return Elevator{}, errors.New("\nAll of our elevators are currently undergoing maintenance, sorry for the inconvenience.")
}

