using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DropDownController : MonoBehaviour {
	
	List<string> items = new List<string> ();
	public Dropdown dropDown;
	// Use this for initialization
	void Start () {
		PopulateList ();
	}
	void PopulateList(){
		switch (LevelManager.CurrentLevel) {
		case 1:
			PushItems (1);
			break;
		case 2:
			PushItems (2);
			break;
		case 3:
			PushItems (3);
			break;
		}
		dropDown.AddOptions (items);
	}
	public void OnValueChanged(int index){
		string selectedItem = items [index];
		switch (selectedItem) {
		case "Level 1":
			break;
		case "Level 2":
			break;
		case "Level 3":
			break;
		}
	}
	void PushItems(int n){
		for (int i = 1; i <= n; i++) {
			items.Add ("Level " + i);
		}
	}
}
