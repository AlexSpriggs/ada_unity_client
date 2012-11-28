using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class AdaHeatmapData
{
	public string _id;
	public string _type;
	public string created_at;
	public string updated_at;
	public string game;
	public int user_id;
	public string gameName;
	public string schema;
	public double timestamp;
	public string session_token;
	public string key;
	public string level;
	public double x;
	public double y;
	public double z;
	public double rotx;
	public double roty;
	public double rotz;

}

public class HeatmapData
{
	public List<Vector3> points;
}



public class Heatmap : MonoBehaviour {

	/// <summary>
	/// The server url
	/// </summary>
	public string serverURL;
	
	/// <summary>
	/// the url that we will be writing the data to.
	/// </summary>
	private static string heatmapURL;
	private static string user_data_url;
	private string heatmapPath = "/data/heatmap.json?";
	private string userDataPath = "/user/user_data.json?";

	public static Heatmap mapper;

	public string dataKey;
	public string startTime;
	public List<int> UserIds;

	string stuff;
	public AdaHeatmapData[] allTheData;
	public GameObject ProjectorPrefab;


	public GameObject prefab;
	public GameObject holderPrefab;
	public GameObject particleSystemPrefab;
	public GameObject user;
	public int index = 0;
	public int currentUser;
	public Color currentColor;
	public Dictionary<int, HeatmapData> userSets;
	public List<GameObject> players;

	private bool dataFetching = false;
	private bool initalized = false;
	public Texture2D mapTexture;
	private GameObject parent;

	public static int BLOCK_SIZE = 1;



	void Awake()
	{
		if (mapper == null) mapper = this;
		heatmapURL = serverURL + heatmapPath;
		user_data_url = serverURL + userDataPath;
		DontDestroyOnLoad(this);

	}



	void BuildTexture() {

	
		Dictionary<Vector2, int> counts = new Dictionary<Vector2, int>();
		mapTexture = new Texture2D(256, 256);
		Color[] blue = new Color[256*256];
		for(int i=0; i < 256*256; i++)
		{
			blue[i] = Color.blue;
		}
		mapTexture.SetPixels(blue,0);
		foreach(AdaHeatmapData cpd in allTheData)
		{
			int cx = 128 + (int)cpd.x;
			int cy = 128 + (int)cpd.z;
			Vector2 key = new Vector2(cx, cy);
			if(counts.ContainsKey(key))
			{
				counts[key] = counts[key] + 1;
			}
			else
			{
				counts.Add(key, 1);
			}
		}

		int highest_hit = 0;
		foreach(int count in counts.Values)
		{
			if(count > highest_hit)
			{
				highest_hit = count;
			}
				
		}

		DebugEx.Log("highest count: " + highest_hit);

		foreach(KeyValuePair<Vector2,int> pair in counts)
		{
			Vector2 coord = pair.Key;
			float percent = (float)pair.Value/(float)highest_hit;
			Color[] pixels = mapTexture.GetPixels((int)coord.x, (int)coord.y, BLOCK_SIZE, BLOCK_SIZE);
			for(int i=0; i < pixels.Length; i++)
			{
				pixels[i] = Color.Lerp(pixels[i], Color.red, percent);
			}
			mapTexture.SetPixels((int)coord.x, (int)coord.y, BLOCK_SIZE, BLOCK_SIZE, pixels);

			mapTexture.Apply();

		}

		GameObject proj_obj = (GameObject)Instantiate(ProjectorPrefab);
		Projector proj = proj_obj.GetComponent<Projector>();
		//proj.material.SetTexture("_ShadowTex", mapTexture);
		proj.material.SetTexture("_MainTex", mapTexture);



	}

	void SpawnParticleSystems ()
	{

		parent = new GameObject("heatmap");
		parent.transform.position = new Vector3(0,1,0);
		Dictionary<Vector3, int> counts = new Dictionary<Vector3, int>();
		foreach(AdaHeatmapData cpd in allTheData)
		{
			int cx =	(int)cpd.x + (int)cpd.x%BLOCK_SIZE;
			int cy = 	(int)cpd.y + (int)cpd.y%BLOCK_SIZE;
			int cz = 	(int)cpd.z + (int)cpd.z%BLOCK_SIZE;
			Vector3 key = new Vector3(cx, cy, cz);
			if(counts.ContainsKey(key))
			{
				counts[key] = counts[key] + 1;
			}
			else
			{
				counts.Add(key, 1);
			}
		}

		int highest_hit = 0;
		float average = 0;
		foreach(int count in counts.Values)
		{
			if(count > highest_hit)
			{
				highest_hit = count;
			}

			average += count;
				
		}

		average = (float)average/(float)counts.Count;

		DebugEx.Log("highest hit count: " + highest_hit);
		DebugEx.Log("average hit count: " + average);
		DebugEx.Log("System count: " + counts.Count);


		foreach(KeyValuePair<Vector3,int> pair in counts)
		{
			Vector3 coord = pair.Key;
			float percent = 0;

			//float percent = (float)pair.Value/(float)highest_hit;
			//float percent = Mathf.Log((float)pair.Value);
			GameObject obj = (GameObject)Instantiate(particleSystemPrefab);
			obj.transform.parent = parent.transform;
			ParticleSystem ps = obj.GetComponent<ParticleSystem>();
			ps.transform.position = coord;
			ps.emissionRate = ps.emissionRate + (ps.emissionRate * percent);
			//ps.startSize = ps.startSize * percent;
			if(pair.Value > average)
			{
				ps.startColor = Color.Lerp(ps.startColor, new Color(.7f,0,0,.5f), (float)(pair.Value-average)/(float)average);
			}
			else
			{
				ps.startColor = Color.Lerp(ps.startColor, new Color(0,0,.7f,.5f), (float)(pair.Value)/(float)average);
			}

		}

	}

	void InitUserPath(AdaHeatmapData[] userData)
	{
		userSets = new Dictionary<int, HeatmapData>();
		foreach(AdaHeatmapData cpd in userData)
		{
			if(userSets.ContainsKey(cpd.user_id))
			{
				userSets[cpd.user_id].points.Add(new Vector3((float)cpd.x, (float)cpd.y, (float)cpd.z));	
			}
			else
			{
				HeatmapData foo = new HeatmapData();
				foo.points = new List<Vector3>();
				userSets.Add(cpd.user_id, foo);
				userSets[cpd.user_id].points.Add(new Vector3((float)cpd.x, (float)cpd.y, (float)cpd.z));	
			}
		}
		
		players = new List<GameObject>();
		foreach(KeyValuePair<int,HeatmapData> points in userSets)
		{
			user = (GameObject)Instantiate(holderPrefab);
			user.name = points.Key.ToString();
			
			LineRenderer line = user.GetComponent<LineRenderer>();
			int i=0;
			currentColor = new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),1);
			line.SetColors(currentColor, currentColor);
			user.renderer.material.color = currentColor;
			line.SetVertexCount(points.Value.points.Count);
			foreach(Vector3 dot in points.Value.points)
			{
				line.SetPosition(i,dot);
				i++;
			}
			user.renderer.enabled = true;
			players.Add(user);
			
		}
		
	}

	void InitHeatmap () {

		if(initalized == true || allTheData == null)
		{
			return;
		}

		initalized = true;
		//BuildTexture();
		SpawnParticleSystems();
	


		
		Debug.Log("read " + allTheData.Length + " points of data");

	}
	

	public void StartHeatmap () {

		if(dataFetching == false && UserManager.isLoggedIn)
		{
			dataFetching = true;
			StartCoroutine(RequestHeatmap());
		}
		else
		{
			Debug.Log("you must be logged in to Ada to perform this action.");
		}
	}

	public void StartUserData () {

		if(dataFetching == false && UserManager.isLoggedIn)
		{
			dataFetching = true;
			foreach(int id in UserIds)
			{
				StartCoroutine(RequestUserData(id));
			}
		}
		else
		{
			Debug.Log("you must be logged in to Ada to perform this action.");
		}
	}



	public void OnLevelWasLoaded(int level)
    {
		dataFetching = false;
		initalized = false;
		allTheData = null;
		DestroyObject(parent);
    }
	
	void OnGUI()
	{

	

		
	}

	/// <summary>
	/// Get the heatmap data for this level and a time range
	/// </summary>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	private static IEnumerator RequestHeatmap()
	{


		WebMessage webMessage = new WebMessage();

		string url = heatmapURL + "&gameName=" + GameInfo.GAME_NAME + "&schema=" + GameInfo.SCHEMA + "&level=" +  Application.loadedLevelName + "&key=" + mapper.dataKey + "&since=" + mapper.startTime.ToString() + "&auth_token=" + UserManager.userInfo.auth_token.ToString();

		DebugEx.Log(url);
		yield return mapper.StartCoroutine(webMessage.PostNoData(url));
		if(webMessage.error == WebErrorCode.Error)
        {
			DebugEx.LogError("Heatmap retreival failed " + webMessage.extendedError);
            yield break;
        }

		try 
		{
			DebugEx.Log(webMessage.www.text);
			mapper.allTheData = JsonMapper.ToObject<AdaHeatmapData[]>(webMessage.www.text);
			mapper.InitHeatmap();

		}
		catch(JsonException)
		{
            if(webMessage.error == WebErrorCode.None)
			{
				DebugEx.LogError("Heatmap retreival failed to parse");
				yield break;
			}
		}

		mapper.dataFetching = false;
		
	}

	/// <summary>
	/// Get one users data
	/// </summary>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	private static IEnumerator RequestUserData(int user_id)
	{


		WebMessage webMessage = new WebMessage();
		AdaHeatmapData[] userData;

		string url = user_data_url + "&gameName=" + GameInfo.GAME_NAME + "&schema=" + GameInfo.SCHEMA + "&level=" +  Application.loadedLevelName + "&key=" + mapper.dataKey + "&user_id=" + user_id + "&auth_token=" + UserManager.userInfo.auth_token.ToString();

		DebugEx.Log(url);
		yield return mapper.StartCoroutine(webMessage.PostNoData(url));
		if(webMessage.error == WebErrorCode.Error)
        {
			DebugEx.LogError("User Data retreival failed " + webMessage.extendedError);
            yield break;
        }

		try 
		{
			DebugEx.Log(webMessage.www.text);
			userData = JsonMapper.ToObject<AdaHeatmapData[]>(webMessage.www.text);
			mapper.InitUserPath(userData);

		}
		catch(JsonException)
		{
            if(webMessage.error == WebErrorCode.None)
			{
				DebugEx.LogError("User Data retreival failed to parse");
				yield break;
			}
		}

		mapper.dataFetching = false;
		
	}
}
