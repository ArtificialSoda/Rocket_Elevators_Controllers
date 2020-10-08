package main

import (
	"errors"
	"fmt"
	"math"
	"time"
)

/* Elevator call button - one on each floor, on each column, for each elevator of that column. Brings you to the ground floor (RC) */
type CallButton struct {

	// PROPERTIES
	Column          *Column
	Floor           int
	Direction       string // up|down
	isToggled       bool
	isEmittingLight bool
}

// CONSTRUCTOR :-> Initialize a call button with the default values when creating a column's call buttons list
func (btn *CallButton) InitCallButton(floor int, column *Column) {

	btn.Floor = floor
	btn.Column = column
	btn.isToggled = false
	btn.isEmittingLight = false
}

// METHODS
// Send request to chosen elevator + return its value for further use
func (btn *CallButton) Press() (Elevator, error) {

	btn.SetDirection()

	fmt.Println("\nELEVATOR REQUEST - FROM A CALL BUTTON")
	time.Sleep(time.Second)

	// Print the important details of the request
	if btn.Floor > 1 {
		fmt.Printf("Someone is on floor %d and will now have to go %s to RC (floor %d). This person decides to call an elevator.",
			btn.Floor, btn.Direction, OriginFloor)

	} else {
		fmt.Printf("Someone is on floor B%d (floor %d) and will now have to go %s to RC (floor %d). This person decides to call an elevator.",
			int64(math.Abs(float64(btn.Floor))), btn.Floor, btn.Direction, OriginFloor)
	}
	time.Sleep(time.Second)

	// Turn on the pressed button's light
	btn.isToggled = true
	btn.ControlLight()

	// Get the chosen elevator and send it the request, if at least 1 elevator has a status of 'online'
	var chosenElevator Elevator
	if elevator, err := btn.ChooseElevator(); err != nil {

		chosenElevator = elevator
		btn.SendRequest(chosenElevator)

		// Turn off the pressed button's light
		btn.isToggled = false
		btn.ControlLight()

		return chosenElevator, nil

	} else {

		fmt.Println("ChooseElevator(), CallButton.go FAILED: ", err)

		// Turn off the pressed button's light
		btn.isToggled = false
		btn.ControlLight()

		return Elevator{}, errors.New("No elevator can take the call button's request at this moment.")
	}
}

// Set what is the direction of the request when requesting an elevator to pick you up from a floor
func (btn *CallButton) SetDirection() {

	if btn.Floor > 1 {
		btn.Direction = "down"
	} else {
		btn.Direction = "up"
	}
}

// Light up a pressed button
func (btn *CallButton) ControlLight() {

	if btn.isToggled {
		btn.isEmittingLight = true

	} else {
		btn.isEmittingLight = false
	}
}

// Choose which elevator should be called
func (btn *CallButton) ChooseElevator() (Elevator, error) {

	elevatorScores := []float64{}

	for _, elevator := range btn.Column.ElevatorList {

		// Initialize score to 0
		var score float64 = 0

		// Calculate floor difference differently based on whether or not elevator is already at RC or not
		// Remember: Each column has a floor range that its elevators must respect. The RC is not included in the range, so to make the calculations fair, if elevator is already at RC the floor difference will still look normal thanks to the 2nd calculation option as it will start from the column's lowest floor instead of at RC.
		floorDifference := 0

		if elevator.CurrentFloor != OriginFloor {
			floorDifference = elevator.CurrentFloor - btn.Floor

		} else {
			floorDifference = btn.Column.LowestFloor - btn.Floor
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

				if floorDifference >= 0 && btn.Direction == "down" && elevator.Movement == "down" {

					// Paths are crossed going down, therefore favor this elevator
					score += 10000

				} else if floorDifference <= 0 && btn.Direction == "up" && elevator.Movement == "up" {

					// Paths are crossed going up, therefore favor this elevator
					score += 10000

				} else {

					// Paths are not crossed, therefore try avoiding the use of this elevator
					score = 0

					// Calculate next floor difference differently based on whether or not elevator's next floor will be at RC or not
					nextFloorDifference := 0
					if elevator.NextFloor != OriginFloor {
						nextFloorDifference = elevator.NextFloor - btn.Floor

					} else {
						nextFloorDifference = btn.Column.LowestFloor - btn.Floor
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

		chosenElevator = btn.Column.ElevatorList[highestScoreIndex]
		fmt.Printf("Chosen elevator of Column %d: Elevator %d", btn.Column.ID, chosenElevator.ID)
		return chosenElevator, nil
	}
	return Elevator{}, errors.New("All of our elevators are currently undergoing maintenance, sorry for the inconvenience.")
}

// Send new request to chosen elevator
func (btn CallButton) SendRequest(elevator Elevator) {

	r := Request{btn.Floor, btn.Direction}
	elevator.RequestsQueue = append(elevator.RequestsQueue, r)
}
