using System;
using System.Collections.Generic;
using static System.Console;
using static System.Threading.Thread;

namespace Commercial_Controller
{
    /// <summary>
    /// Entrance board button - there's one assigned to each floor of the building. It will decide which elevator of which column to call for you.
    /// </summary>
    class BoardButton
    {
        #region FIELDS
        private Battery _battery;
        private int _requestedFloor;
        private int _floor;
        private string _direction;
        private bool _isToggled;
        private bool _isEmittingLight;
        #endregion

        #region PROPERTIES
        public int RequestedFloor
        {
            get { return _requestedFloor; }
            set
            {
                if (value > Battery.NumFloors || value < -(Battery.NumBasements))
                    throw new Exception("The floor value provided for the board button is invalid.");
                else
                    _requestedFloor = value;
            }
        }
        public string Direction
        {
            get { return _direction; }
            private set
            {
                if (value.ToLower() != "up" && value.ToLower() != "down")
                    throw new Exception("The direction provided for the board button is invalid. It can only be 'up' or 'down'.");
                else
                    _direction = value;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public BoardButton(int requestedFloor_, Battery battery_)
        {
            RequestedFloor = requestedFloor_;
            _battery = battery_;
            _floor = Elevator.OriginFloor;
            _isToggled = false;
            _isEmittingLight = false;
        }
        #endregion

        #region METHODS
        // Send request to chosen elevator + return its value for further use. 
        public Elevator Press()
        {
            SetDirection();

            WriteLine("\nELEVATOR REQUEST - FROM A BOARD BUTTON");
            Sleep(Program.SLEEP_TIME);

            if (RequestedFloor > 1)
                WriteLine($"Someone is at RC (floor {_floor}) and wants to go {Direction} to floor {RequestedFloor}. This person decides to call an elevator.");
            else
                WriteLine($"Someone is at RC (floor {_floor}) and wants to go {Direction} to B{Math.Abs(RequestedFloor)} (floor {RequestedFloor}). This person decides to call an elevator.");
            Sleep(Program.SLEEP_TIME);

            _isToggled = true;
            ControlLight();

            var chosenElevator = ChooseElevator();
            if (chosenElevator == null)
                WriteLine("All of our elevators are currently undergoing maintenance, sorry for the inconvenience.");
            else
                SendRequest(chosenElevator);

            _isToggled = false;
            ControlLight();

            return chosenElevator;
        }

        // Light up a pressed button
        public void ControlLight()
        {
            if (_isToggled)
                _isEmittingLight = true;
            else
                _isEmittingLight = false;
        }

        // Set what is the direction of the request when requesting an elevator to pick you up from RC
        public void SetDirection()
        {
            int floorDifference =  RequestedFloor - _floor;
            Direction = (floorDifference > 0) ? "up" : "down";
        }

        // Choose which column to go to, based on the requested floor
        public Column ChooseColumn()
        {
            foreach (var column in _battery.ColumnList)
            {
                if (RequestedFloor >= column.LowestFloor && RequestedFloor <= column.HighestFloor)
                {
                    WriteLine($"Chosen column: Column {column.ID}");
                    return column;
                }
            }
            throw new Exception($"None of columns go to that specified floor (floor {RequestedFloor}). Fix the floor ranges.");
        }

        // Choose which elevator should be called from the chosen column
        public Elevator ChooseElevator()
        {
            List<int> elevatorScores = new List<int>();
            var chosenColumn = ChooseColumn();

            foreach (var elevator in chosenColumn.ElevatorList)
            {
                // Initialize score to 0
                int score = 0;

                // Calculate floor difference differently based on whether or not elevator is already at RC or not
                // Remember: Each column has a floor range that its elevators must respect. The RC is not included in the range, so to make the calculations fair, if elevator is already at RC the floor difference will still look normal thanks to the 2nd calculation option as it will start from the column's lowest floor instead of at RC.
                int floorDifference;

                if (elevator.CurrentFloor != Elevator.OriginFloor)
                    floorDifference = elevator.CurrentFloor - chosenColumn.LowestFloor;
                else
                    floorDifference = elevator.CurrentFloor - Elevator.OriginFloor;

                // Prevents use of any offline/under-maintenance elevators
                if (elevator.Status != "online")
                {
                    score = -1;
                    elevatorScores.Add(score);
                }
                else
                {
                    // Bonify score based on floor difference
                    if (floorDifference == 0)
                        score += 5000;
                    else
                        score += 5000 / (Math.Abs(floorDifference) + 1);

                    // Bonify score based on direction (highest priority)
                    if (elevator.Movement != "idle")
                    {
               
                        if (floorDifference < 0 && Direction == "down" && elevator.Movement == "down")
                        {
                            // Paths are not crossed, therefore try avoiding the use of this elevator
                            score = 0;
                        }
                        else if (floorDifference > 0 && Direction == "up" && elevator.Movement == "up")
                        {
                            // Paths are not crossed, therefore try avoiding the use of this elevator
                            score = 0;
                        }
                        else
                        {
                            // Paths are crossed, therefore favor this elevator
                            score += 1000;

                            // Calculate next floor difference differently based on whether or not elevator's next floor will be at RC or not
                            int nextFloorDifference;
                            if (elevator.NextFloor != Elevator.OriginFloor)
                                nextFloorDifference = elevator.NextFloor - _floor;
                            else
                                nextFloorDifference = elevator.NextFloor - _floor;

                            // Give redemption points, in worst case scenario where all elevators never cross paths
                            if (nextFloorDifference == 0)
                                score += 500;
                            else
                                score += 500 / (Math.Abs(nextFloorDifference) + 1);
                        }
                    }

                    // Bonify score on request queue size (the smaller number of pre-existing requests, the faster therefore the better)
                    if (elevator.RequestsQueue.Count <= 3)
                        score += 1000;
                    else if (elevator.RequestsQueue.Count <= 7)
                        score += 250;

                    // Send total score of elevator to the scores list
                    elevatorScores.Add(score);
                }
            }

            // Get value of highest score
            int highestScore = -1;

            foreach (int score in elevatorScores)
            {
                if (score > highestScore)
                    highestScore = score;
            }

            // Get the elevator with the highest score (or NULL if all elevators were offline)
            Elevator chosenElevator = null;
            if (highestScore > -1)
            {
                int index = elevatorScores.FindIndex(score => score == highestScore);
                chosenElevator = chosenColumn.ElevatorList[index];

                WriteLine($"Chosen elevator of Column {chosenColumn.ID}: Elevator {chosenElevator.ID}\n");
            }
            return chosenElevator;
        }

        // Send new request to chosen elevator 
        public void SendRequest(Elevator elevator)
        {
            var request = new Request(_floor, Direction);
            elevator.RequestsQueue.Add(request);
        }
        #endregion
    }
}
