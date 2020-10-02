# /****************************************************
#   Author Fabien H. Dimitrov 
#   Context Codeboxx Week 2 (Odyssey)
# ****************************************************\

#################################################################################################################################################
# OBJECT DEFINITIONS
#################################################################################################################################################

class Battery
    
    #ACCESSORS
    class << self
        attr_accessor :num_columns
    end
    attr_accessor :id, :status, :is_fire, :is_power_outage, :is_mechanical_failure, :column_list

    # CLASS VARIABLES
    @@num_columns = nil

    # INSTANCE VARIABLES
    def initialize(id)
        @id = id
        @status = "online" # offline|online
        @is_fire = false
        @is_power_outage = false
        @is_mechanical_failure = false
        @column_list = []
    end

    # METHODS
    # Initialize the battery's collection of columns
    def create_column_list
        for columnID in 1..Battery.num_columns do
            column = Column.new(columnID)
            column.create_elevator_list()
            column.create_call_buttons()

            @column_list.append(column)
        end
    end
    
    # Monitor the battery's elevator system
    # In real conditions, the monitoring would be done in a near-infinite while loop
    def monitor_system
        if @is_fire || @is_power_outage || @is_mechanical_failure
            
            @status = "offline"
            for column in @column_list do
                column.status = "offline"
                for elevator in column.elevator_list do
                    elevator.status = "offline"
                end
            end
            puts "Battery #{@id} has been shut down for maintenance. Sorry for the inconvenience"
            exit()
        end
    end
end

class Column
    
    #ACCESSORS
    class << self
        attr_accessor :num_elevators
    end
    attr_accessor :id, :status, :elevator_list, :up_call_buttons, :down_call_buttons

    # CLASS VARIABLES
    @@num_elevators = nil
    
    # INSTANCE VARIABLES
    def initialize(id)
        @id = id
        @status = "online" # offline|online
        @elevator_list = []
        @up_call_buttons = []
        @down_call_buttons = []
    end
    
    # METHODS
    # Initialize the column's collection of elevators
    def create_elevator_list
        
        for elevatorID in 1..Column.num_elevators do
            elevator = Elevator.new(elevatorID)
            elevator.create_floor_buttons()
            @elevator_list.append(elevator)
        end
    end

    # Initialize all the call buttons, on each floor
    def create_call_buttons
        
        for floor in 1..Elevator.num_floors do
            
            up_btn = CallButton.new(floor, "up", self)
            @up_call_buttons.append(up_btn)

            down_btn = CallButton.new(floor, "down", self)
            @down_call_buttons.append(down_btn)
        end
    end

    # Complete request that was sent to chosen elevator
    # Return chosen elevator, for further use
    def request_elevator(requested_floor, direction)
        if direction == "up"
            call_btn_to_press  = @up_call_buttons.find { |btn| btn.floor == requested_floor } 
        elsif direction == "down"
            call_btn_to_press  = @down_call_buttons.find { |btn| btn.floor == requested_floor } 
        end

        chosen_elevator = call_btn_to_press.press()
        chosen_elevator.do_requests()

        return chosen_elevator
    end

    # Move chosen elevator to requested floor
    def request_floor(elevator, requested_floor)
        floor_btn_to_press = elevator.floor_buttons.find { |btn| btn.floor == requested_floor } 
        
        floor_btn_to_press.press()
        elevator.do_requests()
    end
end

class Elevator

    #ACCESSORS
    class << self
        attr_accessor :origin_floor, :max_weight_kg, :num_floors
    end
    attr_accessor :id, :status, :movement, :current_floor, :next_floor, :requests_queue, :floor_buttons, :door, :floor_display
    
    # CLASS VARIABLES
    @@origin_floor = 1
    @@max_weight_kg = 1000
    @@num_floors = nil

    # INSTANCE VARIABLES
    def initialize(id)
        @id = id
        @status = "online" # offline|online
        @movement = "idle" # idle|up|down
        @current_floor = @@origin_floor
        @next_floor = nil
        @requests_queue = []
        @floor_buttons = []
        @door = ElevatorDoor.new("closed")
        @floor_display = FloorDisplay.new(self)
    end
    
    # METHODS
    # Change properties of elevator in one line - USE ONLY FOR TESTING
    def change_properties(new_current_floor, new_next_floor, new_movement)
        @current_floor = new_current_floor
        @next_floor = new_next_floor
        @movement = new_movement
    end

    # Create all floor buttons that should be inside the elevator
    def create_floor_buttons
        for floor in 1..Elevator.num_floors do
            button = FloorButton.new(floor, self)
            @floor_buttons.append(button)
        end
    end

    # Make elevator go to its scheduled next floor
    def go_to_next_floor
        puts "Elevator #{@id}, currently at floor #{@current_floor}, is about to go to floor #{@next_floor}..."
        puts "====================================================================="
        
        while @current_floor != @next_floor
            if @movement == "up"
                @current_floor += 1
            elsif (@movement == "down")
                @current_floor -= 1
            end
            @floor_display.display_floor()
        end

       puts "======================================================================"
       puts "Elevator #{@id} has reached its requested floor! It is now at floor #{@current_floor}."
    end

    # Make elevator go to origin floor
    def go_to_origin
        @next_floor = @origin_floor
        puts "Elevator #{@id} going back to origin..."
        go_to_next_floor()
    end

    # Get what should be the movement direction of the elevator for its upcoming request
    def get_movement
        floor_difference = @current_floor - @requests_queue[0].floor
        if floor_difference > 0
            @movement = "down"
        else
            @movement = "up"
        end
    end
        
    # Sort requests, for added efficiency
    def sort_requests_queue
        request = @requests_queue[0]
        get_movement()

        if @requests_queue.length() > 1

            if @movement == "up"
                # Sort the queue in ascending order
                @requests_queue.sort!

                # Push any request to the end of the queue that would require a direction change
                for request in @requests_queue do
                    if request.floor < @current_floor || request.direction != @movement
                        @requests_queue.append(@requests_queue.delete(request))
                    end
                end

            elsif @movement == "down"
                # Sort the queue in descending order
                @requests_queue.sort!
                @requests_queue.reverse!

                # Push any request to the end of the queue that would require a direction change
                for request in @requests_queue do
                    if request.floor > @current_floor || request.direction != @movement
                        @requests_queue.append(@requests_queue.delete(request))
                    end
                end
            end
        end
    end

    # Complete the elevator requests
    def do_requests
        if @requests_queue.length() > 0
            # Make sure queue is sorted before any request is completed
            sort_requests_queue()
            request = @requests_queue[0]

            # Go to requested floor
            @door.close_door()
            @next_floor = request.floor
            go_to_next_floor()

            # Remove request after it is complete
            @door.open_door()
            @requests_queue.delete(request)

            # Automatically close the door
            @door.close_door()
        end

        # Automatically go idle if 0 requests or at the end of request
        @movement = "idle"
    end
    
    # Check if elevator is at full capacity
    def check_weight(current_weight_kg)
        
        # currentWeightKG calculated thanks to weight sensors
        if (current_weight_kg > @max_weight_kg)

            # Display 10 warnings
            for _ in 1..10 do
               puts "\nALERT Maximum weight capacity reached on Elevator #{@id}"
            end

            # Freeze elevator until weight goes back to normal
            if (@movement != "idle")
                @movement = "idle"
            end
    
            @door.open_door()
        end
    end
end

class ElevatorDoor
    
    # INSTANCE VARIABLES
    def initialize(status)
        @status = status
    end
    
    # METHODS
    def open_door
        @status = "opened"
    end
    
    def close_door
        @status = "closed"
    end
end

class FloorButton
    
    # ACCESSORS
    attr_accessor :floor, :elevator, :direction

    # INSTANCE VARIABLES
    def initialize(floor, elevator)
        @floor = floor
        @elevator = elevator
        @direction = nil
        @is_toggled = false
        @is_emitting_light = false
    end
    
    # METHODS
    def press

        puts "\nFLOOR REQUEST"
        puts "Someone is currently on floor #{@elevator.current_floor}, inside Elevator #{@elevator.id}. The person decides to go to floor #{@floor}."

        @is_toggled = true
        control_light()

        send_request()

        @isToggled = false
        control_light()
    end

    # Light up a pressed button
    def control_light
        if @is_toggled
            @is_emitting_light = true
        else
            @is_emitting_light = false
        end
    end
    
    # Send new request to its elevator
    def send_request
        request = Request.new(@floor, @direction)
        @elevator.requests_queue.append(request)
    end
end

class FloorDisplay
    
    # INSTANCE VARIABLES 
    def initialize(elevator)
        @elevator = elevator
    end

    #METHODS
    #Displays current floor of elevator as it travels
    def display_floor
        puts "... Elevator #{@elevator.id}'s current floor mid-travel #{@elevator.current_floor} ..."
    end
end

class CallButton
    
    # ACCESSORS
    attr_accessor :floor, :direction, :column

    # INSTANCE VARIABLES
    def initialize(floor, direction, column)
        @floor = floor
        @direction = direction
        @column = column
        @is_toggled = false
        @is_emitting_light = false
    end

    # METHODS
    def press
        
        puts "\nELEVATOR REQUEST"
        puts "Someone is on floor #{@floor}. The person decides to call an elevator."

        @is_toggled = true
        control_light()

        chosen_elevator = choose_elevator()
        send_request(chosen_elevator)

        @is_toggled = false
        control_light()
      
        return chosen_elevator 
    end

    # Light up a pressed button
    def control_light
        if @is_toggled
            @is_emitting_light = true
        else
            @is_emitting_liht = false
        end
    end

    # Choose which elevator to call
    def choose_elevator
        elevator_scores = []

        for elevator in @column.elevator_list do
            
            # Initialize score to 0
            score = 0
            floor_difference = elevator.current_floor - @floor

            # Prevents use of any offline/under-maintenance elevators
            if  elevator.status != "online"
                score = -1
                elevator_scores.append(score)
            else

                # Bonify score based on difference in floors
                if floor_difference == 0
                    score += 5000
                else
                    score += 5000/(floor_difference.abs() + 1)
                end
                
                # Bonify score based on direction (highest priority)
                if elevator.movement != "idle"
                    if floor_difference >= 0 and @direction == "down" and elevator.movement == "down"
                        
                        # Paths are crossed going down, therefore favor this elevator
                        score += 10000
                    
                    elsif floor_difference <= 0 and @direction == "up" and elevator.movement == "up"

                        # Paths are crossed going down, therefore favor this elevator
                        score += 10000
                    
                    else
                        
                        # Paths are not crossed, therefore try avoiding the use of this elevator
                        score = 0
            
                        # Give redemption points, in worst case scenario where all elevators never cross paths
                        next_floor_difference = elevator.next_floor - @floor
                        if next_floor_difference == 0
                            score += 500
                        else
                            score += 500/(next_floor_difference.abs() + 1)
                        end
                    end
                end

                # Bonify score on request queue size (the smaller number of pre-existing requests, the faster therefore the better)
                if elevator.requests_queue.length() <= 3
                    score += 1000
                elsif elevator.requests_queue.length() <= 7
                    score += 250
                end
                
                # Send total score of elevator to the scores list
                elevator_scores.append(score)
            end
        end

        # Get value of highest score
        highest_score = -1
        for score in elevator_scores do
            if (score > highest_score)
                highest_score = score
            end
        end
        
        # Get elevator with the highest score (or nil if all elevators were offline
        chosen_elevator = nil
        if (highest_score > -1)
            chosen_elevator = @column.elevator_list[elevator_scores.find_index(highest_score)]
        end

        puts "Chosen elevator's ID: #{chosen_elevator.id}"
        return chosen_elevator
    end

    # Send new request to chosen elevator
    def send_request(elevator)
        request = Request.new(@floor, @direction)
        elevator.requests_queue.append(request)
    end
end

class Request

    # ACCESSORS
    attr_accessor :floor, :direction

    # INSTANCE VARIABLES
    def initialize(floor, direction)
        @floor = floor
        @direction = direction
    end
end

#################################################################################################################################################
# TEST SCENARIOS
#################################################################################################################################################
#Variables
Battery.num_columns = 1
Column.num_elevators = 2
Elevator.num_floors = 10

# Instantiate the batteries, the columns, and the elevators
battery = Battery.new(1)
battery.create_column_list()
battery.monitor_system()


# Set placeholder for column used in test scenario
column = battery.column_list[0]

### SCENARIO 1 ###
def scenario1(column)
    puts "**********************************************************************************************************************************"
    puts "SCENARIO 1"
    puts "**********************************************************************************************************************************"

    column.elevator_list[0].change_properties(2, nil, "idle")
    column.elevator_list[1].change_properties(6, nil, "idle")

    chosen_elevator = column.request_elevator(3, "up")
    column.request_floor(chosen_elevator, 7)
end

### SCENARIO 2 ###
def scenario2(column)
    puts "\n\n"
    puts "**********************************************************************************************************************************"
    puts "SCENARIO 2"
    puts "**********************************************************************************************************************************"
    column.elevator_list[0].change_properties(10, nil, "idle")
    column.elevator_list[1].change_properties(3, nil, "idle")

    chosen_elevator = column.request_elevator(1, "up")
    column.request_floor(chosen_elevator, 6)

    puts "\n\n\n=== 2 MINUTES LATER ===\n\n"

    chosen_elevator = column.request_elevator(3, "up")
    column.request_floor(chosen_elevator, 5)

    puts "\n\n\n=== AFTER A BIT MORE TIME ===\n\n"

    chosen_elevator = column.request_elevator(9, "down")
    column.request_floor(chosen_elevator, 2)
end

### SCENARIO 3 ###
def scenario3(column)
    puts "\n\n"
    puts "**********************************************************************************************************************************"
    puts "SCENARIO 3"
    puts "**********************************************************************************************************************************"

    column.elevator_list[0].change_properties(10, nil, "idle")
    column.elevator_list[1].change_properties(3, 6, "up")

    chosen_elevator = column.request_elevator(3, "down")

    column.request_floor(chosen_elevator, 2)
    column.request_floor(column.elevator_list[1], 6)

    puts "\n\n\n=== 5 MINUTES LATER ===\n\n"

    chosen_elevator = column.request_elevator(10, "down")
    column.request_floor(chosen_elevator, 3)
end

scenario1(column)
scenario2(column)
scenario3(column)
