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
		set { 
			 if ( value > 3 ) value = 3;
			 else if ( value < 1) value = 1;
			 currentLevel = value;
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
	int level1Damage = 10;
	float level1Speed = 3.0f;
	float level1SurvivorWalkSpeed = 1.5f;
	float level1SurvivorRunSpeed = 3.0f;
	float level1SurvivorGuageSpeed = 1.0f;

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
			return level1Time-150f;
		case 3:
			return level1Time-300f;
		default:
			return 120f;
		}
	}
	public int SetMurderDamageByLevel(){
		switch (currentLevel) {
		case 1:
			return level1Damage;
		case 2:
			return level1Damage+5;
		case 3:
			return level1Damage+10;
		default:
			return 25;
		}
	}
	public float SetMurdererSpeedByLevel(){
		switch (currentLevel) {
		case 1:
			return level1Speed;
		case 2:
			return level1Speed+0.5f;
		case 3:
			return level1Speed-0.5f;
		default:
			return 3.5f;
		}
	}
	public float SetSurvivorWalkSpeedByLevel(){
		switch (currentLevel) {
		case 1:
			return level1SurvivorWalkSpeed;
		case 2:
			return level1SurvivorWalkSpeed+0.5f;
		case 3:
			return level1SurvivorWalkSpeed*2f;
		default:
			return 1.5f;
		}
	}
	public float SetSurvivorRunSpeedByLevel(){
		switch (currentLevel) {
		case 1:
			return level1SurvivorRunSpeed;
		case 2:
			return level1SurvivorRunSpeed+0.8f;
		case 3:
			return level1SurvivorRunSpeed+1.8f;
		default:
			return 1.5f;
		}
	}
	public float SetSurvivorGuageSpeedByLevel(){
		switch (currentLevel) {
		case 1:
			return level1SurvivorGuageSpeed;
		case 2:
			return level1SurvivorGuageSpeed+0.5f;
		case 3:
			return level1SurvivorGuageSpeed+1.0f;
		default:
			return 1.0f;
		}
	}
}
