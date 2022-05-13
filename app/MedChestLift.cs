/* There are likely faster ways of doing much of the below with C# libraries, but for 
   the purposes of this exercise I'll try and do as much as possible from first principles */

class Init {
    // First, some global variables defining filepaths for the tester to change to suit their system!
    public static string data_source_filepath = "C:\\Projects\\AVAMAE application process\\Cloud Software Engineer Coding Exercise Data.csv";
    public static string debug_path = "C:\\Projects\\AVAMAE application process\\debug.txt";
    public static string output_path = "C:\\Projects\\AVAMAE application process\\lift algorithm output.csv";

    public static void Main() {
        tests();
        MedChestLift algorithm = new MedChestLift();
    }

    public static void tests() {
        /* Time to run some tests to make sure the various helper classes / methods I've run are working correctly
           It's likely there are much faster, perhaps obvious ways to do some of the stuff below but due to time pressure,
           I chose the simple, first-principles way over doing some research for some things since I'm not experienced in C# */
        object[] test_array = {0,1,2,3,4,5,6,7};

        /* Test all the "Utility" class functions,
           Starting with "subsetArray": returns a slice of an array */
        object[] test_result = Utility.subsetArray(test_array,1,test_array.Length);
        Console.Write(string.Join(",",test_result)); // expected 1,2,3,4,5,6,7
        Console.WriteLine(" - subsetArray Test passed? "+(string.Join(",",test_result) == "1,2,3,4,5,6,7").ToString());

        // Test "addArrays": adds together two arrays (e.g. appends one to the other)
        test_result = Utility.addArrays(test_array,test_array);
        Console.Write(string.Join(",",test_result)); // expected 0,1,2,3,4,5,6,7,0,1,2,3,4,5,6,7
        Console.WriteLine(" - addArrays Test passed? "+(string.Join(",",test_result) == "0,1,2,3,4,5,6,7,0,1,2,3,4,5,6,7").ToString());

        // Test "appendArray":
        test_result = Utility.appendArrays(test_array,8);
        Console.Write(string.Join(",",test_result)); // expected 0,1,2,3,4,5,6,7,8
        Console.WriteLine(" - appendArrays Test passed? "+(string.Join(",",test_result) == "0,1,2,3,4,5,6,7,8").ToString());

        object[] second_test_array = {};  // edge case that I suspected may have been causing a bug
        test_result = Utility.appendArrays(second_test_array,8);
        Console.Write(string.Join(",",test_result)); // expected 8
        Console.WriteLine(" - appendArrays Test passed? "+(string.Join(",",test_result) == "8").ToString());

        // Test "arrayAppendArray":
        object[][] test_2D_array = {test_array,test_array};
        test_2D_array = Utility.arrayAppendArrays(test_2D_array,test_array);
        Console.Write("arrayAppendArrays test: ");
        foreach(object[] item in test_2D_array) {
            Console.Write((string.Join(",",item) == "0,1,2,3,4,5,6,7")+", ");
        }
        Console.WriteLine("Correct length? "+(test_2D_array.Length == 3).ToString());

        // Test "rotateArray": brings all elements down one index and sends the 0th element to the back
        test_result = Utility.rotateArray(test_array);
        Console.Write(string.Join(",",test_result)); // expected 1,2,3,4,5,6,7,0
        Console.WriteLine(" - rotateArray Test passed? "+(string.Join(",",test_result) == "1,2,3,4,5,6,7,0").ToString());

        // Now the big one - test "getAllPossibleListCombinations": returns a 2D array containing all possible ways of ordering an array
        test_array = new object[] {0,1,2,3,4,5,6,7};
        object[][] results = Utility.getAllPossibleListCombinations(test_array);
        Console.Write(results.Length); // expected 8!, or 40,320
        Console.WriteLine(" - getAllPossibleListCominbations Test passed? "+(results.Length == 40320).ToString());

        /* -------------------------------------------------------
           Testing the lift and it's functions!
           ------------------------------------------------------- */
        LiftObject test_lift = new LiftObject(5);
        object[] test_schedule = {"1_3C","2_5D","3_8C","4_1D"}; // (ID)_(Floor)("C" for calling the lift to a floor, "D" for passenger already in lift requesting to be dropped off )
        test_lift.passengers = new Dictionary<short, LiftPassenger>();
        foreach(object item in test_schedule) {
            short ID = LiftObject.schedID(item); // returns the ID of the passenger from the "schedule" using above convention as a short
            short floor = LiftObject.schedFloor(item); // returns the destination floor for a lift call from the "schedule" using above convention as a short
            test_lift.passengers.Add(ID,new LiftPassenger(floor,1,0)); // passenger stores all passenger data as a LiftPassenger object
        }

        // Test "calcSchedulePF": returns the "PF" value, as described in the previous application stage, of a "schedule"
        int PF = test_lift.calcSchedulePF(test_schedule);
        Console.Write(PF); // Expect abs(5-3)*4 + abs(3-5)*3 + abs(8-5)*2 + abs(1-8) = 27
        Console.WriteLine(" - calcSchedulePF Test passed? "+(PF == 27).ToString());
        
        // Test "getScheduleThatMinimisesPF": loops over all schedules returned using "getAllPossibleCombinations" and finds the one with the lowest PF
        // "getScheduleThatMinimisesPF" needs a LiftObject as it takes previous PF scores of all passengers into account
        test_schedule = new object[] {"1_3C","2_5D","3_8C","4_1D","5_2D","6_9D","7_10C","8_6C","9_7D"};
        test_lift.lift_schedule = test_schedule;
        test_lift.curr_floor = 1;
        test_lift.passengers = new Dictionary<short, LiftPassenger>();
        foreach(object item in test_schedule) {
            short ID = LiftObject.schedID(item);
            short floor = LiftObject.schedFloor(item);
            test_lift.passengers.Add(ID,new LiftPassenger(floor,1,0));
        }
        object[] best_schedule_test = test_lift.getScheduleThatMinimisesPF();
        Console.WriteLine(string.Join(",",best_schedule_test)); // expect {"4_1D","5_2D","1_3C","2_5D","8_6C","9_7D","3_8C","6_9D","7_10C"}
        Console.WriteLine(" - getScheduleThatMinimisesPF Test passed? "+(string.Join(",",best_schedule_test) == "4_1D,5_2D,1_3C,2_5D,8_6C,9_7D,3_8C,6_9D,7_10C").ToString());
    }
}

class MedChestLift { 
    /* main class containing the algorithm that runs the lift
       Changing the below constants will change the lift's behaviour - the below are reasonable but feel free to experiment! */
    public static short max_individual_PF_travelling = 20; // max "PF"" an individual can spend in the lift before it takes them to their floor regardless of if it's the most efficient choice
    public static short max_individual_PF_waiting = 20; // max "PF"" an individual can spend waiting for lift before it picks them up regardless of if it's the most efficient choice
    public static short algorithm_start_floor = 5; // floor the lift starts on

    public MedChestLift() {
        CSVDataFrame CSV_dataframe = new CSVDataFrame(Init.data_source_filepath); // loads in the target data and puts it in a convenient object
        CSV_dataframe.printTable();
        LiftObject lift = new LiftObject(MedChestLift.algorithm_start_floor);
        this.mainAlgorithm(CSV_dataframe,lift);
    }

    public void mainAlgorithm(CSVDataFrame data, LiftObject lift) {
        /* main algorithm that will print several debug statements and output a CSV with the requested format */
        short time = 0;
        short curr_index = 0; // corresponds to the row of the CSV data; essentially represents the ID (-1) of the next person to call the lift
        
        Utility.clearFile(Init.debug_path); // creates an empty file for relevant info to be appended to as required
        Utility.clearFile(Init.output_path); // readies the output file to be filled as the algorithm runs
        string[] output_column_names = {"Time","People In Lift","At Floor","Call Queue"};
        CSVDataFrame output = new CSVDataFrame(output_column_names); // create the data frame for storing data eventually used to populate the output CSV 
        Utility.appendArrayToFile(output_column_names,Init.output_path); // appends an array as raw text to the end of file without modifying any existing lines in that file

        while(true) {

            // First, evaluate if the lift has reached a floor to drop off or pick up any passengers
            
            if(lift.lift_schedule.Length > 0) { // check if there is any floors in the lift's queue
                if(lift.curr_floor == LiftObject.schedFloor(lift.lift_schedule[0])) { // see if the lift has reached it's next scheduled stop
                    char type = LiftObject.schedType(lift.lift_schedule[0]); // determine if the next call in the queue is to pick someone up ("C") or drop them off ("D")
                    if(type == char.Parse("D")) {
                        Utility.appendStringToFile("Dropped off passenger "+LiftObject.schedID(lift.lift_schedule[0]).ToString(),Init.debug_path);
                        lift.passengers[LiftObject.schedID(lift.lift_schedule[0])].journey_completed = true;
                        lift.lift_schedule = Utility.subsetArray(lift.lift_schedule, 1, lift.lift_schedule.Length); // when a passenger is dropped off they should be removed from the lift's schedule
                    } else {
                        Utility.appendStringToFile("Picked up passenger "+LiftObject.schedID(lift.lift_schedule[0]).ToString(),Init.debug_path);
                        lift.lift_schedule[0] = LiftObject.schedID(lift.lift_schedule[0]).ToString()+"_"+data.column("Going to Floor")[LiftObject.schedID(lift.lift_schedule[0])-1].ToString()+"D"; // when a passenger is picked up we maintain the (ID)_(Floor)("D"/"C") convention, but need to change origin floor to destination floor and "C" to "D"
                        lift.passengers[LiftObject.schedID(lift.lift_schedule[0])].in_lift = true; // so we know to start counting their "travelling_PF" and not "waiting_PF" since we chose to place constraints on both separately
                        lift.lift_schedule = lift.getScheduleThatMinimisesPF(); // find the new most efficient schedule
                    }
                }
            }

            // If we know there are still calls remaining in our data set, we listen for new calls at each unit of time

            if(curr_index < data.length()) {
                while((short) data.column("Time")[curr_index] == time) {
                    Utility.appendStringToFile("New call from passenger "+data.column("Person ID")[curr_index].ToString(),Init.debug_path);
                    lift.lift_schedule = Utility.appendArrays(lift.lift_schedule,data.column("Person ID")[curr_index].ToString()+"_"+data.column("At Floor")[curr_index].ToString()+"C"); // register the new call in the lift's schedule
                    LiftPassenger passenger_object = new LiftPassenger((short) data.column("At Floor")[curr_index],(short) data.column("Going to Floor")[curr_index],time); 
                    lift.passengers.Add((short) data.column("Person ID")[curr_index],passenger_object); // create the passenger object and add it to our dictionary with the ID as the key, so we can access more detail about the passenger without complicating our lift schedule array
                    lift.lift_schedule = lift.getScheduleThatMinimisesPF(); // recalc the lift schedule now that we've picked up the new passenger
                    curr_index++;
                    if(curr_index >= data.length()) {
                        break;
                    }
                }
            } else {
                // Now we know we're at a stage where there will be no new passengers and lift should process all remaining calls and then shut down
                Utility.appendStringToFile("All calls done - now processing remaining passengers...",Init.debug_path);
            }

            short next_floor = lift.curr_floor;
            if(lift.lift_schedule.Length > 0) {
                next_floor = LiftObject.schedFloor(lift.lift_schedule[0]); // if there exists calls in the lifts queue, set it's aim to the next one
            }
            /* ------------------------- DISCUSSION ---------------------------
               There was some ambiguity in the AVAMAE instructions; I'm told to "assume the lift takes 10 seconds to move from one floor to the next"
               This could be interpreted as the lift takes 10 seconds to move from it's current floor to the floor corresponding to the next scheduled call,
               or it could mean the lift takes 10 seconds to travel the distance of one single floor.
               I initially thought it was the latter but it became clear when writing that this would be unrealistically slow so I opted to finish the code using
               the former; however, I left the original code and variables in as a demonstration for how it might work.
               ---------------------------------------------------------------- */

            /* UNUSED - as part of "lift moves one single floor in 10 seconds" old system

            if(lift.curr_floor > next_floor) {
                lift.curr_direction = -1;
            } else if(lift.curr_floor < next_floor) {
                lift.curr_direction = 1;
            } else {
                lift.curr_direction = 0;
            }*/

            time++;
            foreach(KeyValuePair<short, LiftPassenger> entry in lift.passengers) {
                if(!entry.Value.journey_completed) {
                    entry.Value.total_time++;
                }
            }
            /* UNUSED - as part of "lift moves one single floor in 10 seconds" old system   

            if(time % LiftObject.time_per_floor == 0) {
                lift.curr_floor += lift.curr_direction;
                Console.WriteLine("Lift moved to floor "+lift.curr_floor.ToString());
                foreach(KeyValuePair<short, LiftPassenger> entry in lift.passengers) {
                    if(entry.Value.in_lift & !entry.Value.journey_completed) {
                        entry.Value.travelling_PF++;
                    } else if(!entry.Value.journey_completed) {
                        entry.Value.waiting_PF++;
                    }
                }
            }*/

            if(time % LiftObject.time_per_floor == 0) { // The lift taking 10 seconds to move from current to target floor is equivalent to it moving once every 10 seconds
                Utility.appendStringToFile("Lift moved to floor "+next_floor.ToString(),Init.debug_path);
                short pf_gain = (short) Math.Abs(lift.curr_floor-next_floor); // We keep track of total PF waited and travelled for each passenger so need to obviously calculate this whenever the lift runs
                lift.curr_floor = next_floor; // working under the "lift magically teleports to it's desired floor every 10 seconds" model
                Utility.appendStringToFile("PF gain: "+pf_gain.ToString(),Init.debug_path);

                foreach(KeyValuePair<short, LiftPassenger> entry in lift.passengers) { // Calculate if passenger is due to be picked up or dropped off and then update their PF_waiting/travelled count
                    if(entry.Value.in_lift & !entry.Value.journey_completed) {
                        entry.Value.travelling_PF += pf_gain;
                    } else if(!entry.Value.journey_completed) {
                        entry.Value.waiting_PF += pf_gain;
                    }
                }

                // update output CSV whenever lift stops at a floor
                output.appendToColumn("Time", (object) time);
                object[] people_in_lift = {};
                foreach(KeyValuePair<short, LiftPassenger> entry in lift.passengers) {
                    if(entry.Value.in_lift & !entry.Value.journey_completed) {
                        people_in_lift = Utility.appendArrays(people_in_lift,(object) entry.Key);
                    }
                }
                // Since the actual columns should be seperated by commas, having the individual elements as comma-separated lists might be tricky
                // Therefore, we can use another delimitter we haven't yet used in any of our lists, e.g. a hyphen
                output.appendToColumn("People In Lift", (object) string.Join(",",people_in_lift).Replace(",","-"));
                output.appendToColumn("At Floor",(object) lift.curr_floor);
                output.appendToColumn("Call Queue",(object) string.Join(",",lift.lift_schedule).Replace(",","-"));
            }
            
            Console.WriteLine(time); // Nice to leave this as a console output as well so we can see the script isn't stuck
            Utility.appendStringToFile(time.ToString(),Init.debug_path);
            if(curr_index >= data.length() & lift.lift_schedule.Length == 0) {
                Utility.appendStringToFile("Lift sim done! Last passenger dropped off at "+time.ToString(),Init.debug_path);
                foreach(KeyValuePair<short, LiftPassenger> entry in lift.passengers) {
                    Utility.appendStringToFile(entry.Key.ToString()+": "+entry.Value.debugString(),Init.debug_path);
                }
                output.printTable();
                output.saveCSV(Init.output_path);
                break;
            }
        }
        // Sleep for a while so the user can read the output if running purely the console application
        Console.WriteLine("Simulation complete!");
        Console.WriteLine("Output CSV has been saved - can now safely close window (will auto-close in 60 secs)");
        Thread.Sleep(60000);
    }
}

class LiftObject {
    public static short num_floors = 10;
    public static short max_capacity = 8;
    public static short time_per_floor = 10;
    public Dictionary<short, LiftPassenger> passengers = new Dictionary<short, LiftPassenger>();
    public object[] lift_schedule = {}; 
    public short curr_floor;
    public short curr_direction = 0; // 1 is up, -1 is down, 0 is no movement (e.g. lift empty)

    public LiftObject(short start_floor) {
        this.curr_floor = start_floor;
    }

    public object[] getScheduleThatMinimisesPF() {
        /* Discussed in more detail in the documentation; essentially takes the "lift_schedule" array and uses
           the "getAllPossibleListCombinations" function to get every single possible order of floors the lift can call at
           Then checks each one satisfies any criteria e.g. max capacity, and individual passenger PF's, and 
           find the schedule with the lowest PF value and returns it */
        object[][] schedules = Utility.getAllPossibleListCombinations(this.lift_schedule);
        int min_PF = -1; // Could arbitrarily set this to "9999" or something but I like to avoid doing this where possible incase I don't consider that a case like this might bite somehow
        int best_index = -1;
        for(int i = 0; i < schedules.Length; i++) {
            int PF = calcSchedulePF(schedules[i]);
            if((min_PF < 0 | min_PF > PF) & PF >= 0) {
                // First need to check it won't ruin capacity of lift
                short existing_people_in_lift = 0;
                bool allowed = true;
                foreach(KeyValuePair<short,LiftPassenger> entry in this.passengers) { // need to find how many people are currently in lift
                    if(entry.Value.in_lift & !entry.Value.journey_completed) {
                        existing_people_in_lift++;
                    }
                }
                short lift_future_max_cap = existing_people_in_lift;
                for(int j = 0; j < schedules[i].Length; j++) { // checks at no point in the lift's future schedule will it go over maximum capacity
                    char type = LiftObject.schedType(schedules[i][j]);
                    if(type == char.Parse("C")) {
                        lift_future_max_cap++;
                    } else {
                        lift_future_max_cap--;
                    }
                    // rejects the schedule if it doesn't meet all criteria. The convenience of calcuating every possible schedule means there will (almost?) always be a valid solution
                    if(lift_future_max_cap > LiftObject.max_capacity) {
                        allowed = false;
                        break;
                    }
                }
                if(allowed) { // if a schedule is found that is more efficient than the current most efficient and doesn't break any criteria, remember it
                    best_index = i;
                    min_PF = PF;
                }
            }
        }
        return(schedules[best_index]);
    }

    public int calcSchedulePF(object[] schedule) {
        /* Schedules are arrays of objects where each object is a string of "ID"_"num of floor"+"D/C" 
           where D is code for drop-off and C is code for call */
        int PF = 0;
        int curr_floor_sim = this.curr_floor;
        int[] passenger_PFs = new int[schedule.Length]; // need to store individual passenger PF's as well as total PF if we wish to impose extra criteria on an individual's maximum waiting time
        bool allowed = true;
        bool break_loop = false;
        for(int i = 0; i < schedule.Length; i++) { // hopefully this loop is self explanatory - calc number of floors between current and next, update PF value, then move to that floor and repeat
            short next_floor = LiftObject.schedFloor(schedule[i]);
            PF += Math.Abs(curr_floor_sim - next_floor) * (schedule.Length - i);
            curr_floor_sim = next_floor;
            for(int j = i; j < schedule.Length; j++) { // update each individual's PF value, noting that a few people would have hypothetically got off the lift at this point
                passenger_PFs[j] += Math.Abs(curr_floor_sim - next_floor);
                char type = LiftObject.schedType(schedule[j]);
                short ID = LiftObject.schedID(schedule[j]);
                short curr_PF;
                if(type == char.Parse("C")) {
                    curr_PF = this.passengers[ID].waiting_PF;
                } else {
                    curr_PF = this.passengers[ID].travelling_PF;
                }
                if(type == char.Parse("C") & passenger_PFs[j]+curr_PF > MedChestLift.max_individual_PF_waiting | 
                   type == char.Parse("D") & passenger_PFs[j]+curr_PF > MedChestLift.max_individual_PF_travelling) {
                       allowed = false;
                       break_loop = true;
                       break;
                   }
            }
            if(break_loop) {
                break;
            }
        }
        if(allowed) {
            return(PF);
        } else {
            return(-1); // I currently don't have any way of handling the exception "-1" case because I know it won't arise for me for the dataset I'm given - perhaps a talking point for later
        }
    }

    public static short schedID(object sched) {
        // Simple method to return the ID of a passenger, given their string entry into "schedules" or form (ID)_(floor)(D/C)
        return(Int16.Parse(sched.ToString().Split("_")[0]));
    }

    public static short schedFloor(object sched) {
        // Simple method to return the ID of a passenger, given their string entry into "schedules" or form (ID)_(floor)(D/C)
        string[] split_schedule = sched.ToString().Split("_");
        return(Int16.Parse(split_schedule[1].ToString().Substring(0,split_schedule[1].ToString().Length-1)));
    }

    public static char schedType(object sched) {
        // Simple method to return the ID of a passenger, given their string entry into "schedules" or form (ID)_(floor)(D/C)
        string[] split_schedule = sched.ToString().Split("_");
        return(char.Parse(split_schedule[1].Substring(split_schedule[1].Length-1,1)));
    }
}

class LiftPassenger {
    // Class to store additional useful information about passengers in or waiting for the lift
    public short origin_floor;
    public short destination_floor;
    public short waiting_PF;
    public short travelling_PF;
    public short origin_time;
    public short total_time;
    public bool in_lift = false;
    public bool journey_completed = false;

    public LiftPassenger(short origin, short destination, short time) {
        this.origin_floor = origin;
        this.destination_floor = destination;
        this.origin_time = time;
    }

    public string debugString() {
        // Prints a helpful summary of the properties of an object of this class
        string debug = "From floor "+this.origin_floor.ToString()+"; to floor "+this.destination_floor.ToString()+"; waiting pf "+this.waiting_PF.ToString()+"; travelling pf "+this.travelling_PF.ToString()+"; origin time "+this.origin_time.ToString()+"; total time travelled "+this.total_time.ToString()+"; in lift "+in_lift.ToString()+"; journey completed "+journey_completed.ToString();
        return(debug);
    }
}

class CSVDataFrame {
    // A "Data frame"-esque object used to provide CSV-like data in a convenient format with a few helper methods
    public string[] column_names = {};
    public object[][] column_data = {};

    public CSVDataFrame(string filepath) {
        /* Overloaded - if you want to populate a CSVDataFrame object with an existing CSV file you can pass the filename
           and it will populate the object automatically */
        constructDataFrameFromCSV(filepath);
    }

    public CSVDataFrame(string[] new_column_names) {
        /* Overloaded - if you want to create your own data frame, not from any existing CSV file, then you
           can pass a string[] of the desired column names */
        constructDataFrameManually(new_column_names);
    }

    public void constructDataFrameFromCSV(string filepath) {
        // Fills the data frame from existing data in a CSV file
        string[] CSV_data = readCSV(filepath);
        constructShortDataFrame(CSV_data);
    }

    public void constructDataFrameManually(string[] new_column_names) {
        // Sets up a base, empty data frame with the desired column names for later population
        this.column_names = new_column_names;
        for(int i = 0; i < new_column_names.Length; i++) {
            object[] empty_array = {};
            this.column_data = Utility.arrayAppendArrays(this.column_data,empty_array);
        }
    }

    public static string[] readCSV(string filepath) {
        return(System.IO.File.ReadAllLines(filepath));
    }

    public void constructShortDataFrame(string[] CSV_data) {
        /* Assumes all data is of the primitive data type "short", e.g. an integer less than 32,767 in magnitude
           This method is bespoke in the sense that it works only for a subset of cases, but we know it will for the AVAMAE lift problem */
        this.column_names = CSV_data[0].Split(",");
        this.column_data = new object[this.column_names.Length][];
        for(int i = 0; i < column_data.Length; i++) {
                this.column_data[i] = new object[CSV_data.Length-1]; // makes empty columns to be filled with the CSV data at a later date
            }
        for(int i = 1; i < CSV_data.Length; i++) {
            string[] temp_split = CSV_data[i].Split(",");
            for(int j = 0; j < temp_split.Length; j++) {
                this.column_data[j][i-1] = Int16.Parse(temp_split[j]); // Populate the columns, I wrote this method knowing I would only be dealing with 16-bit integers
            }
        }
    }

    public void printTable() {
        // used in the debugging process to ensure formatting was correct and any sense checks could be done
        for(int i = 0; i < this.column_names.Length; i++) {
            System.Console.Write(this.column_names[i]);
            System.Console.Write(", ");
        }
        System.Console.Write("\n");
        for(int i = 0; i < this.column_data[0].Length; i++) {
            for(int j = 0; j < this.column_data.Length; j++) {
                System.Console.Write(this.column_data[j][i]);
                System.Console.Write(", ");
            }
            System.Console.Write("\n");
        }
    }

    public object[] column(string name) {
        // Conveniently returns a column of the data frame given the column's name
        return(this.column_data[Array.IndexOf(this.column_names,name)]);
    }

    public void appendToColumn(string column_name, object item) {
        // Conveniently appends an object to a column
        this.column_data[Array.IndexOf(this.column_names,column_name)] = Utility.appendArrays(this.column_data[Array.IndexOf(this.column_names,column_name)],item);
    }

    public int length() {
        // Because I'm lazy and didn't want to retrieve a column with the "column" function and then find it's length every time
        return(this.column_data[0].Length);
    }

    public void saveCSV(string path) {
        // Saves the data frame in the CSV format
        string[] lines = {};
        object[] line = {};
        for(int i = 0; i < this.column_names.Length; i++) {
            line = Utility.appendArrays(line,this.column_names[i]);
        }
        lines = (string[]) Utility.appendArrays(lines,string.Join(",",line));
        for(int i = 0; i < this.length(); i++) {
            line = new object[] {};
            for(int j = 0; j < this.column_names.Length; j++) {
                line = Utility.appendArrays(line,this.column(this.column_names[j])[i]);
            }
            lines = Utility.appendArrays(lines,string.Join(",",line));
        }
        System.IO.File.WriteAllLines(path, lines.ToList());
    }
}

class Utility {
    // Generic helper class to provide various C# methods since I'm trying not to rely on libraries
    public Utility() {
    }

    public static object[] subsetArray(object[] array, int from, int to) {
        // Returns a slice of the array from it's "from" element to it's "to" element, including the "from" element but excluding the "to" element
        object[] new_array = new object[to-from];
        for(int i = from; i < to; i++) {
            new_array[i-from] = array[i];
        }
        return(new_array);
    }

    public static object[] addArrays(object[] array1, object[] array2) {
        // Adds two arrays together - into a 1D array not a 2D one, e.g. every element of array 2 is appended to array 1
        object[] new_array = new object[array1.Length+array2.Length];
        for(int i = 0; i < array1.Length; i++) {
            new_array[i] = array1[i];
        }
        for(int i = 0; i < array2.Length; i++) {
            new_array[i+array1.Length] = array2[i];
        }
        return(new_array);
    }

    public static object[] appendArrays(object[] array, object element) {
        // Appends an object to an array. Potentially misleading name but I got used to writing it so never changed it
        object[] new_array = new object[array.Length + 1];
        for(int i = 0; i < array.Length; i++) {
            new_array[i] = array[i];
        }
        new_array[new_array.Length-1] = element;
        return(new_array);
    }

    // Need to overload so we can add strings to arrays to save them in text readable format
    public static string[] appendArrays(string[] array, string element) {
        // Appends a string to an array. Potentially misleading name but I got used to writing it so never changed it
        string[] new_array = new string[array.Length + 1];
        for(int i = 0; i < array.Length; i++) {
            new_array[i] = array[i];
        }
        new_array[new_array.Length-1] = element;
        return(new_array);
    }

    public static object[][] arrayAppendArrays(object[][] array, object[] subarray) {
        // appends a 1D array to a 2D array
        object[][] new_array = new object[array.Length+1][];
        for(int i = 0; i < array.Length; i++) {
            new_array[i] = array[i];
        }
        new_array[new_array.Length-1] = subarray;
        return(new_array);
    }

    public static object[] rotateArray(object[] array) {
        /* rotates the array - that is, moves every element down one index and moves the 0th index to the back
           seemingly no quick way to do this in C# - in Python it can be done in one line, "rotated_array = array[1:len(array)] + array[0:1]" */
        object[] rotated_array = Utility.subsetArray(array,1,array.Length);
        rotated_array = Utility.appendArrays(rotated_array,array[0]);
        return(rotated_array);
    }

    public static object[][] getAllPossibleListCombinations(object[] array) {
        /* Works with the recursive function "calcListCombs" to find every possible way of ordering the
           elements in an input array, and returning a 2D array of all elements.
           ----------------------- WARNING! -----------------------------------
           Extremely computationally intense - will cause awful performance for arrays of length 10 or above */
        object[] bolt = {};
        List<object[]> combs = new List<object[]>();
        combs = Utility.calcListCombs(array,combs,bolt); // Recursive function where the magic happens!
        List<string> string_results = new List<string>();
        foreach(object[] item in combs) { // calcListCombs doesn't filter out duplicates, but it's hard to find duplicate arrays in a 2D array
            string_results.Add(string.Join(",",item)); // easier to turn everything into a string, find and erase duplicate strings, and then turn back into an array
        }
        string_results = string_results.Distinct().ToList();
        object[][] combs_final = new object[string_results.Count][];
        for(int i = 0; i < string_results.Count; i++) {
            combs_final[i] = string_results[i].Split(",");
        }
        return(combs_final);
    }

    public static List<object[]> calcListCombs(object[] source_list, List<object[]> combs, object[] bolt) {
        /* Recursive function that calculates every possible way of ordering an array "source_list";
           Not intended to be called directly by the user due to it's novel method and complexity. Use "getAllPossibleListCombinations" in practise
           combs: the 2D array to store all the calculated combinations; needs to be returned from and passed to the function
           bolt: Method works by "rotating" the array, and all subsets of the array, so when taking a subset of the array need to remember the bit you "forgot" to give the overall unique combination
           
           Exact way this works is to complicated to describe in a docstring so will be explained in detail in the documentaion */

        for(int i = 0; i < source_list.Length; i++) {
            source_list = Utility.rotateArray(source_list);
            combs.Add(Utility.addArrays(bolt,source_list));
            if(source_list.Length > 1) {
                object[] new_source_list = Utility.subsetArray(source_list,1,source_list.Length);
                object[] new_bolt = Utility.appendArrays(bolt,source_list[0]);
                combs = Utility.calcListCombs(new_source_list,combs,new_bolt);
            }
        }
        return(combs);
    }

    public static void appendArrayToFile(object[] array, string path) {
        // Appends an array to a file as a raw text string
        System.IO.File.AppendAllText(path,string.Join(",",array)+"\n");
    }

    public static void appendStringToFile(string data, string path) {
        // Appends a string to a file; could perhaps be an overload of the above function but I found it convenient to keep it separate
        System.IO.File.AppendAllText(path,data+"\n");
    }

    public static void clearFile(string path) {
        /* For debugging and other uses it's convenient to append results a file as we calculate them
           We want to start with an empty file everytime we run the script, but deleting it and later trying
           to access it might return an error so it's better to create a new, empty file before we attempt to append to it */
        System.IO.File.WriteAllLines(path, new string[0]);
    }
}