using System;
using System.Collections.Generic;
using static System.Console;
using static System.Threading.Thread;

namespace Commercial_Controller
{
    /// <summary>
    /// Elevator call button - one on each floor, on each column, for each elevator of that column. Brings you to the ground floor (RC).
    /// </summary>
    class CallButton
    {
        #region FIELDS
        private int _floor;
        private readonly Column _column;
        private string _direction;
        private bool _isToggled;
        private bool _isEmittingLight;
        #endregion

        #region PROPERTIES
        public int Floor
        {
            get { return _floor; }
            private set
            {
                if (value > Battery.NumFloors || value < -(Battery.NumBasements))
                    throw new Exception("The floor value provided for the call button is invalid.");
                else
                    _floor = value;
            }
        }
        public string Direction
        {
            get { return _direction; }
            private set
            {
                if (value.ToLower() != "up" || value.ToLower() != "down")
                    throw new Exception("The direction provided for the call button is invalid. It can only be 'up' or 'down'.");
                else
                    _direction = value;
            }
        }
        public Column Column { get; private set; }
        #endregion

        #region CONSTRUCTOR
        public CallButton(int floor_, Column column_)
        {
            Floor = floor_;
            Column = column_;
            _isToggled = false;
            _isEmittingLight = false;
        }
        #endregion

        #region METHODS
        // Send request to chosen elevator + return its value for further use. 
        public Elevator Press()
        {
            WriteLine("\nELEVATOR REQUEST");
            Sleep(Program.SLEEP_TIME);
            WriteLine($"Someone is on floor {Floor} and will now have to go to floor {REQUESTED_FLOOR}. This person decides to call an elevator.");
            Sleep(Program.SLEEP_TIME);

            _isToggled = true;
            ControlLight();
            SetDirection();

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

        // Set what is the direction when requesting an elevator
        public void SetDirection()
        {
            int originDifference = Elevator.OriginFloor - Floor;
            Direction = (originDifference > 0) ? "up" : "down";
        }

        // Choose which elevator should be called
        public Elevator ChooseElevator()
        {
            List<int> elevatorScores = new List<int>();
            
            foreach (var elevator in Column.ElevatorList)
            {
                // Initialize score to 0
                int score = 0;

                // Calculate floor difference differently based on whether or not elevator is already at RC or not
                // Remember: Each column has a floor range that its elevators must respect. The RC is not included in the range, so to make the calculations fair, if elevator is already at RC the floor difference will still look normal thanks to the 2nd calculation option as it will start from the column's lowest floor instead of at RC.
                int floorDifference;

                if (elevator.CurrentFloor != 1)
                    floorDifference = elevator.CurrentFloor - Floor;
                else
                    floorDifference = Column.LowestFloor - Floor;

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
                        if (floorDifference >= 0 && Direction == "down" && elevator.Movement == "down")
                        {
                            // Paths are crossed going down, therefore favor this elevator
                            score += 10000;
                        }
                        else if (floorDifference <= 0 && Direction == "up" && elevator.Movement == "up")
                        {
                            // Paths are crossed going up, therefore favor this elevator
                            score += 10000;
                        }
                        else
                        {
                            // Paths are not crossed, therefore try avoiding the use of this elevator
                            score = 0;

                            // Give redemption points, in worst case scenario where all elevators never cross paths
                            int nextFloorDifference = (int)(elevator.NextFloor - Floor);
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
                chosenElevator = Column.ElevatorList[index];

                WriteLine($"Chosen elevator of Column {Column.ID}: Elevator ${chosenElevator.ID}");
            }
            return chosenElevator;
        }

        // Send new request to chosen elevator 
        public void SendRequest(Elevator elevator)
        {
            var request = new Request(Floor, Direction);
            elevator.RequestsQueue.Add(request);
        }
        #endregion
    }
}
