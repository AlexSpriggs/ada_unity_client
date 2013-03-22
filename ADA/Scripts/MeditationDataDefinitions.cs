using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ADAConst {
	//TENConfigurationDatapack constants
	public const string kConfigServer = "Server";
	public const string kConfigPlayerPrefs = "PlayerPrefs";
	//TENTouchDatapack constants
	public const string kTouchValid = "ValidTouch";
	public const string kTouchInvalid = "InvalidTouch";
	public const string kTouchRecovery = "RecoveryTouch";
	public const string kTouchCycleComplete = "CycleCompleteTouch";
	public const string kTouchQuit = "QuitTouch";
	
}

public class GameInfo
{
	public static string GAME_NAME = "Tenacity-Meditation";
	public static string SCHEMA = "BETA-TESTING-3-14-2013";
	public static string PLAYERNAME = GameManager.currentPlayerName;
}


public class TenStageStart : ADAUnitStart {
	public string breathsPerCycle = "undefined";
	public string sessionTime = "undefined";
}

public class TenStageComplete :  ADAUnitEnd{	
	public string timeInLevel = "0";
}
	
public class TenBreathCycleStart : ADAUnitStart {}

public class TenBreathCycleEnd:  ADAUnitEnd{	
	public bool success = false;
}

public class TenTouchEvent : ADAPlayerAction{
	public string timeSinceLastTouch = "undefined";
	public string averageTouchTime = "undefined";
	public string touchType = "undefined";

}

public class TenSelfAssessment : ADAGameEvent{
	public float selfAssessmentValue = -1;
	public string isPrePost = "undefined";

}
	
public class TenPlayerSelect :  ADAGameEvent{
	public string selectedPlayerName = "";

}

public class TenGUIUse :  ADAGameEvent{
	public string uiAction = "";
}
	

public class TenTimeFromLastQuitSession : ADAGameEvent{
	public string timeInlevel = "0";
}

public class TenStagePackage : ADAGameEvent {
	public string sessionTime = "undefined";
	public string breathsPerCycle = "undefined";
}

public class TenErrorMessage : ADAGameEvent {
	public string error_message;
}
