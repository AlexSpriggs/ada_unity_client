using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// The GUI for the login screen
/// </summary>
public class LoginGUI : MonoBehaviour
{
	

	/// <summary>
	/// The server url
	/// </summary>
	public string serverURL;
	
	/// <summary>
	/// Have users log in with pre-created serial numbers for research data collection
	/// </summary>
	public bool serialNumberLogin;
	public string serialSuffix;
	public string serialPassword;
	
	/// <summary>
	/// Never as the user for login info.  Create a unique id if one does not exist and try to connect to the server
	/// all login fails should fail without asking the user for credentials.
	/// </summary>
	public bool silentSignin;
	
	public bool webGuestAutoLogin; //auto login as the guest account in the web build
	
	/// <summary>
	/// The register url
	/// </summary>
	private string registerURL;
	private string registerPath = "/users.json";
	
	/// <summary>
	/// The login url
	/// </summary>
	private string loginURL;
	private string loginPath = "/users/authenticate_for_token.json";
	
	/// <summary>
	/// The validate token url
	/// </summary>
	private string validateURL;
	private string validatePath = "?";
		
	/// <summary>
	/// The background image for the login
	/// </summary>
	public GUITexture background;
	
	/// <summary>
	/// The login name
	/// </summary>
	private string loginName = "";
	
	/// <summary>
	/// The login password
	/// </summary>
	private string loginPassword = "";
	private string loginPasswordConfirm = "";
	
	/// <summary>
	/// Super User mode for debugging purposes
	/// </summary>
	//private bool _isSuperUser = false;
	
	private enum AuthStatus
	{
		NO_NET,
		FIND_PREVIOUS_TOKEN,
		VALIDATE_TOKEN,
		WAIT_FOR_VALIDATION,
		SHOW_LOGIN,
		SHOW_REGISTRATION,
		AUTHENTICATION_COMPLETE,
		ERROR,
		LOADING
		
	}
	
	private AuthStatus state;
	
	
	/// <summary>
	/// The status of the login
	/// </summary>
	private string status = "";
	
	/// <summary>
	/// Is there an auth_token
	/// </summary>
	private bool hasToken;
	
	/// <summary>
	/// Is the auth_token valid
	/// </summary>
	private bool isValid;
		
	private Rect _rectLoginControlsBG;
	private Rect _rectLoginControls;
	private Rect _rectLoginControlsiPhone;
	//private GUIStyle styleLabelCenter;
	private Vector2 worldScroll;
	private int selGridIndex;
	
	//private WorldsData worldsJson;
	//private PlayersData playersData;
	
	/// <summary>
	/// The prefab containing all of the GUIStyles
	/// </summary>
	public GUIStyles styles;
	
	/// <summary>
	/// the style to use for boxes throughout the gui
	/// </summary>
	private GUIStyle _styleBox;
	
	/// <summary>
	/// singleton
	/// </summary>
	public static LoginGUI use = null;
	
	private bool isLoginUIUp;
	
	
	//have we initialized
	private bool initalized = false;
	
	/// <summary>
	/// Initialize
	/// </summary>
	void Awake()
	{
		
		// store singleton
		if (use == null) use = this;
		else if(GameObject.FindGameObjectsWithTag("ADA").Length > 1)
		{
			GameObject.Destroy(gameObject);
		}
		DontDestroyOnLoad(this);
		
	}
	
	void UpdateLinks()
	{
		// update the urls
		registerURL = serverURL + registerPath;
		loginURL = serverURL + loginPath;
		validateURL = serverURL + validatePath;
		
	}
	
	void Start()
	{	
		if(initalized)
		{
			return;	
		}
		
		initalized = true;
		
		// the box style in which most items float
		_styleBox = styles.roundDarkBox;
		
		
		// create the location of the login GUI
		float width = Screen.width;
		float height = Screen.height/2f;
		GUIStyle styleDarkBox = styles.sqWhiteBox;
		_rectLoginControlsBG = new Rect(
			0, 
		    0,
			width+styleDarkBox.padding.left+styleDarkBox.padding.right,
			height+styleDarkBox.padding.top+styleDarkBox.padding.bottom);
		_rectLoginControls = _rectLoginControlsBG;
		_rectLoginControls.x += styleDarkBox.padding.left;
		_rectLoginControls.y += styleDarkBox.padding.top;
		_rectLoginControls.width -= styleDarkBox.padding.left+styleDarkBox.padding.right;
		_rectLoginControls.height -= styleDarkBox.padding.top+styleDarkBox.padding.bottom;
		
		_rectLoginControlsiPhone = _rectLoginControlsBG;
		_rectLoginControlsiPhone.x += styleDarkBox.padding.left;
		_rectLoginControlsiPhone.y = styleDarkBox.padding.top;
		_rectLoginControlsiPhone.width -= styleDarkBox.padding.left+styleDarkBox.padding.right;
		_rectLoginControlsiPhone.height -= styleDarkBox.padding.top+styleDarkBox.padding.bottom;


		
		
		UserManager.userInfo = new UserData();
		UserManager.isOnline = false;
		UserManager.isLoggedIn = false;
		state = AuthStatus.FIND_PREVIOUS_TOKEN;
		//Check for pre-saved auth_token in the player preferences and use if present
		if(!serialNumberLogin && !silentSignin )
		{
			if(PlayerPrefs.HasKey("Auth_token") )
			{
				UserManager.userInfo.auth_token = PlayerPrefs.GetString("Auth_token");
				UserManager.playerName = PlayerPrefs.GetString("player_name");
				state = AuthStatus.VALIDATE_TOKEN;
			}
		}
		
		
		if(Application.isWebPlayer && state == AuthStatus.FIND_PREVIOUS_TOKEN)
		{
			//If we are in the webplayer check for an Auth_token that was passed in through javascript
			state= AuthStatus.SHOW_LOGIN;
			if(webGuestAutoLogin)
			{
				loginName = "guest@ada.dev.mirerca.com";
				loginPassword = "eria123";
				UpdateLinks();
				StartCoroutine(Login());
				
			}else{
				
				Application.ExternalCall("GetAuthToken");
				
			}
		}
		
		if(!Application.isWebPlayer && state == AuthStatus.FIND_PREVIOUS_TOKEN)
		{
			state = AuthStatus.SHOW_LOGIN;	
			
		}
		
		//if we are on an iPhone check for a network connection
#if UNITY_IPHONE
		if(Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork && !serialNumberLogin)
		{
			state = AuthStatus.NO_NET;	
			UserManager.isLoggedIn = true;
		}
#else
		//Check for valid ip address
		if(Network.player.ipAddress.ToString() == "127.0.0.1")
		{
			state = AuthStatus.NO_NET;
			UserManager.isLoggedIn = true;
		}
#endif
		
	
	 	UserManager.session_token = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
		//state = LoginGUI.AuthStatus.NO_NET;
		
		if(silentSignin && state != AuthStatus.NO_NET)
		{
			if(PlayerPrefs.HasKey("silent_signin_token") )
			{
				
				loginName = PlayerPrefs.GetString("silent_signin_token") + "@ada.dev.mirerca.com";
				loginPassword = PlayerPrefs.GetString("silent_signin_token");
				DebugEx.Log("using previous silent signin" + loginName);
				UpdateLinks();
				StartCoroutine(Login());
			}
			else
			{
				PlayerPrefs.SetString("silent_signin_token", UserManager.session_token);
				loginName = PlayerPrefs.GetString("silent_signin_token") + "@ada.dev.mirerca.com";
				loginPassword = PlayerPrefs.GetString("silent_signin_token");
				loginPasswordConfirm = PlayerPrefs.GetString("silent_signin_token");
				DebugEx.Log("creating silent signin" + loginName);
				UpdateLinks();
				StartCoroutine(Registration());
				
			}
				
		}
		
		
	}
	
	/// <summary>
	/// Used for the webplayer authentication.  This will get called from the browsers javascript
	/// </summary>
	/// <param name="incoming">
	/// A <see cref="System.String"/>
	/// </param>
	public void ListenForToken(string incoming){
		if(incoming != "invalid")
		{
			UserManager.userInfo.auth_token = incoming;
			state = AuthStatus.VALIDATE_TOKEN;
		}
	}
		
	public bool IsUIOpen()
	{
		return (state != LoginGUI.AuthStatus.NO_NET && state != LoginGUI.AuthStatus.AUTHENTICATION_COMPLETE);
	}
	
	
	/// <summary>
	/// Draw the GUI
	/// </summary>
	void OnGUI()
	{	
		if (silentSignin) return;
		//We lost connection and need to re-authenticate
		if(state == AuthStatus.AUTHENTICATION_COMPLETE && !UserManager.isLoggedIn)
		{
			state = AuthStatus.SHOW_LOGIN;
		}
		
		if(state == AuthStatus.VALIDATE_TOKEN)
		{
			//For now lets just pretend it worked
			state = AuthStatus.AUTHENTICATION_COMPLETE;
			UserManager.isOnline = true;
			UserManager.isLoggedIn = true;
			
			//make the call to check if the auth_token we had found from a previous login is still valid
			//state = AuthStatus.WAIT_FOR_VALIDATION;
			//UpdateLinks();
			//StartCoroutine(ValidateToken());
		}
		
		if(state == AuthStatus.SHOW_LOGIN || state == AuthStatus.NO_NET || state == AuthStatus.SHOW_REGISTRATION)
		{
			DrawLogin();
		}
		
		
	}
	
	protected void	BeginLoginControlLayout() {
		bool	virtualKeyboardVisible = false;
#if UNITY_IPHONE
		virtualKeyboardVisible = iPhoneKeyboard.visible;
#endif
		//if(virtualKeyboardVisible)
		//{
		//	GUILayout.BeginArea(_rectLoginControlsiPhone, _styleBox);
		//}
		//else
		//{
			GUILayout.BeginArea(_rectLoginControls, _styleBox);
		//}
	}
	
	/// <summary>
	/// Draw the login GUI
	/// </summary>
	void DrawLogin()
	{
		
		// draw background texture
		GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), background.texture);
		
		
		// draw contents of login gui
		BeginLoginControlLayout();
		{
			// title
			//GUILayout.Label("LOGIN", styles.largeTextLight); //, _styles.styleLabelCenter);
			
			GUILayout.BeginHorizontal();
			GUILayout.Label(styles.eriaLogo, GUILayout.Width(_rectLoginControls.width/4f) );
			//GUILayout.EndHorizontal();
			
			
			if(state == AuthStatus.SHOW_LOGIN)
			{
				GUILayout.BeginVertical();
				if(serialNumberLogin)
				{
					DrawSerialLogin();	
				}
				else
				{
					DrawNormalLogin();
				}
				GUILayout.EndVertical();
			}
			else if( state == AuthStatus.SHOW_REGISTRATION)
			{
				GUILayout.BeginVertical();
				DrawRegistartion();
				GUILayout.EndVertical();
			}
			else
			{
				if(silentSignin)
				{
					UserManager.isOnline = false;
					UserManager.isLoggedIn = true;
					state = AuthStatus.AUTHENTICATION_COMPLETE;
					
				}
				else
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("Server can not be reached, please check your network connect.  Would you like to play offline?", styles.largeTextHighlighted);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Yes", styles.button, GUILayout.Width(150)))
					{
						UserManager.isOnline = false;
						UserManager.isLoggedIn = true;
						state = AuthStatus.AUTHENTICATION_COMPLETE;
					}
					if (GUILayout.Button("Exit", styles.button, GUILayout.Width(150)))
					{
						//Transition to the main game screen
						Application.Quit();
					}
					GUILayout.EndHorizontal();
				}
					
			}
			
		}
		GUILayout.EndArea();
		
		// process return key
		// NOTE: doesn't seem to work when textbox has focus
		//if (Event.current.Equals(Event.KeyboardEvent("return")))
		//{
		//	StartCoroutine(Login());
		//}
		
		
	}
	
	private void DrawNormalLogin()
	{
		
		GUILayout.Space(50);
		// username
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label("e-mail:", styles.largeTextLight, GUILayout.Width(60), GUILayout.Height(50));
			loginName = GUILayout.TextField(loginName, GUILayout.Width(300), GUILayout.Height(30));
		}
		GUILayout.EndHorizontal();
	
	
		// password
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label("password:", styles.largeTextLight, GUILayout.Width(60), GUILayout.Height(50));
			loginPassword = GUILayout.PasswordField(loginPassword, "*"[0], GUILayout.Width(300), GUILayout.Height(30));
		}
		GUILayout.EndHorizontal();
	
		// login buttons
		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Login", styles.button, GUILayout.Width(150)))
			{
				UpdateLinks();
				StartCoroutine(Login());
			}
			if (GUILayout.Button("Register", styles.button, GUILayout.Width(150)))
			{
				state = AuthStatus.SHOW_REGISTRATION;
				//UpdateLinks();
				//Register();
				return;
			}
		}
		GUILayout.EndHorizontal();

		// status of login
		GUILayout.Label(status, styles.largeTextHighlighted);
			
	}
	
	private void DrawSerialLogin()
	{
		GUILayout.Space(50);
		// username
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label("Serial Number:", styles.largeTextLight, GUILayout.Width(60), GUILayout.Height(50));
			loginName = GUILayout.TextField(loginName, GUILayout.Width(300), GUILayout.Height(30));
		}
		GUILayout.EndHorizontal();
	
		

		// login buttons
		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Login", styles.button, GUILayout.Width(150)))
			{
				UserManager.playerName = loginName;
				loginName += serialSuffix;
				loginPassword = serialPassword;
				UpdateLinks();
				if(Application.internetReachability == NetworkReachability.NotReachable)
				{
					state = AuthStatus.NO_NET;	
					UserManager.isLoggedIn = true;
					//if(PlayerPrefs.HasKey("Auth_token") )
					//{
					//	PlayerPrefs.DeleteKey("Auth_token");	
					//}
				}
				else
				{
					StartCoroutine(Login());
				}
			}
			if (GUILayout.Button("Registered User", styles.button, GUILayout.Width(150)))
			{
				if(PlayerPrefs.HasKey("Auth_token") )
				{
					UserManager.userInfo.auth_token = PlayerPrefs.GetString("Auth_token");
					UserManager.playerName = PlayerPrefs.GetString("player_name");
					state = AuthStatus.VALIDATE_TOKEN;
				}
				else
				{
					serialNumberLogin = false;	
				}
			}
			
		}
		GUILayout.EndHorizontal();

		// status of login
		GUILayout.Label(status, styles.largeTextHighlighted);
			
	}
	
	private void DrawRegistartion()
	{
		GUILayout.Space(50);
		// username
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label("e-mail:", styles.largeTextLight, GUILayout.Width(60), GUILayout.Height(50));
			loginName = GUILayout.TextField(loginName, GUILayout.Width(300), GUILayout.Height(30));
		}
		GUILayout.EndHorizontal();
	
	
		// password
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label("password:", styles.largeTextLight, GUILayout.Width(60), GUILayout.Height(50));
			loginPassword = GUILayout.PasswordField(loginPassword, "*"[0], GUILayout.Width(300), GUILayout.Height(30));
		}
		GUILayout.EndHorizontal();
		
		//confirm password
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label("confirm password:", styles.largeTextLight, GUILayout.Width(60), GUILayout.Height(50));
			loginPasswordConfirm = GUILayout.PasswordField(loginPasswordConfirm, "*"[0], GUILayout.Width(300), GUILayout.Height(30));
		}
		GUILayout.EndHorizontal();
	
		//button
		GUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("OK", styles.button, GUILayout.Width(150)))
			{
				UpdateLinks();
				StartCoroutine(Registration());
			}
			if (GUILayout.Button("Cancel", styles.button, GUILayout.Width(150)))
			{
				state = AuthStatus.SHOW_LOGIN;
			}
		}
		GUILayout.EndHorizontal();
		
		// status of login
		GUILayout.Label(status, styles.largeTextHighlighted);
	}
	
	/// <summary>
	/// Check to see if the auth_token from a previous login is still valid
	/// </summary>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	private IEnumerator ValidateToken()
	{
		WebMessage webMessage = new WebMessage();
		yield return StartCoroutine(webMessage.Post(validateURL, 
		                                            "auth_token", UserManager.userInfo.auth_token
		                                        	));
		if(webMessage.error == WebErrorCode.Error)
        {
            status = webMessage.www.error;
			state = AuthStatus.ERROR;
            yield break;
        }
	}
	
	
	
	/// <summary>
	/// Process the login button
	/// </summary>
	private IEnumerator Login()
	{  
		status = "Trying to login...";
		WebMessage webMessage = new WebMessage();
		yield return StartCoroutine(webMessage.Post(loginURL, 
		                                            "email", loginName,
		                                            "password", loginPassword));
		if(webMessage.error == WebErrorCode.Error)
        {
            status = webMessage.extendedError;
			//The server is down - off the user the option to play offline
			if(status.Contains("503"))
			{
				UserManager.isOnline = false;
				state = AuthStatus.NO_NET;
			}
			//if we are trying to sign in silently and get ANY error just abort the login
			if(silentSignin)
			{
				UserManager.isOnline = false;
				state = AuthStatus.NO_NET;
			}
            yield break;
        }
		
       		
		// deserialize the login JSON text
		//
		try 
		{
			DebugEx.Log(webMessage.www.text);
			UserManager.userInfo = JsonMapper.ToObject<UserData>(webMessage.www.text);
			UserManager.isOnline = true;
			UserManager.isLoggedIn = true;
			UserManager.playerName = loginName;
		}
		catch(JsonException)
		{
            if(webMessage.error == WebErrorCode.None)
			{
				UserManager.isOnline = false;
				state = AuthStatus.NO_NET;
				yield break;
			}
		}
		
	
		DebugEx.Log(webMessage.www.text);
		
       
		// login successfull so get the world list
        status = "";
		
		state = AuthStatus.AUTHENTICATION_COMPLETE;
		
		//save the auth_token for later
		PlayerPrefs.SetString("Auth_token", UserManager.userInfo.auth_token);
		PlayerPrefs.SetString("player_name", loginName);
		
		//Look for logs that were previously collected and push them to the database
		DataCollection.PushLocalToOnline();
		
		
		
	}
	
	/// <summary>
	/// Process the Registartion
	/// </summary>
	private IEnumerator Registration()
	{  
		status = "Trying to Register...";
		
		RegisterWrapper adaReg = new RegisterWrapper();
		adaReg.user = new RegisterUser();
		adaReg.user.email = loginName;
		adaReg.user.password = loginPassword;
		adaReg.user.password_confirmation = loginPasswordConfirm;
		string jsonData = JsonMapper.ToJson(adaReg);
		WebMessage webMessage = new WebMessage();
		yield return StartCoroutine(webMessage.Postjson(registerURL, 
		                                            jsonData));
		if(webMessage.error == WebErrorCode.Error)
        {
            status = webMessage.extendedError;
			//The server is down - off the user the option to play offline
			if(status.Contains("503"))
			{
				UserManager.isOnline = false;
				state = AuthStatus.NO_NET;
			}
			//if we are trying to sign in silently and get ANY error just abort the login
			if(silentSignin)
			{
				UserManager.isOnline = false;
				state = AuthStatus.NO_NET;
			}
            yield break;
        }
		
       		
		// deserialize the login JSON text
		//
		try 
		{
			DebugEx.Log(webMessage.www.text);
			RegistrationSuccess success = JsonMapper.ToObject<RegistrationSuccess>(webMessage.www.text);
			UserManager.userInfo.auth_token = success.authentication_token;
			UserManager.isOnline = true;
			UserManager.isLoggedIn = true;
			UserManager.playerName = success.email;
		}
		catch(JsonException)
		{
            if(webMessage.error == WebErrorCode.None)
			{
				
				try
				{
					RegistrationError error = JsonMapper.ToObject<RegistrationError>(webMessage.www.text);
					status = "";
					if(error.email != null)
					{
						foreach(string s in error.email)
						{
							status += "email: " + s + "\n";	
						}
					}
					if(error.password != null)
					{
						foreach(string s in error.password)
						{
							status += "password: " + s + "\n";	
						}
					}
					
				}
				catch(JsonException)
				{
					status = "network error";
					UserManager.isOnline = false;
					state = AuthStatus.NO_NET;
				}
				yield break;
			}
		}
		
	
		DebugEx.Log(webMessage.www.text);
		
       
		// login successfull so get the world list
        status = "";
		
		state = AuthStatus.AUTHENTICATION_COMPLETE;
		
		//save the auth_token for later
		PlayerPrefs.SetString("Auth_token", UserManager.userInfo.auth_token);
		PlayerPrefs.SetString("player_name", loginName);
		
		//Look for logs that were previously collected and push them to the database
		DataCollection.PushLocalToOnline();
		
		
		

		
		
	}
	
	/// <summary>
	/// Process the register button
	/// </summary>
	private void Register()
	{
		Application.OpenURL(registerURL);
	}
	
	/// <summary>
	/// Process the exit button
	/// </summary>
	private void Exit()
	{
		Application.Quit();
	}
	
	
}
