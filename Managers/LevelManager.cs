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
		set { currentLevel=value;}
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
	void Awake () {
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
			DestroyImmediate(this);
	}

}
