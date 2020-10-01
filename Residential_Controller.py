import sys
"""
/*****************************************************
    Author: Fabien H. Dimitrov
    Context: Codeboxx Week 2 (Odyssey)
*****************************************************/
"""
#################################################################################################################################################
# OBJECT DEFINITIONS
#################################################################################################################################################
class Battery:
    
    # CLASS VARIABLES
    num_columns = None

    # INSTANCE VARIABLES
    def __init__(self, id):
        self.id = id
        self.status = "online" # offline|online
        self.is_fire = False
        self.is_power_outage = False
        self.is_mechanical_failure = False
        self.column_list = []

    # METHODS

    # Initialize the battery's collection of columns
    def create_column_list(self):
        for columnID in range(self.num_columns):
            column = Column(columnID + 1)
            column.create_elevator_list()
            column.create_call_buttons()

            self.column_list.append(column)
    
    # Monitor the battery's elevator system
    # In real conditions, the monitoring would be done in a near-infinite while loop
    def monitor_system(self):
        if (self.is_fire or self.is_power_outage or self.is_mechanical_failure):
            
            self.status = "offline"
            for column in self.column_list:
                column.status = "offline"
                for elevator in column.elevator_list:
                    elevator.status = "offline"
            sys.exit("Battery {} has been shut down for maintenance. Sorry for the inconvenience".format(self.id))

class Column:
    
    # CLASS VARIABLES
    num_elevators = None
    
    # INSTANCE VARIABLES
    def __init__(self, id):
        self.id = id
        self.status = "online" # offline|online
        self.elevator_list = []
        self.up_call_buttons = []
        self.down_call_buttons = []
    
    # METHODS

    # Initialize the column's collection of elevators
    def create_elevator_list(self):
        
        for elevatorID in range(self.num_elevators):
            elevator = Elevator(elevatorID + 1)
            elevator.create_floor_buttons()
            self.elevator_list.append(elevator)
    
    # Initialize all the call buttons, on each floor
    def create_call_buttons(self):
        
        for floor in range(Elevator.num_floors):
            
            up_btn = CallButton(floor + 1, "up", self)
            self.up_call_buttons.append(up_btn)

            down_btn = CallButton(floor + 1, "down", self)
            self.down_call_buttons.append(down_btn)
    
    # Complete request that was sent to chosen elevator
    # Return chosen elevator, for further use
    def request_elevator(self, requested_floor, direction):
        if direction == "up":
            call_btn_to_press  = [btn for btn in self.up_call_buttons if btn.floor == requested_floor][0]
        elif direction == "down":
            call_btn_to_press  = [btn for btn in self.down_call_buttons if btn.floor == requested_floor][0]
       
        chosen_elevator = call_btn_to_press.press()
        chosen_elevator.do_requests()

        return chosen_elevator
    
    # Move chosen elevator to requested floor
    def request_floor(self, elevator, requested_floor):
        floor_btn_to_press = [btn for btn in elevator.floor_buttons if btn.floor == requested_floor][0]
        
        floor_btn_to_press.press()
        elevator.do_requests()
    
class Elevator:

    # CLASS VARIABLES
    origin_floor = 1
    max_weight_kg = 1000
    num_floors = None

    # INSTANCE VARIABLES
    def __init__(self, id):
        self.id = id
        self.status = "online" # offline|online
        self.movement = "idle" # idle|up|down
        self.current_floor = self.origin_floor
        self.next_floor = None
        self.requests_queue = []
        self.floor_buttons = []
        self.door = ElevatorDoor("closed")
        self.floor_display = FloorDisplay(self)
    
    # METHODS

    # Change properties of elevator in one line - USE ONLY FOR TESTING
    def change_properties(self, new_current_floor, new_next_floor, new_movement):
        self.current_floor = new_current_floor
        self.next_floor = new_next_floor
        self.movement = new_movement
    
    # Create all floor buttons that should be inside the elevator
    def create_floor_buttons(self):
        for floor in range(self.num_floors):
            button = FloorButton(floor + 1, self)
            self.floor_buttons.append(button)

    # Make elevator go to its scheduled next floor
    def go_to_next_floor(self):
        print("Elevator {}, currently at floor {}, is about to go to floor {}...".format(self.id, self.current_floor, self.next_floor))
        print("=====================================================================")

        while (self.current_floor != self.next_floor):
            
            if (self.movement == "up"):
                self.current_floor += 1
            elif (self.movement == "down"):
                self.current_floor -= 1
            
            self.floor_display.display_floor()
        
        print("======================================================================")
        print("Elevator {} has reached its requested floor! It is now at floor {}.".format(self.id, self.current_floor))

    # Make elevator go to origin floor
    def go_to_origin(self):
        self.next_floor = self.origin_floor
        print("Elevator {} going back to origin...".format(self.id))
        self.go_to_next_floor()

    # Get what should be the movement direction of the elevator for its upcoming request
    def get_movement(self):
        floor_difference = self.current_floor - self.requests_queue[0].floor
        if floor_difference > 0:
            self.movement = "down"
        else:
            self.movement = "up"
        
    # Sort requests, for added efficiency
    def sort_requests_queue(self):
        request = self.requests_queue[0]
        self.get_movement()

        if len(self.requests_queue) > 1:

            if self.movement == "up":
                # Sort the queue in ascending order
                self.requests_queue.sort()

                # Push any request to the end of the queue that would require a direction change
                for request in self.requests_queue:
                    if request.floor < self.current_floor or request.direction != self.movement:
                        self.requests_queue.append(self.requests_queue.pop(request))
            
            elif self.movement == "down":
                # Sort the queue in descending order
                self.requests_queue.sort(reverse=True)

                # Push any request to the end of the queue that would require a direction change
                for request in self.requests_queue:
                    if request.floor > self.current_floor or request.direction != self.movement:
                        self.requests_queue.append(self.requests_queue.pop(request))
        
    # Complete the elevator requests
    def do_requests(self):
        if len(self.requests_queue) > 0:
            # Make sure queue is sorted before any request is completed
            self.sort_requests_queue()
            request = self.requests_queue[0]

            # Go to requested floor
            self.door.close_door()
            self.next_floor = request.floor
            self.go_to_next_floor()

            # Remove request after it is complete
            self.door.open_door()
            self.requests_queue.remove(request)

            # Automatically close the door
            self.door.close_door()
            
        # Automatically go idle if 0 requests or at the end of request
        self.movement = "idle"
    
    # Check if elevator is at full capacity
    def check_weight(self, current_weight_kg):
        # currentWeightKG calculated thanks to weight sensors
        if (current_weight_kg > self.max_weight_kg):

            # Display 10 warnings
            for _ in range(10):
                print("\nALERT: Maximum weight capacity reached on Elevator {}".format(self.id))
            
            # Freeze elevator until weight goes back to normal
            if (self.movement != "idle"):
                self.movement = "idle"
    
            self.door.open_door()

class ElevatorDoor:


    # INSTANCE VARIABLES
    def __init__(self, status):
        self.status = status
    
    # METHODS
    def open_door(self):
        self.status = "opened"
    
    def close_door(self):
        self.status = "closed"

class FloorButton:
    # INSTANCE VARIABLES
    def __init__(self, floor, elevator):
        self.floor = floor
        self.elevator = elevator
        self.direction = None
        self.is_toggled = False
        self.is_emitting_light = False
    
    # METHODS
    
    def press(self):

        print("\nFLOOR REQUEST")
        print("Someone is currently on floor {}, inside Elevator {}. The person decides to go to floor {}.".format(
            self.elevator.current_floor,
            self.elevator.id,
            self.floor
        ))

        self.is_toggled = True
        self.control_light()

        self.send_request()

        self.isToggled = False
        self.control_light()
    
    # Light up a pressed button
    def control_light(self):
        if (self.is_toggled):
            self.is_emitting_light = True
        else:
            self.is_emitting_light = False
    
    # Send new request to its elevator
    def send_request(self):
        request = Request(self.floor, self.direction)
        self.elevator.requests_queue.append(request)

class FloorDisplay:
    # INSTANCE VARIABLES 
    def __init__(self, elevator):
        self.elevator = elevator
    
    #METHODS

    #Displays current floor of elevator as it travels
    def display_floor(self):
        print("... Elevator {}'s current floor mid-travel: {} ...".format(self.elevator.id, self.elevator.current_floor))

class CallButton:
    
    # INSTANCE VARIABLES
    def __init__(self, floor, direction, column):
        self.floor = floor
        self.direction = direction
        self.column = column
        self.is_toggled = False
        self.is_emitting_light = False
    
    # METHODS

    def press(self):
        
        print("\nELEVATOR REQUEST")
        print("Someone is on floor {}. The person decides to call an elevator.".format(self.floor))

        self.is_toggled = True
        self.control_light()

        chosen_elevator = self.choose_elevator()
        self.send_request(chosen_elevator)

        self.is_toggled = False
        self.control_light()

        return chosen_elevator 

    # Light up a pressed button
    def control_light(self):
        if self.is_toggled:
            self.is_emitting_light = True
        else:
            self.is_emitting_liht = False
        
    # Choose which elevator to call
    def choose_elevator(self):
        elevator_scores = []

        for elevator in self.column.elevator_list:
            
            # Initialize score to 0
            score = 0
            floor_difference = elevator.current_floor - self.floor

            # Prevents use of any offline/under-maintenance elevators
            if (elevator.status != "online"):
                score = -1
                elevator_scores.append(score)
            else:

                # Bonify score based on difference in floors
                if floor_difference == 0:
                    score += 5000
                else:
                    score += 5000/(abs(floor_difference) + 1)
                
                # Bonify score based on direction (highest priority)
                if elevator.movement != "idle":
                    if (floor_difference >= 0 and self.direction == "down" and elevator.movement == "down"):
                        
                        # Paths are crossed going down, therefore favor this elevator
                        score += 10000
                    
                    elif (floor_difference <= 0 and self.direction == "up" and elevator.movement == "up"):

                        # Paths are crossed going down, therefore favor this elevator
                        score += 10000
                    
                    else:
                        
                        # Paths are not crossed, therefore try avoiding the use of this elevator
                        score = 0
            
                        # Give redemption points, in worst case scenario where all elevators never cross paths
                        next_floor_difference = elevator.next_floor - self.floor
                        if next_floor_difference == 0:
                            score += 500
                        else:
                            score += 500/(abs(next_floor_difference) + 1)
                
                # Bonify score on request queue size (the smaller number of pre-existing requests, the faster therefore the better)
                if len(elevator.requests_queue) <= 3:
                    score += 1000
                elif len(elevator.requests_queue) <= 7:
                    score += 250
                
                # Send total score of elevator to the scores list
                elevator_scores.append(score)
            
        # Get value of highest score
        highest_score = -1
        for score in elevator_scores:
            if (score > highest_score):
                highest_score = score
        
        # Get elevator with the highest score (or None if all elevators were offline
        chosen_elevator = None
        if (highest_score > -1):
            chosen_elevator = self.column.elevator_list[elevator_scores.index(highest_score)]

        print("Chosen elevator's ID: {}".format(chosen_elevator.id))
        return chosen_elevator

    # Send new request to chosen elevator
    def send_request(self, elevator):
        request = Request(self.floor, self.direction)
        elevator.requests_queue.append(request)
    
class Request:
    # INSTANCE VARIABLES
    def __init__(self, floor, direction):
        self.floor = floor
        self.direction = direction

#################################################################################################################################################
# TEST SCENARIOS
#################################################################################################################################################
# Variables
Battery.num_columns = 1
Column.num_elevators = 2
Elevator.num_floors = 10

# Instantiate the batteries, the columns, and the elevators
battery = Battery(1)
battery.create_column_list()
battery.monitor_system()

# Set placeholder for column used in test scenario
column = battery.column_list[0]

### SCENARIO 1 ###
def scenario1():
    print("**********************************************************************************************************************************")
    print("SCENARIO 1")
    print("**********************************************************************************************************************************")

    column.elevator_list[0].change_properties(2, None, "idle")
    column.elevator_list[1].change_properties(6, None, "idle")

    chosen_elevator = column.request_elevator(3, "up")
    column.request_floor(chosen_elevator, 7)

### SCENARIO 2 ###
def scenario2():
    print("\n\n")
    print("**********************************************************************************************************************************")
    print("SCENARIO 2")
    print("**********************************************************************************************************************************")

    column.elevator_list[0].change_properties(10, None, "idle")
    column.elevator_list[1].change_properties(3, None, "idle")

    chosen_elevator = column.request_elevator(1, "up")
    column.request_floor(chosen_elevator, 6)

    print("\n\n\n=== 2 MINUTES LATER ===\n\n")

    chosen_elevator = column.request_elevator(3, "up")
    column.request_floor(chosen_elevator, 5)

    print("\n\n\n=== AFTER A BIT MORE TIME ===\n\n")

    chosen_elevator = column.request_elevator(9, "down")
    column.request_floor(chosen_elevator, 2)

### SCENARIO 3 ###
def scenario3():
    print("\n\n")
    print("**********************************************************************************************************************************")
    print("SCENARIO 3")
    print("**********************************************************************************************************************************")

    column.elevator_list[0].change_properties(10, None, "idle")
    column.elevator_list[1].change_properties(3, 6, "up")

    chosen_elevator = column.request_elevator(3, "down")

    column.request_floor(chosen_elevator, 2)
    column.request_floor(column.elevator_list[1], 6)

    print("\n\n\n=== 5 MINUTES LATER ===\n\n")

    chosen_elevator = column.request_elevator(10, "down")
    column.request_floor(chosen_elevator, 3)


scenario1()
scenario2()
scenario3()

    


