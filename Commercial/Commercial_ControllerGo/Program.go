package main

import (
	"bufio"
	"fmt"
	"os"
	"time"
)

// MAIN METHOD
func main() {

	// Variables
	ChangeScenarioProperties(4, 5, 60, 6)

	// Instantiate the batteries, the columns and the elevators
	battery := Battery{}
	battery.InitBattery(1)
	battery.Run()
	battery.MonitorSystem()

	Run(battery)
}

// METHODS
// Run the full test program
func Run(battery Battery) {
	fmt.Println("\nHello! Choose which scenario you'd like to emulate [1, 2, 3, 4]: ")

	input := bufio.NewScanner(os.Stdin)
	for input.Scan() {

		if input.Text() == "1" {
			Scenario1(battery, battery.ColumnList[1]) // Covers B6 to B1 + RC
		} else if input.Text() == "2" {
			Scenario2(battery, battery.ColumnList[2]) // Covers 02 to 20 + RC
		} else if input.Text() == "3" {
			Scenario3(battery.ColumnList[3]) // Covers 21 to 40 + RC
		} else if input.Text() == "4" {
			Scenario4(battery.ColumnList[0]) // Covers 41 to 60 + RC
		} else {
			fmt.Println("\nHello! Choose which scenario you'd like to emulate [1, 2, 3, 4]: ")
			continue
		}

		fmt.Println("\n\n***** SCENARIO SUCCESSFULLY TESTED *****")
		break
	}
}

// Quickly change the Battery's static properties
func ChangeScenarioProperties(numColumns int, numElevatorsPerColumn int, numFloors int, numBasements int) {

	NumColumns = numColumns
	NumElevators = numElevatorsPerColumn
	NumFloors = numFloors
	NumBasements = numBasements
}

func Scenario1(battery Battery, column Column) {
	fmt.Println("\n**********************************************************************************************************************************\n" +
		"SCENARIO 1\n" +
		"**********************************************************************************************************************************\n")

	time.Sleep(time.Second)

	column.ElevatorList[0].ChangePropertiesActive(20, 5)
	column.ElevatorList[1].ChangePropertiesActive(3, 15)
	column.ElevatorList[2].ChangePropertiesActive(13, 1)
	column.ElevatorList[3].ChangePropertiesActive(15, 2)
	column.ElevatorList[4].ChangePropertiesActive(6, 1)

	battery.AssignElevator(20)
	time.Sleep(time.Second)
}

func Scenario2(battery Battery, column Column) {
	fmt.Println("\n**********************************************************************************************************************************\n" +
		"SCENARIO 2\n" +
		"**********************************************************************************************************************************\n")

	time.Sleep(time.Second)

	column.ElevatorList[0].ChangePropertiesActive(1, 21)
	column.ElevatorList[1].ChangePropertiesActive(23, 28)
	column.ElevatorList[2].ChangePropertiesActive(33, 1)
	column.ElevatorList[3].ChangePropertiesActive(40, 24)
	column.ElevatorList[4].ChangePropertiesActive(39, 1)

	battery.AssignElevator(36)
	time.Sleep(time.Second)
}

func Scenario3(column Column) {
	fmt.Println("\n**********************************************************************************************************************************\n" +
		"SCENARIO 3\n" +
		"**********************************************************************************************************************************\n")

	time.Sleep(time.Second)

	column.ElevatorList[0].ChangePropertiesActive(58, 1)
	column.ElevatorList[1].ChangePropertiesActive(50, 60)
	column.ElevatorList[2].ChangePropertiesActive(46, 58)
	column.ElevatorList[3].ChangePropertiesActive(1, 54)
	column.ElevatorList[4].ChangePropertiesActive(60, 1)

	column.RequestElevator(54)
	time.Sleep(time.Second)
}

func Scenario4(column Column) {
	fmt.Println("\n**********************************************************************************************************************************\n" +
		"SCENARIO 4\n" +
		"**********************************************************************************************************************************\n")

	time.Sleep(time.Second)

	column.ElevatorList[0].ChangePropertiesIdle(-4)
	column.ElevatorList[1].ChangePropertiesIdle(1)
	column.ElevatorList[2].ChangePropertiesActive(-3, -5)
	column.ElevatorList[3].ChangePropertiesActive(-6, 1)
	column.ElevatorList[4].ChangePropertiesActive(-1, -6)

	column.RequestElevator(-3)
	time.Sleep(time.Second)
}
