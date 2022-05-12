﻿/* Since there's no guarantee The MedicineChest will have my C# libaries,
   I'll try and do everything from more or less first principles using
   Very standard libraries. We could use CsvHelper or a similar lib to
   read the CSVs but I'll do it myself using the base system libs. */

class Init {
    public static void Main() {
        tests();
        MedChestLift algorithm = new MedChestLift();
    }

    public static void tests() {
        /* First, some tests! */
        object[] test_array = {0,1,2,3,4,5,6,7};

        /* Test all the "Utility" class functions,
           Starting with "subsetArray": */
        object[] test_result = Utility.subsetArray(test_array,1,test_array.Length);
        Console.Write(string.Join(",",test_result)); // expected 1,2,3,4,5,6,7
        Console.WriteLine(" - subsetArray Test passed? "+(string.Join(",",test_result) == "1,2,3,4,5,6,7").ToString());

        // Test "addArrays":
        test_result = Utility.addArrays(test_array,test_array);
        Console.Write(string.Join(",",test_result)); // expected 0,1,2,3,4,5,6,7,0,1,2,3,4,5,6,7
        Console.WriteLine(" - addArrays Test passed? "+(string.Join(",",test_result) == "0,1,2,3,4,5,6,7,0,1,2,3,4,5,6,7").ToString());

        // Test "appendArray":
        test_result = Utility.appendArrays(test_array,8);
        Console.Write(string.Join(",",test_result)); // expected 0,1,2,3,4,5,6,7,8
        Console.WriteLine(" - appendArrays Test passed? "+(string.Join(",",test_result) == "0,1,2,3,4,5,6,7,8").ToString());

        object[] second_test_array = {};
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

        // Test "rotateArray":
        test_result = Utility.rotateArray(test_array);
        Console.Write(string.Join(",",test_result)); // expected 1,2,3,4,5,6,7,0
        Console.WriteLine(" - rotateArray Test passed? "+(string.Join(",",test_result) == "1,2,3,4,5,6,7,0").ToString());

        // Now the big one - test "getAllPossibleListCombinations":
        test_array = new object[] {0,1,2,3,4,5,6,7};
        object[][] results = Utility.getAllPossibleListCombinations(test_array);
        Console.Write(results.Length); // expected 8!, or 40,320
        Console.WriteLine(" - getAllPossibleListCominbations Test passed? "+(results.Length == 40320).ToString());

        /* -------------------------------------------------------
           Testing the lift and it's functions!
           ------------------------------------------------------- */
        LiftObject test_lift = new LiftObject(5);
        object[] test_schedule = {"1_3C","2_5D","3_8C","4_1D"};
        test_lift.passengers = new Dictionary<short, LiftPassenger>();
        foreach(object item in test_schedule) {
            short ID = LiftObject.schedID(item);
            short floor = LiftObject.schedFloor(item);
            test_lift.passengers.Add(ID,new LiftPassenger(floor,1,0));
        }

        // Test "calcSchedulePF":
        int PF = test_lift.calcSchedulePF(test_schedule);
        Console.Write(PF); // Expect abs(5-3)*4 + abs(3-5)*3 + abs(8-5)*2 + abs(1-8) = 27
        Console.WriteLine(" - calcSchedulePF Test passed? "+(PF == 27).ToString());
        
        // Test "getScheduleThatMinimisesPF":
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
    public static short max_individual_PF_travelling = 20;
    public static short max_individual_PF_waiting = 20;
    public short algorithm_start_floor = 5;
    private static string data_source_filepath = "C:\\Projects\\AVAMAE application process\\Cloud Software Engineer Coding Exercise Data.csv";

    public MedChestLift() {
        CSVDataFrame CSV_dataframe = new CSVDataFrame(data_source_filepath);
        CSV_dataframe.printTable();
        LiftObject lift = new LiftObject(this.algorithm_start_floor);
        this.mainAlgorithm(CSV_dataframe,lift);
    }

    public void mainAlgorithm(CSVDataFrame data, LiftObject lift) {
        /* data has columns "Person ID", "At Floor", "Going to Floor", "Time" */
        short time = 0;
        short curr_index = 0;
        object[] schedule = {};
        string output_path = "C:\\Projects\\AVAMAE application process\\lift algorithm output.csv";
        Utility.clearFile(output_path);
        object[] output_column_names = {"Time","People In Lift","At Floor","Call Queue"};
        Utility.appendArrayToFile(output_column_names,output_path);

        while(curr_index < data.length()) {

            List<short> delete_index = new List<short>(); // to store which elements we need to delete from schedule if a passenger is dropped off; can't delete during the loop as will interfere with end criteria of "for" loop
            for(short i = 0; i < schedule.Length; i++) {
                short floor = LiftObject.schedFloor(schedule[i]);
                short ID = LiftObject.schedID(schedule[i]);

                if(floor == lift.curr_floor) {
                    char type = LiftObject.schedType(schedule[i]);

                    if(type == char.Parse("C")) {
                        short going_to_floor = (short) data.column("Going to Floor")[ID];
                        schedule[i] = ID.ToString()+"_"+going_to_floor.ToString()+"D";
                        schedule = lift.getScheduleThatMinimisesPF();
                        lift.lift_schedule = schedule;
                        Console.WriteLine("Picked up passenger "+ID.ToString()+" at time "+time.ToString()+" from floor "+floor.ToString());

                    } else {
                        delete_index.Add(i);
                        Console.WriteLine("Dropped off passenger "+ID.ToString()+" at time "+time.ToString()+" at floor "+floor.ToString());
                    }
                }
            }

            object[] new_schedule = {};
            for(short i = 0; i < schedule.Length; i++) {
                if(!delete_index.Contains(i)) {
                    Utility.appendArrays(new_schedule,schedule[i]);
                }
            }

            schedule = new_schedule;
            lift.lift_schedule = schedule;

            if((short) data.column("Time")[curr_index] == time) {
                Console.WriteLine("New call from passenger "+data.column("Person ID")[curr_index].ToString()+" at time "+time.ToString()+" from floor "+data.column("At Floor")[curr_index].ToString());
                schedule = Utility.appendArrays(schedule,data.column("Person ID")[curr_index].ToString()+"_"+data.column("At Floor")[curr_index].ToString()+"C");
                LiftPassenger passenger_object = new LiftPassenger((short) data.column("At Floor")[curr_index],(short) data.column("Going to Floor")[curr_index],time);
                lift.passengers.Add((short) data.column("Person ID")[curr_index],passenger_object);
                lift.lift_schedule = schedule;
                schedule = lift.getScheduleThatMinimisesPF();
                curr_index++;
            }
            
            short next_floor = lift.curr_floor;
            if(schedule.Length > 0) {
                next_floor = LiftObject.schedFloor(schedule[0]);
            }

            if(lift.curr_floor > next_floor) {
                lift.curr_direction = -1;
            } else {
                lift.curr_direction = 1;
            }

            time++;
            foreach(KeyValuePair<short, LiftPassenger> entry in lift.passengers) {
                if(!entry.Value.journey_completed) {
                    entry.Value.total_time++;
                }
            }
            if(time % LiftObject.time_per_floor == 0) {
                lift.curr_floor = (short) Math.Max(Math.Min(lift.curr_floor + lift.curr_direction,LiftObject.num_floors),1);
                foreach(KeyValuePair<short, LiftPassenger> entry in lift.passengers) {
                    if(entry.Value.in_lift & !entry.Value.journey_completed) {
                        entry.Value.travelling_PF++;
                    } else if(!entry.Value.journey_completed) {
                        entry.Value.waiting_PF++;
                    }
             }
            }
        }
        Console.WriteLine(string.Join(",",schedule));
        foreach(KeyValuePair<short, LiftPassenger> entry in lift.passengers)
        {  
            Console.Write(entry.Key.ToString()+": "+entry.Value.debugString());
        }
    }
}

class LiftObject {
    public static short num_floors = 10;
    public static short max_capacity = 8;
    public static short time_per_floor = 10;
    public Dictionary<short, LiftPassenger> passengers = new Dictionary<short, LiftPassenger>();
    public object[] lift_schedule = {}; // we declare this as a string so we can store both floor and type of visit: "(number of floor)(drop off or pick up)" - e.g. "1D" or "2P"
    public short curr_floor;
    public short curr_direction = 0; // 1 is up, -1 is down, 0 is no movement (e.g. lift empty)

    public LiftObject(short start_floor) {
        this.curr_floor = start_floor;
    }

    public object[] getScheduleThatMinimisesPF() {
        object[][] schedules = Utility.getAllPossibleListCombinations(this.lift_schedule);
        int min_PF = -1; // Could arbitrarily set this to "9999" or something but I like to avoid doing this where possible incase I don't consider that a case like this might bite somehow
        int best_index = -1;
        for(int i = 0; i < schedules.Length; i++) {
            int PF = calcSchedulePF(schedules[i]);
            if((min_PF < 0 | min_PF > PF) & PF >= 0) {
                best_index = i;
                min_PF = PF;
            }
        }
        return(schedules[best_index]);
    }

    public int calcSchedulePF(object[] schedule) {
        /* Schedules are arrays of objects where each object is a string of "ID"_"num of floor"+"D/C" 
           where D is code for drop-off and C is code for call */
        int PF = 0;
        int curr_floor_sim = this.curr_floor;
        int[] passenger_PFs = new int[schedule.Length];
        bool allowed = true;
        bool break_loop = false;
        for(int i = 0; i < schedule.Length; i++) {
            short next_floor = LiftObject.schedFloor(schedule[i]);
            PF += Math.Abs(curr_floor_sim - next_floor) * (schedule.Length - i);
            curr_floor_sim = next_floor;
            for(int j = i; j < schedule.Length; j++) {
                passenger_PFs[j] += Math.Abs(curr_floor_sim - next_floor); // need to keep a record of each individual's PF to ensure nobody is left waiting too long
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
            return(-1);
        }
    }

    public bool doesScheduleSatisfyCriteria(object[] schedule) {
        bool result = true;
        return(result);
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
        return(char.Parse(split_schedule[1].Substring(split_schedule[1].Length-2,1)));
    }
}

class LiftPassenger {
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
        string debug = "From floor "+this.origin_floor.ToString()+"; to floor "+this.destination_floor.ToString()+"; waiting pf "+this.waiting_PF.ToString()+"; travelling pf "+this.travelling_PF.ToString()+"; origin time "+this.origin_time.ToString()+"; total time travelled "+this.total_time.ToString()+"; in lift "+in_lift.ToString()+"; journey completed "+journey_completed.ToString();
        return(debug);
    }
}

class CSVDataFrame {
    public string[] column_names = {};
    public object[][] column_data = {};

    public CSVDataFrame(string filepath) {
        constructDataFrameFromCSV(filepath);
    }

    public CSVDataFrame(string[] new_column_names) {
        constructDataFrameManually(new_column_names);
    }

    public void constructDataFrameFromCSV(string filepath) {
        string[] CSV_data = readCSV(filepath);
        constructShortDataFrame(CSV_data);
    }

    public void constructDataFrameManually(string[] new_column_names) {
        this.column_names = new_column_names;
    }

    public static string[] readCSV(string filepath) {
        return(System.IO.File.ReadAllLines(filepath));
    }

    public void constructShortDataFrame(string[] CSV_data) {
        /* Assumes all data is of the primitive data type "short", e.g. an integer less than 32,767 in magnitudes 
           This method is bespoke in the sense that it works only for a subset of cases, which we know applies here */
        this.column_names = CSV_data[0].Split(",");
        this.column_data = new object[this.column_names.Length][];
        for(int i = 0; i < column_data.Length; i++) {
                this.column_data[i] = new object[CSV_data.Length-1];
            }
        for(int i = 1; i < CSV_data.Length; i++) {
            string[] temp_split = CSV_data[i].Split(",");
            for(int j = 0; j < temp_split.Length; j++) {
                this.column_data[j][i-1] = Int16.Parse(temp_split[j]);
            }
        }
    }

    public void constructGenericDataFrame() {
        /* A little different to constructShortDataFrame; assumes we've already chosen our column names
           but don't currently have any actual data, and will be generating it as we go along */
           this.column_data = new object[this.column_names.Length][];
    }

    public void printTable() {
        /* used in the debugging process to ensure formatting was correct */
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
        return(this.column_data[Array.IndexOf(this.column_names,name)]);
    }

    public int length() {
        return(this.column_data[0].Length);
    }
}

class Utility {
    /* Generic helper class to provide various C# methods since I'm trying not to rely on libraries */
    public Utility() {
    }

    public static object[] subsetArray(object[] array, int from, int to) {
        object[] new_array = new object[to-from];
        for(int i = from; i < to; i++) {
            new_array[i-from] = array[i];
        }
        return(new_array);
    }

    public static object[] addArrays(object[] array1, object[] array2) {
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
        object[] new_array = new object[array.Length + 1];
        for(int i = 0; i < array.Length; i++) {
            new_array[i] = array[i];
        }
        new_array[new_array.Length-1] = element;
        return(new_array);
    }

    // Need to overload so we can add strings to arrays to save them in text readable format
    public static string[] appendArrays(string[] array, string element) {
        string[] new_array = new string[array.Length + 1];
        for(int i = 0; i < array.Length; i++) {
            new_array[i] = array[i];
        }
        new_array[new_array.Length-1] = element;
        return(new_array);
    }

    public static object[][] arrayAppendArrays(object[][] array, object[] subarray) {
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
        object[] bolt = {};
        List<object[]> combs = new List<object[]>();
        combs = Utility.calcListCombs(array,combs,bolt);
        List<string> string_results = new List<string>();
        foreach(object[] item in combs) {
            string_results.Add(string.Join(",",item));
        }
        string_results = string_results.Distinct().ToList();
        object[][] combs_final = new object[string_results.Count][];
        for(int i = 0; i < string_results.Count; i++) {
            combs_final[i] = string_results[i].Split(",");
        }
        return(combs_final);
    }

    public static List<object[]> calcListCombs(object[] source_list, List<object[]> combs, object[] bolt) {
        /* Method that fills a text document containing all possible combinations of an array, for later retrieval
           "source_list": the 1D array to be evaluated
           "bolt": the 1D array to be "bolted on" to the start of the source_list so all original array elements are accounted for */
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

    public static void save2DArrayToFile(object[][] array, string path) {
        string[] list_to_save = {};
        foreach(object item in array) {
            list_to_save = appendArrays(list_to_save, string.Join(",",item));
        }
        System.IO.File.WriteAllLines(path, list_to_save.ToList());
    }

    public static void appendArrayToFile(object[] array, string path) {
        System.IO.File.AppendAllText(path,string.Join(",",array)+"\n");
    }

    public static void clearFile(string path) {
        /* Because we rapidly calculate unique array elements and then save then append them to a file, we need to clear
           the results of any previous calculation from the relevant file; however, deleting it means, if we ever want  to
           clear it again, we first have to check it exists as deleting a non-existent file returns an error.
           Therefore, it is more convenient to keep the file but simply erase it's contents! */
        System.IO.File.WriteAllLines(path, new string[0]);
    }

    public static string[] getFileContents(string path) {
        string[] contents  = {};
        return(contents);
    }
}