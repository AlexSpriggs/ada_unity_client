using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameInfo
{
	public static string GAME_NAME = "KrystalsOfKaydor";
	public static string SCHEMA = "DEVELOPMENT-01-28-2013";

}

public class KoKQuestStart : ADAUnitStart {}

public class KoKQuestEnd : ADAUnitEnd {}

public class KoKObjectiveStart : ADAUnitStart {}


public class KoKObjectiveEnd : ADAUnitEnd {}

public class KoKQuitGame : ADAPlayerAction {}

public class KoKNewGame : ADAGameEvent {}
public class KoKContinueGame : ADAGameEvent {}


public class KoKPlayerMovement : ADAPlayerAction {}


public class KoKEmotionChoice : ADAPlayerChoice {
	public string questName;	
}


public class KoKEmotionCalibration : ADAPlayerAction
{
	public string questName;
	public double correlation;  					//What the correlation to the recording was
	public bool success; 						//Did the game consider this successful
	public List<double> calibration_values; 	//The raw calibration values
	public string emotion;						//the emotion you are calibrating 
}

public class KoKConversationChoice : ADAPlayerChoice {
	public string questName;
}

public class KoKTool : ADAPlayerAction
{
	public List<string> object_names; //what objects did they use the tool on. This may be empty
	public KoKTool() 
	{
		object_names = new List<string>();
	}
}

public class KoKBehavior : ADAPlayerAction
{
	public float duration;

	public void start_timer()
	{
		duration = Time.realtimeSinceStartup;
	}

	public void stop_timer()
	{
		duration = Time.realtimeSinceStartup - duration;
	}
}

public class KoKPickup : ADAPlayerAction
{
	public string item_name;
}

public class KoKScan : KoKTool {}

public class KoKAbility : KoKTool 
{
	public string ability_name;
}

public class KoKCloak : KoKBehavior {}

public class KoKHover : KoKBehavior {}



public class KoKAlmanacEntryUnlock : ADAGameEvent 
{
	public string entry_id;
}

public class KoKActorStartAnimation : ADAGameEvent
{
	public string actor_name;
	public string animation_name;
}

public class KoKReputationChange : ADAGameEvent 
{
	public float score;
}

public class KoKHealthChange : ADAPlayerAction
{
	public float score;
}

