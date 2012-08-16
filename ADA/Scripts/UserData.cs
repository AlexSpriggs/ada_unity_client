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

/// <summary>
/// Base class for data objects for ADA's data collection system.  Subclass this to create objects for the game
/// to log.
/// </summary>
public class ADAData
{
	//These get set by the Data Collection system do not set directly.
	public string gameName {get; set;}
	public string schema {get; set;}
	public int timestamp { get; set; }
	public string session_token { get; set;}
	
	public string key { get; set; } //This is the only data that the game using ADA needs to set when sending data
}

//a structure for recording location information that can be used to build heatmaps.
public class ADAPosition : ADAData
{
	public string level;  //The name of the level - this should be unique
	public float x;
	public float y;
	public float z;
	//Euler angles for rotation
	public float rotx;
	public float roty;
	public float rotz;
}

/*  In order for the game name and schema to be set copy this class to the games file with data logging classes.  Set the game name and update the
 * schema string with the data that the game related data structures were last modified.  Example below:
public class GameInfo
{
	public static string GAME_NAME = "APA:Tracts";
	public static string SCHEMA = "1-31-2012";
}
*/