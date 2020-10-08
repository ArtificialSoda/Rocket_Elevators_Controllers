using System;
using System.Collections.Generic;
using static System.Console;

namespace Commercial_ControllerCS
{
    class Elevator
    {
        #region STATIC FIELDS
        private static int _originFloor = 1;
        #endregion

        #region STATIC PROPERTIES
        public static int OriginFloor
        {
            get { return _originFloor; }
            set
            {
                if (value != 1)
                    throw new Exception("Invalid value. The RC (origin) floor should always be at floor 1.");
                else
                    _originFloor = value;
            }
        }
        #endregion

        #region FIELDS
        private string _status;
        private string _movement;
        private int _maxWeightKG;
        private int _currentFloor;
        private int _nextFloor;
        #endregion

        #region PROPERTIES
        public int ID { get; private set; }
        public string Status 
        {
            get { return _status; }
            set
            {
                if (value.ToLower() != "online" && value.ToLower() != "offline")
                    throw new Exception("Invalid value for elevator's status. Can only be either 'online' or 'offline'.");
                else
                    _status = value;
            }
        }
        public string Movement
        {
            get { return _movement; }
            set
            {
                if (value.ToLower() != "up" && value.ToLower() != "down" && value.ToLower() != "idle")
                    throw new Exception("Invalid value for elevator's movement. Can only be either 'up', 'down', or 'idle'.");
                else
                    _movement = value;
            }
        }
        public int MaxWeightKG
        {
            get { return _maxWeightKG; }
            private set
            {
                if (value <= 0)
                    throw new Exception("Invalid max weight limit for elevators. Can't be 0kg or lower.");
                else
                    _maxWeightKG = value;
            }
        }
        public int CurrentFloor 
        {
            get { return _currentFloor; }
            set
            {
                if (value > Battery.NumFloors || value < -(Battery.NumBasements))
                    throw new Exception("The current floor value provided for the elevator is invalid.");
                else
                    _currentFloor = value;
            }
        }
        public int NextFloor 
        {
            get { return _nextFloor; }
            set
            {
                if (value > Battery.NumFloors || value < -(Battery.NumBasements))
                    throw new Exception("The current floor value provided for the elevator is invalid.");
                else
                    _nextFloor = value;
            }
        }

        public Column Column { get; private set; }
        public FloorDisplay FloorDisplay { get; private set; }
        public List<Request> RequestsQueue { get; set; }
        public ElevatorDoor Door { get; set; }
        #endregion

        #region CONSTRUCTOR
        public Elevator(int id_, Column column_)
        {
            ID = id_;
            Column = column_;
            Status = "online"; // online|offline
            Movement = "idle"; // up|down|idle
            CurrentFloor = 1;
            FloorDisplay = new FloorDisplay(this);
            RequestsQueue = new List<Request>();
            Door = new ElevatorDoor("closed");
        }
        #endregion

        #region METHODS
        // Change properties of elevator in one line - USE ONLY FOR TESTING
        public void ChangeProperties(int newCurrentFloor, int newNextFloor)
        {
            CurrentFloor = newCurrentFloor;
            NextFloor = newNextFloor;

            if (CurrentFloor > NextFloor)
                Movement = "down";
            else
                Movement = "up";
                
            RequestsQueue.Add(new Request(NextFloor, Movement));
        }
        public void ChangeProperties(int newCurrentFloor)
        {
            CurrentFloor = newCurrentFloor;
            Movement = "idle";
        }

        // Make elevator go to its scheduled next floor
        public void GoToNextFloor()
        {
            if (CurrentFloor != NextFloor)
            {
                if (CurrentFloor > 0)
                    WriteLine($"Elevator {ID} of Column {Column.ID}, currently at floor {CurrentFloor}, is about to go to floor {NextFloor}...");
                else if (NextFloor < 0)
                    WriteLine($"Elevator {ID} of Column {Column.ID}, currently at floor B{Math.Abs(CurrentFloor)}, is about to go to floor B{Math.Abs(NextFloor)}...");
                else if (NextFloor > 0)
                    WriteLine($"Elevator {ID} of Column {Column.ID}, currently at floor B{Math.Abs(CurrentFloor)}, is about to go to floor {NextFloor}...");
                WriteLine("=================================================================");

                FloorDisplay.DisplayFloor();

                // Traverse through the floors
                while (CurrentFloor != NextFloor)
                {
                    // Do not display floors that are not part of the column's range
                    if (Movement.ToLower() == "up")
                    {
                        if (CurrentFloor + 1 < Column.LowestFloor)
                        {
                            WriteLine($"\n... Quickly traversing through the floors not in column {Column.ID}'s usual elevator range ...\n"); ;
                            CurrentFloor = Column.LowestFloor;
                        }
                        else
                            CurrentFloor++;
                    }
                    else
                    {
                        if (CurrentFloor - 1 < Column.LowestFloor)
                        {
                            WriteLine($"\n... Quickly traversing through the floors not in column {Column.ID}'s usual elevator range ...\n");
                            CurrentFloor = OriginFloor;
                        }
                        else
                            CurrentFloor--;
                    }
                    FloorDisplay.DisplayFloor();
                }

                WriteLine("=================================================================");
                if (CurrentFloor > 0)
                    WriteLine($"Elevator {ID} of Column {Column.ID} has reached its requested floor! It is now at floor {CurrentFloor}");
                else
                    WriteLine($"Elevator {ID} of Column {Column.ID} has reached its requested floor! It is now at floor B{Math.Abs(CurrentFloor)}");
            }
        }

        // Make elevator go to origin floor
        public void GoToOrigin()
        {
            NextFloor = OriginFloor;
            WriteLine($"Elevator {ID} of Column {Column.ID} going back to RC (floor {Elevator.OriginFloor})...");
            GoToNextFloor();
        }

        // Set what should be the movement direction of the elevator for its upcoming request
        private void SetMovement()
        {
            int floorDifference = CurrentFloor - RequestsQueue[0].Floor;

            if (floorDifference > 0)
                Movement = "down";
            else if (floorDifference < 0)
                Movement = "up";
            else
                Movement = "idle";
        }

        // Sort requests, for added efficiency
        private void SortRequestsQueue()
        {
            Request request = RequestsQueue[0];

            // Remove any requests which are useless i.e. requests that are already on their desired floor
            foreach (var req in RequestsQueue.ToArray())
            {

                if (req.Floor == CurrentFloor)
                    RequestsQueue.Remove(RequestsQueue.Find(x => x.Floor == CurrentFloor));
            }

            SetMovement();

            if (RequestsQueue.Count > 1)
            {
                if (Movement == "up")
                {
                    // Sort the queue in ascending order
                    RequestsQueue.Sort((x, y) => x.Floor.CompareTo(y.Floor));

                    // Push any request to the end of the queue that would require a direction change
                    foreach (var req in RequestsQueue.ToArray())
                    {
    
                        if (req.Direction != Movement || req.Floor < CurrentFloor)
                        {
                            RequestsQueue.Remove(req);
                            RequestsQueue.Add(req);
                        }
                    }
                }

                else
                {
                    // Reverse the sorted queue (will now be in descending order)
                    RequestsQueue.Sort((x, y) => y.Floor.CompareTo(x.Floor));

                    // Push any request to the end of the queue that would require a direction change
                    foreach (var req in RequestsQueue.ToArray())
                    {

                        if (req.Direction != Movement || req.Floor > CurrentFloor)
                        {
                            RequestsQueue.Remove(req);
                            RequestsQueue.Add(req);
                        }
                    }
                }

            }
        }

        // Complete the elevator requests
        public void DoRequests()
        {
            if (RequestsQueue.Count > 0)
            {
                // Make sure queue is sorted before any request is completed
                SortRequestsQueue();
                Request requestToComplete = RequestsQueue[0];

                // Go to requested floor
                if (Door.Status != "closed")
                    Door.CloseDoor();
                NextFloor = requestToComplete.Floor;
                GoToNextFloor();

                // Remove request after it is complete
                Door.OpenDoor();
                RequestsQueue.Remove(requestToComplete);

                // Automatically close door
                Door.CloseDoor();
            }
            // Automatically go idle temporarily if 0 requests or at the end of request
            Movement = "idle";
        }

        // Check if elevator is at full capacity
        public void CheckWeight(int currentWeightKG)
        {
            // currentWeightKG calculated thanks to weight sensors
            if (currentWeightKG > MaxWeightKG)
            {
                // Display 10 warnings
                for (var i = 0; i < 10; i++)
                {
                    WriteLine($"\nALERT: Maximum weight capacity reached on Elevator {ID} of Column {Column.ID}");
                }

                // Freeze elevator until weight goes back to normal
                Movement = "idle";
                Door.OpenDoor();
            }
        }
        #endregion
    }
}
