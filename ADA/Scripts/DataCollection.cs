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
	
	/// <summary>
	/// Initialize
	/// </summary>
	void Awake()
	{
		if (dc == null) dc = this;
		if (cd != this) Destroy(this.gameObject);
		collectionURL = serverURL + collectionPath;
		DontDestroyOnLoad(this);
		
#if UNITY_IPHONE
		logPath = Application.dataPath.Substring(0, Application.dataPath.Length - 23) + "Documents/";
		if(Application.isEditor)
		{
			logPath = ".";
		}
#else
		logPath = "";
#endif
		Debug.Log("********************" + Application.dataPath);
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
	
	/// <summary>
	/// Add data to the log
	/// </summary>
	/// <param name="data">
	/// A <see cref="ADAData"/>
	/// </param>
	public static void WriteData(ADAData data)
	{
		
		data.gameName = GameInfo.GAME_NAME;
		data.schema = GameInfo.SCHEMA;
		data.timestamp = Time.realtimeSinceStartup;
		data.session_token = UserManager.session_token;
		wrapper.data.Add(data);
		//Debug.Log("add log");
		
	}

	/// <summary>
	/// Uploads collected data to online
	/// </summary>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	private static IEnumerator PushDataOnline(string outgoingData)
	{
		if(Debug.isDebugBuild)
		{
			//Skip online write and just push the data locally in debug
			PushDataLocal(outgoingData);
		}
		
		WebMessage webMessage = new WebMessage();
		Debug.Log("Writing Log Online");
		Debug.Log(outgoingData);
		yield return dc.StartCoroutine(webMessage.PostAuthenticated(collectionURL, 
		                                            UserManager.userInfo.auth_token, outgoingData ));
		if(webMessage.error == WebErrorCode.Error)
        {
			//we had an error so write the data locally.
            Debug.Log(webMessage.extendedError);
			PushDataLocal(outgoingData);
            yield break;
        }
		
		Debug.Log("Online write complete");
		pushingData = false;
		
	}
	
	/// <summary>
	/// Writes a local text file with log data
	/// </summary>
	private static void PushDataLocal(string outgoingData)
	{
#if !UNITY_WEBPLAYER
		Debug.Log("Writing Log Local");	
		string stripedData = outgoingData.Substring(9, outgoingData.Length - 11) + ",";
		Debug.Log(stripedData);
		if(Debug.isDebugBuild)
		{
			System.IO.File.AppendAllText(logPath+UserManager.session_token+".debug", stripedData);
		}
		else
		{
			System.IO.File.AppendAllText(logPath+UserManager.playerName+UserManager.session_token+".data", stripedData);
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
#if !UNITY_WEBPLAYER
		StreamReader logFile;
		string line = "";
		string outgoingData = "{\"data\":[";
		Debug.Log("logPath == " + logPath);
		string[] files = System.IO.Directory.GetFiles(logPath, ".data");
		for(int i=0; i < files.Length; i++)
		{
			Debug.Log("reading file: " + files[i]);
			if(files[i].Contains(UserManager.playerName))
			{
				outgoingData += System.IO.File.ReadAllText(files[i]);
				outgoingData = outgoingData.Substring(0, outgoingData.Length - 1);  //remove trailing comma
				outgoingData += "]}";  //add closing brackets.
				dc.StartCoroutine(PushDataOnline(outgoingData));
			}
			else
			{
				Debug.Log("skipping file from different user: " + files[i]);	
			}
			//System.IO.File.Delete(files[i]);
		}
#endif
		//Added since something goes wonky if we have a function with nothing in it...
		return;
	}
	
}