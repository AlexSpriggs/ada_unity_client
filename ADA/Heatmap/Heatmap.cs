using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class AdaHeatmapData
{
	public string _id;
	public string created_at;
	public string updated_at;
	public string game;
	public int user_id;
	public string gameName;
	public string schema;
	public double timestamp;
	public string session_token;
	public string key;
	//public string level;
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
	private string heatmapPath = "/data/heatmap.json?";

	public static Heatmap mapper { get; private set; }

	public string dataKey;
	public string startTime;

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
	private bool initalized = true;
	public bool StartIt = false;
	public Texture2D mapTexture;
	private GameObject parent;

	public int BLOCK_SIZE = 10;

	public string jsonPositionFile;
	public string filePath = "";

	void Awake()
	{
		if (mapper == null) mapper = this;
		heatmapURL = serverURL + heatmapPath;
		DontDestroyOnLoad(this);

	}

	// Use this for initialization
	void Start () {
	


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
			float percent = Mathf.Log10((float)pair.Value/10.0f);
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
			int cx =	(int)cpd.x - (int)cpd.x%BLOCK_SIZE;
			int cy = 	(int)cpd.y - (int)cpd.y%BLOCK_SIZE;
			int cz = 	(int)cpd.z - (int)cpd.z%BLOCK_SIZE;
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
		int second_highest = 0;
		float average = 0;
		foreach(int count in counts.Values)
		{
			if(count > highest_hit)
			{
				second_highest = highest_hit;
				highest_hit = count;
			}
			average += count;
				
		}

		DebugEx.Log("highest hit count: " + highest_hit);
		DebugEx.Log("System count: " + counts.Count);
		average = (float)average/(float)counts.Count;

		foreach(KeyValuePair<Vector3,int> pair in counts)
		{
			Vector3 coord = pair.Key;
			//float percent = Mathf.Log((float)pair.Value/10f);
			//float percent = (float)pair.Value/highest_hit;
			float percent = 0;
			Debug.Log("percent " + percent);
			GameObject obj = (GameObject)Instantiate(particleSystemPrefab);
			obj.transform.parent = parent.transform;
			ParticleSystem ps = obj.GetComponent<ParticleSystem>();
			ps.transform.position = coord;
			ps.emissionRate = ps.emissionRate + (ps.emissionRate * percent);
			//ps.startSize = ps.startSize * percent;
			//ps.startColor = Color.Lerp(ps.startColor, Color.red, percent);
			if(pair.Value > average)
			{
				ps.startColor = Color.Lerp(new Color(0,.5f,0,.0f), new Color(.7f,0,0,1f), (float)(pair.Value-average)/(float)average);
			}
			else
			{
				ps.startColor = Color.Lerp(new Color(0,.5f,0,.0f), new Color(0,0,0f,0f), (float)(pair.Value)/(float)average);
			}
		}

	}

	void DrawLines()
	{
		userSets = new Dictionary<int, HeatmapData>();
		foreach(AdaHeatmapData cpd in allTheData)
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
		DebugEx.Log("Init heatmap");
		initalized = true;
		//BuildTexture();
		SpawnParticleSystems();
	


		
		Debug.Log("read " + allTheData.Length + " points of data");

	}
	
	// Update is called once per frame
	void Update () {

		if(StartIt == false)
		{
			return;
		}

		if(dataFetching == false && UserManager.isLoggedIn)
		{
			dataFetching = true;
			initalized = false;
			allTheData = null;
			DestroyObject(parent);
			if(filePath != "")
			{
				LoadDataFromDirectory();
			}
			else
			{
				LoadDataFromFile();
			}

			//StartCoroutine(RequestHeatmap());
			StartIt = false;
		}
		InitHeatmap();
		
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
		int i = 0;
		GUILayout.BeginVertical();
		foreach(GameObject obj in players)
		{
			obj.renderer.enabled = GUILayout.Toggle(obj.renderer.enabled, obj.name);	
			i++;
		}
		if(GUILayout.Button("Show all"))
		{
			foreach(GameObject obj in players)
			{
				obj.renderer.enabled = true;	
			}
		}
		if(GUILayout.Button("Hide all"))
		{
			foreach(GameObject obj in players)
			{
				obj.renderer.enabled = false;	
			}
		}
		GUILayout.EndVertical();
		
	}

	private void LoadDataFromFile()
	{
		string jsonString = System.IO.File.ReadAllText(jsonPositionFile);
		List<AdaHeatmapData> temp = new List<AdaHeatmapData>();
		if(mapper.allTheData != null)
		{
			temp.AddRange(mapper.allTheData);
		}
		temp.AddRange(JsonMapper.ToObject<AdaHeatmapData[]>(jsonString));
		mapper.allTheData = temp.ToArray();
		dataFetching = false;
	}

	private void LoadDataFromDirectory()
	{
		string[] files = System.IO.Directory.GetFiles(filePath);
		Debug.Log("filePath == " + filePath);
		Debug.Log("file count " + files.Length);
		for(int i=0; i < files.Length; i++)
		{
			if(files[i].Contains(".json"))
			{
				Debug.Log("reading file " + files[i]);
				jsonPositionFile = files[i];
				LoadDataFromFile();
			}
		}
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
		DebugEx.Log("Request heatmap");

		//string url = heatmapURL + "&gameName=" + GameInfo.GAME_NAME + "&level=" +  Application.loadedLevelName + "&key=" + mapper.dataKey + "&since=" + mapper.startTime.ToString() + "&auth_token=" + UserManager.userInfo.auth_token.ToString();
		string url = heatmapURL + "&gameName=" + GameInfo.GAME_NAME + "&key=" + mapper.dataKey + "&since=" + mapper.startTime.ToString() + "&auth_token=" + UserManager.userInfo.auth_token.ToString();

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

		}
		catch(JsonException)
		{
            if(webMessage.error == WebErrorCode.None)
			{
				DebugEx.LogError("Heatmap retreival failed to parse");
				yield break;
			}
		}

		
	}
}
