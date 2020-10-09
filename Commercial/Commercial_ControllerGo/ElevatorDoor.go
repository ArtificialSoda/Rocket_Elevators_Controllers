package main

import (
	"fmt"
	"time"
)

type ElevatorDoor struct {

	// PROPERTIES
	Status string
}

// METHODS
func (door *ElevatorDoor) OpenDoor() {

	door.Status = "opened"
	fmt.Println("\n\nDoor has opened")
	time.Sleep(time.Second)
}

func (door *ElevatorDoor) CloseDoor() {

	door.Status = "closed"
	fmt.Println("Door has closed")
	time.Sleep(time.Second)
}
