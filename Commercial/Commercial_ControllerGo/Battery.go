package main

// STATIC PROPERTIES (in Golang, it's treated as a global)
var NumFloors int
var NumBasements int
var NumColumns int

type Battery struct {

	// PROPERTIES
	ID              int
	Status          string // online|offline
	ColumnList      []Column
	BoardButtonList []BoardButton
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
