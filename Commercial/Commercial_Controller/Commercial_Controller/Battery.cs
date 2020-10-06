using System;
using System.Collections.Generic;
using static System.Console;

namespace Commercial_Controller
{
    class Battery
    {
        #region STATIC FIELDS
        public static int NumFloors { get; set; }
        public static int NumBasements { get; set; }
        public static int NumColumns { get; set; }
        #endregion

        #region PROPERTIES
        public int ID { get; private set; }
        public string Status
        {
            get { return Status; }
            set
            {
                if (value.ToLower() != "online" || value.ToLower() != "offline")
                    throw new Exception($"Invalid value for battery {ID}'s status. Can only be either 'online' or 'offline'.");
                else
                    Status = value;
            }
        }
        public bool isFire { get; set; }
        public bool isPowerOutage { get; set; }
        public bool isMechanicalFailure { get; set; }
        public List<Column> ColumnList { get; set; }
        #endregion

        #region CONSTRUCTOR
        public Battery(int id_)
        {
            ID = id_;
            Status = "online"; // online|offline
            ColumnList = new List<Column>();
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
        #endregion

    }
}
