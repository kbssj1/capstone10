using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
public class LevelManager : MonoBehaviour {

	#region
	public static LevelManager Instance
	{
		get { return instance; }
		set { }
	}

	private static LevelManager instance = null;

	#endregion
	#region
	public static int CurrentLevel
	{
		get { return currentLevel; }
		set { if (value > 3) {
				currentLevel = 3;
			} else if (value < 1) {
				currentLevel = 1;
			} else {
				currentLevel = value;
			}
			}
	}
	[Header("LEVEL")]
	[SerializeField]
	private static int currentLevel = 1;

	#endregion
	#region
	public static string LevelKeyword
	{
		get { return levelKeyword; }
		set { }
	}

	private static string levelKeyword = "CurrentLevel";
	#endregion
	// Update is called once per frame
	float level1Time = 600f;
	float level2Time = 450f;
	float level3Time = 300f;
	int level1Damage = 10;
	int level2Damage = 15;
	int level3Damage = 20;
	float level1Speed = 3.0f;
	float level2Speed = 3.5f;
	float level3Speed = 2.5f;
	void Awake () {
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
			DestroyImmediate(this);
	}
	public float SetGameTimerByLevel(){
		switch (currentLevel) {
		case 1:
			return level1Time;
		case 2:
			return level2Time;
		case 3:
			return level3Time;
		default:
			return 120f;
		}
	}
	public int SetMurderDamageByLevel(){
		switch (currentLevel) {
		case 1:
			return level1Damage;
		case 2:
			return level2Damage;
		case 3:
			return level3Damage;
		default:
			return 25;
		}
	}
	public float SetMurdererSpeedByLevel(){
		switch (currentLevel) {
		case 1:
			return level1Speed;
		case 2:
			return level2Speed;
		case 3:
			return level3Speed;
		default:
			return 3.5f;
		}
	}
}
