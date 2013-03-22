#define DATA_COLLECTION_DEBUG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



/// <summary>
/// Handles the collection of research data from the game
/// </summary>
public class DataCollection : MonoBehaviour
{
	

	/// <summary>
	/// The server url
	/// </summary>
	public string serverURL;
	
	/// <summary>
	/// the url that we will be writing the data to.
	/// </summary>
	private static string collectionURL;
	private string collectionPath = "/data_collector";
	
	private static ADAwrapper wrapper = new ADAwrapper();
	
	
	/// <summary>
	/// singleton
	/// </summary>
	public static DataCollection dc { get; private set; }
	
	private static bool pushingData;
	private static string logPath;
	
	/// <summary>
	/// When was the last time we pushed the data to the database or local log
	/// </summary>
	private float lastPush = 0;  
	
	/// <summary>
	/// How often we push the data to the database or local log - in seconds.
	/// </summary>
	public int pushRate = 5;

	private static ADAVirtualContext current_vc;
	private static ADAPositionalContext current_pc;
	private static ADAUnitStart unit_start;
	private static ADAUnitEnd unit_end;
	
	/// <summary>
	/// Initialize
	/// </summary>
	void Awake()
	{
		if (dc == null) dc = this;
		if (dc != this) Destroy(this.gameObject);
		collectionURL = serverURL + collectionPath;
		DontDestroyOnLoad(this);
		
#if UNITY_IPHONE
		logPath = Application.persistentDataPath; //.Substring(0, Application.dataPath.Length - 5) + "/Documents/";
		if(Application.isEditor)
		{
			logPath = ".";
		}
		Debug.Log("********************" +Application.dataPath.Substring(0, Application.dataPath.Length - 5) + "/Documents/");
#else
		logPath = "";
#endif
		Debug.Log("********************" + Application.persistentDataPath);

		current_vc = new ADAVirtualContext();
		current_vc.level = Application.loadedLevelName;
		current_pc = new ADAPositionalContext();
		unit_start = new ADAUnitStart();
		unit_end = new ADAUnitEnd();
	}
	
	void Update()
	{
		//right now flush the data if there is anything to write.
		float elapsedTime = Time.time - lastPush;
		if((elapsedTime > pushRate) && wrapper.data.Count != 0 && !pushingData)
		{
			pushingData = true;
			lastPush = Time.time;
			string outgoingData = JsonMapper.ToJson(wrapper);
			wrapper.data.Clear();
			if(UserManager.isOnline)
			{
				StartCoroutine(PushDataOnline(outgoingData));
			}
			else
			{
				PushDataLocal(outgoingData);	
			}
		}
		
	}

	//Update the current virtual context with the level name
	void OnLevelWasLoaded(int level) {

		current_vc.level = Application.loadedLevelName;
	}

	//Try to flush the log before application quit
	void OnApplicationQuit() {

		ADAQuitGame log_quit = new ADAQuitGame();
		LogPlayerAction(log_quit);

		pushingData = true;
		lastPush = Time.time;
		string outgoingData = JsonMapper.ToJson(wrapper);
		wrapper.data.Clear();
		if(UserManager.isOnline)
		{
			StartCoroutine(PushDataOnline(outgoingData));
		}
		else
		{
			PushDataLocal(outgoingData);	
		}

	}
	/// <summary>
	/// Updates the current positional context - this will be inserted into all ADAPlayerAction structures
	/// </summary>
	/// <param name='position'>
	/// Position.
	/// </param>
	public static void UpdatePositionalContext(Transform position)
	{
		if (dc == null) return;
		current_pc.setPosition(position.position.x, position.position.y, position.position.z);
		current_pc.setRotation(position.rotation.eulerAngles.x, position.rotation.eulerAngles.y, position.rotation.eulerAngles.z);

	}

	public static void UpdatePositionalContext(Vector3 pos, Vector3 rot = new Vector3())
	{
		if (dc == null) return;
		current_pc.setPosition(pos.x, pos.y, pos.z);
		current_pc.setRotation(rot.x, rot.y, rot.z);

	}

	/// <summary>
	/// Log a player action structure
	/// </summary>
	/// <param name='action'>
	/// Action.
	/// </param>
	public static void LogPlayerAction(ADAPlayerAction action)
	{
		if (dc == null) return;
		action.virtual_context = current_vc;
		action.positional_context = current_pc;
		action.ada_base_type = "ADAPlayerAction";
		DataCollection.dc.WriteData(action);

	}

	public static void LogGameEvent(ADAGameEvent game_event)
	{
		if (dc == null) return;
		game_event.virtual_context = current_vc;
		game_event.ada_base_type = "ADAGameEvent";
		DataCollection.dc.WriteData(game_event);
	}

	/// <summary>
	/// Start a Unit of game play. 
	/// </summary>
	/// <param name='name'>
	/// Name.
	/// </param>
	public static void StartUnit(ADAUnitStart start)
	{
		if (dc == null) return;
		//Add this unit to the virtual context
		current_vc.active_units.Add(start.name);
		start.ada_base_type = "ADAUnitStart";
		DataCollection.dc.WriteData(start);
	}

	/// <summary>
	/// End a Unit of game play. 
	/// </summary>
	/// <param name='name'>
	/// Name.
	/// </param>
	public static void EndUnit(ADAUnitEnd end)
	{
		if (dc == null) return;
		//Add this unit to the virtual context
		current_vc.active_units.Remove(end.name);
		end.ada_base_type = "ADAUnitEnd";
		DataCollection.dc.WriteData(end);
	}

	/// <summary>
	/// Add data to the log
	/// </summary>
	/// <param name="data">
	/// A <see cref="ADAData"/>
	/// </param>
	private void WriteData(ADAData data)
	{
		
		if (dc == null) return;
		data.gameName = GameInfo.GAME_NAME;
		data.schema = GameInfo.SCHEMA;
		data.timestamp = System.DateTime.Now.ToUniversalTime().ToString();
		data.session_token = UserManager.session_token;
		data.key = data.GetType().ToString();
		wrapper.data.Add(data);
		//Debug.Log("add log");
		
	}

	/// <summary>
	/// Uploads collected data to online
	/// </summary>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	private static IEnumerator PushDataOnline(string outgoingData, string auth_token = "")
	{

		if(auth_token.Equals(""))
		{
			auth_token = UserManager.userInfo.auth_token;
		}

		if(Debug.isDebugBuild)
		{
			//Skip online write and just push the data locally in debug
			PushDataLocal(outgoingData);

		}


		
		WebMessage webMessage = new WebMessage();
#if DATA_COLLECTION_DEBUG
		Debug.Log("Writing Log Online: " + auth_token);
		Debug.Log(outgoingData);
#endif
		yield return dc.StartCoroutine(webMessage.PostAuthenticated(collectionURL, 
		                                            auth_token, outgoingData ));
		if(webMessage.error == WebErrorCode.Error)
        {
			//we had an error so write the data locally.
            //Debug.Log(webMessage.extendedError);
			PushDataLocal(outgoingData);
            yield break;
        }
		
		//Debug.Log("Online write complete");
		pushingData = false;
		
	}
	
	/// <summary>
	/// Writes a local text file with log data
	/// </summary>
	private static void PushDataLocal(string outgoingData)
	{
		if (dc == null) return;
#if !UNITY_WEBPLAYER
		Debug.Log("Writing Log Local");	
		string stripedData = outgoingData.Substring(9, outgoingData.Length - 11) + ",";
		Debug.Log(stripedData);
		if(Debug.isDebugBuild)
		{
			Debug.Log(logPath+UserManager.session_token+".debug");
			System.IO.File.AppendAllText(logPath+UserManager.session_token+".debug", stripedData);
		}
		else
		{
			Debug.Log(logPath+"/"+UserManager.userInfo.auth_token+"_"+UserManager.playerName+UserManager.session_token+".data");
			System.IO.File.AppendAllText(logPath+"/"+UserManager.userInfo.auth_token+"_"+UserManager.playerName+UserManager.session_token+".data", stripedData);
		}
		outgoingData = "";
#endif
		pushingData = false;

	}
	
	/// <summary>
	/// Pushes previously collected local logs to online
	/// </summary>
	public static void PushLocalToOnline()
	{
		if (dc == null) return;
#if !UNITY_WEBPLAYER
		StreamReader logFile;
		Debug.Log("Push Local to Online");	
		string line = "";
		string outgoingData = "{\"data\":[";
		Debug.Log("logPath == " + logPath);
		string[] files = System.IO.Directory.GetFiles(logPath, "*.data");
		Debug.Log("File Count: " + files.Length);
		for(int i=files.Length-1; i >= 0; i--)
		{
			Debug.Log("reading file: " + files[i]);
			//if(files[i].Contains(UserManager.playerName))
			//{
			string auth_token = files[i].Split('_')[0];
			outgoingData += System.IO.File.ReadAllText(files[i]);
			outgoingData = outgoingData.Substring(0, outgoingData.Length - 1);  //remove trailing comma
			outgoingData += "]}";  //add closing brackets.
			dc.StartCoroutine(PushDataOnline(outgoingData, auth_token));
			System.IO.File.Delete(files[i]);
			//}
			//else
			//{
			//	Debug.Log("skipping file from different user: " + files[i]);	
			//}
		
		}
#endif
		//Added since something goes wonky if we have a function with nothing in it...
		return;
	}
	
}