package main

import (
	"fmt"
	"math"
	"time"
)

type FloorDisplay struct {

	// PROPERTIES
	Elevator *Elevator
}

// METHODS
func (fd *FloorDisplay) DisplayFloor() {

	time.Sleep(time.Second)

	if fd.Elevator.CurrentFloor > 0 {
		fmt.Printf("\n... Elevator %d of column %d's current floor mid-travel: %d ...", fd.Elevator.ID, fd.Elevator.Column.ID, fd.Elevator.CurrentFloor)
	} else if fd.Elevator.CurrentFloor < 0 {
		fmt.Printf("\n... Elevator %d of column %d's current floor mid-travel: B%d ...", fd.Elevator.ID, fd.Elevator.Column.ID, int64(math.Abs(float64(fd.Elevator.CurrentFloor))))
	}
}
