using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// an enum to describe the current visibility mode for Megatiles
/// </summary>
public enum WebErrorCode { None, Error }

/// <summary>
/// class used to send a URL post 
/// </summary>
public class WebMessage : System.Object
{
	/// <summary>
	/// error number of the last send message 
	/// </summary>
	public WebErrorCode error { get; set; }
	
	/// <summary>
	/// the string either returned from www or our server explaining the error 
	/// </summary>
	public string extendedError { get; set; }
	
	/// <summary>
	/// the WWW object associated with the WebMessage 
	/// </summary>
	public WWW www { get; private set; }
	
	/// <summary>
	/// Post a message 
	/// If the start of a value start with '{', then no '"' are added between the value
	/// </summary>
	/// <param name="url">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="fields">
	/// A <see cref="System.String[]"/>
	/// </param>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	public IEnumerator Post(string url, params string[] fields)
	{
		error = WebErrorCode.None;
		
		// create the form
		string json = "{";
		string fieldName, fieldValue;
		for(int i = 0; i < fields.Length;  )
		{
			fieldName = fields[i];
			fieldValue = fields[i+1];
			json += "\"" +  fieldName + "\"" + ":";
			
			if(fieldValue.StartsWith("{"))
				json += fieldValue.Substring(1);
			else
				json += "\"" + fieldValue + "\"";
			
			i = i + 2;
			if(i < fields.Length)
				json += ",";
		}
		json += "}";

		// can add headers to post
		Hashtable headers = new Hashtable();
		UTF8Encoding utf8 = new UTF8Encoding();
		byte[] rawData = utf8.GetBytes(json);
		headers["Content-Type"] = "application/jsonrequest";
		//headers["Content-Length"] = json.Length;
		www = new WWW(url, rawData, headers);
		
		yield return www;
		
		Debug.Log(www.error);
		if(www.error != null)
		{
			error = WebErrorCode.Error;
			extendedError = www.error;
			yield break;
		}
		
		// there was no error for www check for an error reported from our ADA server
		
		try 
		{
			
			ErrorData jerror = JsonMapper.ToObject<ErrorData>(www.text);
			extendedError = jerror.error;
			error = WebErrorCode.Error;
			
		}
		catch(JsonException)
		{
         	
		}
		
	}
	
	/// <summary>
	///Post json data without authentication
	/// </summary>
	/// <param name="url">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="jsonData">
	/// A <see cref="System.String"/>
	/// </param>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	public IEnumerator Postjson(string url, string jsonData)
	{
		error = WebErrorCode.None;
		
		// can add headers to post
		Debug.Log(jsonData);
		Hashtable headers = new Hashtable();
		UTF8Encoding utf8 = new UTF8Encoding();
		byte[] rawData = utf8.GetBytes(jsonData);
		headers["Content-Type"] = "application/jsonrequest";
		www = new WWW(url, rawData, headers);
		
		yield return www;
		
		Debug.Log("finished json post: " + www.error);
		if(www.error != null)
		{
			error = WebErrorCode.Error;
			extendedError = www.error;
			yield break;
		}
		
		// there was no error for www check for an error reported from our ADA server
		
		try 
		{
			
			ErrorData jerror = JsonMapper.ToObject<ErrorData>(www.text);
			extendedError = jerror.error;
			error = WebErrorCode.Error;
			
		}
		catch(JsonException)
		{
         	
		}
		
		
	}
	
	/// <summary>
	///Post an authenticated message to ADA.  All posts to ADA will includ the auth_token and some json data. 
	/// </summary>
	/// <param name="url">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="auth_token">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="jsonData">
	/// A <see cref="System.String"/>
	/// </param>
	/// <returns>
	/// A <see cref="IEnumerator"/>
	/// </returns>
	public IEnumerator PostAuthenticated(string url, string auth_token, string jsonData)
	{
		error = WebErrorCode.None;
		
		// can add headers to post
		Hashtable headers = new Hashtable();
		UTF8Encoding utf8 = new UTF8Encoding();
		byte[] rawData = utf8.GetBytes(jsonData);
		headers["Content-Type"] = "application/jsonrequest";
		www = new WWW(url+"?auth_token="+auth_token.ToString(), rawData, headers);
		
		yield return www;
		
		Debug.Log("finished authenticated post: " + www.error);
		if(www.error != null)
		{
			error = WebErrorCode.Error;
			extendedError = www.error;
			yield break;
		}
		
		// there was no error for www check for an error reported from our ADA server
		
		try 
		{
			
			ErrorData jerror = JsonMapper.ToObject<ErrorData>(www.text);
			extendedError = jerror.error;
			error = WebErrorCode.Error;
			
		}
		catch(JsonException)
		{
         	
		}
		
		
	}
	
}
	
