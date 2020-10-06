using System;
using System.Collections.Generic;
using static System.Console;

namespace Commercial_Controller
{
    class Battery
    {
        #region STATIC FIELDS
        private static int _numFloors;
        private static int _numBasements;
        private static int _numColumns;
        #endregion

        #region STATIC PROPERTIES
        public static int NumFloors {
            get { return _numFloors; }
            set
            {
                if (value <= 0)
                    throw new Exception("There has to be more than 0 floors, excluding the basements.");
                else
                    _numFloors = value;
            }
        }
        public static int NumBasements
        {
            get { return _numBasements; }
            set
            {
                if (value <= 0)
                    throw new Exception("There has to be more than 0 basements.");
                else
                    _numBasements = value;
            }
        }
        public static int NumColumns
        {
            get { return _numColumns; }
            set
            {
                if (value <= 0)
                    throw new Exception("There has to be more than 0 columns.");
                else
                    _numColumns = value;
            }
        }

        #endregion

        #region FIELDS
        private string _status;
        #endregion

        #region PROPERTIES
        public int ID { get; private set; }
        public string Status
        {
            get { return _status; }
            set
            {
                if (value.ToLower() != "online" && value.ToLower() != "offline")
                    throw new Exception($"Invalid value for battery {ID}'s status. Can only be either 'online' or 'offline'.");
                else
                    _status = value;
            }
        }
        public bool isFire { get; set; }
        public bool isPowerOutage { get; set; }
        public bool isMechanicalFailure { get; set; }
        public List<Column> ColumnList { get; set; }
        public List<BoardButton> BoardButtonList { get; set; }
        #endregion

        #region CONSTRUCTOR
        public Battery(int id_)
        {
            ID = id_;
            Status = "online"; // online|offline
            ColumnList = new List<Column>();
            BoardButtonList = new List<BoardButton>();
        }
        #endregion

        #region STATIC METHODS
        // Quickly change the Battery's static properties
        public static void ChangeProperties(int numColumns_, int numFloors_, int numBasements_)
        {
            NumColumns = numColumns_;
            NumFloors = numFloors_;
            NumBasements = numBasements_;
        }
        #endregion

        #region METHODS
        // Initialize the battery's collection of columns
        public void CreateColumnList()
        {
            for (int columnID = 1; columnID <= NumColumns; columnID++)
            {
                var column = new Column(columnID);
                column.CreateElevatorList();
                column.CreateCallButtons();

               ColumnList.Add(column);
            }
        }

        // Initialize the battery's board display's buttons
        public void CreateBoardButtons()
        {
            // Board buttons for basements floors
            for (int numBasement = -(NumBasements); numBasement < 0; numBasement++)
            {
                var button = new BoardButton(numBasement, this);
                BoardButtonList.Add(button);
            }

            // Board buttons for non-basement floors
            for (int numFloor = Elevator.OriginFloor; numFloor <= NumFloors; numFloor++)
            {
                var button = new BoardButton(numFloor, this);
                BoardButtonList.Add(button);
            }

        }

        // Monitor the battery's elevator system
        // In real conditions, the monitoring would be done in a near-infinite while loop
        public void MonitorSystem()
        {
            if (isFire || isPowerOutage || isMechanicalFailure)
            {
                Status = "offline";
                foreach (var column in ColumnList)
                {
                    column.Status = "offline";
                    foreach (var elevator in column.ElevatorList)
                    {
                        elevator.Status = "offline";
                    }
                }

                // Stop execution of script (the infinite while loop is voluntary)
                Write($"Battery {ID} has been shut down for maintenance. Sorry for the inconvenience.");
                System.Environment.Exit(1);
            }
        }

        // Assigns a column and elevator to use when making a request from RC via the board buttons
        public void AssignElevator(int requestedFloor)
        {
            var boardBtnToPress = BoardButtonList.Find(btn => btn.RequestedFloor == requestedFloor);
            Elevator chosenElevator = boardBtnToPress.Press();

            // Do requests until elevator has reached the floor where the call was made
            while (chosenElevator.CurrentFloor != boardBtnToPress.Floor)
                chosenElevator.DoRequests();

            // Set a request for the elevator to go to requested floor, once picked up
            string newDirection = (chosenElevator.CurrentFloor < requestedFloor) ? "up" : "down";
            var newRequest = new Request(requestedFloor, newDirection);
            chosenElevator.RequestsQueue.Add(newRequest);

            // Do requests until elevator has reached requested floor 
            while (chosenElevator.CurrentFloor != requestedFloor)
                chosenElevator.DoRequests();

        }
        #endregion
    }
}
