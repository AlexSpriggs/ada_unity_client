using System.Collections.Generic;

/// <summary>
/// Information returned when authenticating with ADA
/// </summary>
public class UserData : System.Object
{
	public string session_id { get; set; }
    public string auth_token { get; set; }
}

/// <summary>
/// If ADA has encountered an error with a request it will return this object instead of expected object
/// </summary>
public class ErrorData : System.Object
{
	public string error { get; set; }
}

public class RegisterWrapper : System.Object
{
	public RegisterUser user {get; set;}
}

/// <summary>
/// Class used for registering a user with the ADA system
/// </summary>
public class RegisterUser : System.Object
{
	public string email { get; set;}
	public string password {get; set;}
	public string password_confirmation {get; set;}
}

/// <summary>
/// A class used for parsing errors returned from the ADA user registration system
/// </summary>
public class RegistrationError : System.Object
{
	public List<string> email;
	public List<string> password;
}

/// <summary>
/// A class used for parsing a successful registration
/// </summary>
public class RegistrationSuccess : System.Object
{
	public string authentication_token { get; set; }
	public string email {get; set;}
}


/// <summary>
/// We may send more then one log event at a time.  
/// </summary>
public class ADAwrapper
{
	public List<ADAData> data = new List<ADAData>();
}

public class AdaInfo {
	public static string ADA_VERSION = "adventurous_aardvark";
}

/// <summary>
/// Base class for data objects for ADA's data collection system.  Subclass this to create objects for the game
/// to log.
/// </summary>
public class ADAData
{
	//These get set by the Data Collection system do not set directly.
	public string gameName {get; set;}
	public string schema {get; set;}
	public string ADAVersion = AdaInfo.ADA_VERSION;
	public float timestamp { get; set; }
	public string session_token { get; set;}
	
	public string key { get; set; } //This is the only data that the game using ADA needs to set when sending data
}

//a structure for recording location information that can be used to build heatmaps.
public class ADAPosition : ADAData
{
	public string level;  //The name of the level(map, scene, stage, etc)
	public string quest;  //Name of the quest - this should be unique
	public float x;
	public float y;
	public float z;
	//Euler angles for rotation
	public float rotx;
	public float roty;
	public float rotz;
}

/**
 * An objective 
 */
public class ADAObjectiveStart : ADAData{
	public string objectiveName;
}

public class ADAObjectiveComplete : ADAData{
	public string objectiveName;
}


/**
 * A cycle is an activity that a player must complete in order to progress an objective.
 * Example: In fairplay, you may have an objective like "get funding". In order to do this
 * you must have conversations. Therefore, conversations are cycles. cycleEnd would be 
 * triggered when the conversation is exited. In progenitor x an objective may be "collect 
 * x cells" where populating cells would be the start of a cycle and collecting would be 
 * the end, if the gird is destroyed this would also trigger an unsuccessful cycle. 
 * Portal: objective is complete the room. CycleStart may be press the button. cycleEnd 
 * is the check to see if the player makes it through the door, or if the door closes 
 * and a player does not make it through. If a companion cube is needed, releasing one, 
 * and getting it would be cycleStart and cycleEnd.
 * 
 * OnTask - this is true if the current action will progress the current objective. If you
 * have multiple objectives you may send out 2 signals to ADA one that says the action is 
 * OnTask for objective 1 and one that is OnTask = false for objective 2.
 * 
 */
public class ADACycleStart : ADAData{
	public string objectiveName;
	public string level;
	public bool onTask; //true if the begining of this cycle is needed to successfuly complete objective
}

public class ADACycleEnd : ADAData{
	public string objectiveName;
	public string level;
	public bool success; //true if there is progress to objective
}

public class ADAQuitGame : ADAData{
	public string level;
}


/*  In order for the game name and schema to be set copy this class to the games file with data logging classes.  Set the game name and update the
 * schema string with the data that the game related data structures were last modified.  Example below:
public class GameInfo
{
	public static string GAME_NAME = "APA:Tracts";
	public static string SCHEMA = "1-31-2012";
}
*/
