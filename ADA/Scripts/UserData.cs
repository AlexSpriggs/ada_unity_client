using UnityEngine;
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
	public static string ADA_VERSION = "bodacious_bonobo";
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
	public string timestamp { get; set; }
	public string session_token { get; set;}
	public string ada_base_type { get; set;}
	public string key { get; set; } 

}

/// <summary>
/// ADA virtual context - this is not to be used directly for logging but to be included in other structures like player actions to establish the context of the action within the scope of the game
/// </summary>
public class ADAVirtualContext
{

	public string level;  //The name of the level(map, scene, stage, etc)
	public List<string> active_units; //Names of all the currently active game units. This can be used as a flat list of tags for processing actions within the scope of the units
	
	public ADAVirtualContext(){
		active_units = new List<string>();
		level = "";
	}
}

/// <summary>
/// ADA positional context - used for adding a positional element to an ADA log entry
/// </summary>
public class ADAPositionalContext
{
	public float x;
	public float y;
	public float z;
	//Euler angles for rotation
	public float rotx;
	public float roty;
	public float rotz;
	public ADAPositionalContext(){
		x = 0f;
		y = 0f;
		z = 0f;
		rotx = 0f;
		roty = 0f;
		rotz = 0f;
	}
	public void setPosition(float iX, float iY, float iZ)
	{
		x = iX;
		y = iY;
		z = iZ;
	}

	public void setRotation(float iX, float iY, float iZ)
	{
		rotx = iX;
		roty = iY;
		rotz = iZ;
	}
}


/// <summary>
/// ADA unit start - Start a unit of game play
/// </summary>
public class ADAUnitStart : ADAData 
{
	public string name; //Should be unique
	public string parent_name; //This can be left blank if there is no parent for this unit
	public ADAUnitStart(){
		name = "";
		parent_name = "";
	}
}

public class ADAUnitEnd : ADAData 
{
	public string name; //Should be unique
	public string parent_name; //This can be left blank if there is no parent for this unit
	public ADAUnitEnd(){
		name = "";
		parent_name = "";
	}
}

/// <summary>
/// ADA player action - subclass this to add logs for player actions
/// </summary>
public class ADAPlayerAction : ADAData 
{
	public ADAVirtualContext virtual_context;
	public ADAPositionalContext positional_context;
	public ADAPlayerAction(){
		virtual_context = new ADAVirtualContext();
		positional_context = new ADAPositionalContext();
	}
}

/// <summary>
/// ADA game event - subclass this to add game event data 
/// </summary>
public class ADAGameEvent : ADAData
{
	public ADAVirtualContext virtual_context;
	public ADAGameEvent(){
		virtual_context = new ADAVirtualContext();	
	}
}

/// <summary>
/// ADA player choice - often in games players are asked to choose between several options
/// Examples:
/// identifying an emotion of an NPC
/// Correctly identifying an bias
/// Picking from a list of choices a correct answer
/// Picking a choice in a conversation
/// </summary>
public class ADAPlayerChoice : ADAPlayerAction
{
	public string correct_answer;
	public string player_answer;
	public float answer_weight; //a ranged value determining what weight the players choice has. The game will determine the range examples could be -1 to 1 for a range of worst to best answer
	public float time_to_answer; //How long did it take to answer the question (use the timer start and stop for easy calculation

	public void start_timer()
	{
		time_to_answer = Time.realtimeSinceStartup;
	}

	public void stop_timer()
	{
		time_to_answer = Time.realtimeSinceStartup - time_to_answer;
	}
}

public class ADAQuitGame : ADAPlayerAction {}



/*  In order for the game name and schema to be set copy this class to the games file with data logging classes.  Set the game name and update the
 * schema string with the data that the game related data structures were last modified.  Example below:
public class GameInfo
{
	public static string GAME_NAME = "APA:Tracts";
	public static string SCHEMA = "1-31-2012";
}
*/

