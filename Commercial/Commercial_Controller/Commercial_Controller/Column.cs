using System;
using System.Collections.Generic;

namespace Commercial_Controller
{
    class Column
    {
        #region STATIC FIELDS
        private static int _numElevators;
        #endregion

        #region STATIC PROPERTIES
        public static int NumElevators
        {
            get { return _numElevators;  }
            set
            {
                if (value <= 0)
                    throw new Exception("There has to be more than 0 elevators in each column.");
                else
                    _numElevators = value;
            }
        }
        #endregion

        #region FIELDS
        private string _status;
        private int _lowestFloor;
        private int _highestFloor;
        #endregion

        #region PROPERTIES
        public int ID { get; private set; }
        public string Status
        {
            get { return _status; }
            set
            {
                if (value.ToLower() != "online" && value.ToLower() != "offline")
                    throw new Exception($"Invalid value for column {ID}'s status. Can only be either 'online' or 'offline'.");
                else
                    _status = value;
            }
        }
        public int LowestFloor
        {
            get { return _lowestFloor; }
            set
            {
                if (value > Battery.NumFloors || value < -(Battery.NumBasements))
                    throw new Exception($"The lowest floor value provided for Column {ID} is invalid.");
                else
                    _lowestFloor = value;
            }
        }
        public int HighestFloor
        {
            get { return _highestFloor; }
            set
            {
                if (value > Battery.NumFloors || value < -(Battery.NumBasements))
                    throw new Exception($"The highest floor value provided for Column {ID} is invalid.");
                else
                    _highestFloor = value;
            }
        }
        public List<Elevator> ElevatorList { get; set; }
        public List<CallButton> CallButtonsList { get; set; }
        #endregion

        #region CONSTRUCTOR
        public Column(int id_)
        {
            ID = id_;
            Status = "online"; // online|offline
            ElevatorList = new List<Elevator>();
            CallButtonsList = new List<CallButton>();
        }
        #endregion

        #region METHODS
        // Initialize the column's collection of elevators
        public void CreateElevatorList()
        {
            for (var elevatorID = 1; elevatorID <= NumElevators; elevatorID++)
            {
                var elevator = new Elevator(elevatorID, this);
                ElevatorList.Add(elevator);
            }
        }

        // Initialize all the call buttons, on each floor
        public void CreateCallButtons()
        {

            for (int numFloor = LowestFloor; numFloor <= HighestFloor; numFloor++)
            {
                if (numFloor != 0)
                {
                    var callBtn = new CallButton(numFloor, this);
                    CallButtonsList.Add(callBtn);
                }
            }
        }

        // Request an elevator to current location (if current location is not RC)
        public void RequestElevator(int floorNumber)
        {
            var callBtnToPress = CallButtonsList.Find(btn => btn.Floor == floorNumber);
            Elevator chosenElevator = callBtnToPress.Press();

            // Do requests until elevator has reached the floor where the call was made
            while (chosenElevator.CurrentFloor != floorNumber)
                chosenElevator.DoRequests();

            // Set a request for the elevator to go to RC, once picked up
            string originDirection = (chosenElevator.CurrentFloor < Elevator.OriginFloor) ? "up" : "down";
            var originRequest = new Request(Elevator.OriginFloor, originDirection);
            chosenElevator.RequestsQueue.Add(originRequest);

            // Do requests until elevator has reached origin floor (RC)
            while (chosenElevator.CurrentFloor != Elevator.OriginFloor)
                chosenElevator.DoRequests();
              
        }
        #endregion
    }
}
