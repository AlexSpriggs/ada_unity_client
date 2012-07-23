using UnityEngine;
using System.Collections;

/// <summary>
/// User manager
/// </summary>
public class UserManager : MonoBehaviour
{
	
	/// <summary>
	/// the user info 
	/// </summary>
	// TODO not sure what the best way to pass data from a level to another level so 
	// this is done by using a static public data member for now
	static public UserData userInfo { get; set; }
	
	/// <summary>
	/// Is the user currently online
	/// </summary>
	static public bool isOnline {get; set;}
	
	/// <summary>
	/// Is there a user logged in
	/// </summary>
	static public bool isLoggedIn {get; set;}
	
	/// <summary>
	/// the players name
	/// </summary>
	static public string playerName {get; set;}
	
	/// <summary>
	/// the session token
	/// </summary>
	static public string session_token {get; set;}

	/// <summary>
	/// singleton
	/// </summary>
	public static UserManager use { get; private set; }
	
	/// <summary>
	/// Initialize
	/// </summary>
	void Awake()
	{
		if (use != null) Destroy(use.gameObject);
		use = this;
	}
}