package main

type CallButton struct {

	// PROPERTIES
	Battery         Battery
	RequestedFloor  int
	Floor           int
	Direction       string // up|down
	isToggled       bool
	isEmittingLight bool
}
