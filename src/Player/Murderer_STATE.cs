using UnityEngine;
using System.Collections;
using System;
public class Murderer_STATE : MonoBehaviour {

	enum MurdererAIState{
		IDLE,
		PATROL,
		TRACE,
		ATTACK
	}
	public bool idleEnd;
	public Survivor Survivor;
	[SerializeField]
	MurdererAIState MurdererAISatetes;
	Murderer_AI MerdererAI;
    public Murder_Audio murder_Audio;
	void Initialize(){
		idleEnd = false;
	}
	void Awake(){
		Initialize ();
	}
    void Start () {
       
		MurdererAISatetes = MurdererAIState.IDLE;
		MerdererAI = GetComponent<Murderer_AI> ();
        StartCoroutine(WaitTime());
        
	}
    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(3.0f);
        EventManager.Instance.PostNotification(EVENT_TYPE.COUNT_DOWN, this, true);
    }
    public void RealStart()
    {
        StartCoroutine(StateChanger());
    }
    
	void Update(){
		try{
			if(Survivor == null){
				Survivor = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Survivor>();
			}
		}
		catch(Exception e){
			Debug.LogError (e);
		}
	
    }
    IEnumerator StateChanger(){
		while (true) {
			

			if (Survivor != null) {
				
				if (Vector3.Distance (transform.position, Survivor.transform.position) < 2.0f) {
					MurdererAISatetes = MurdererAIState.ATTACK;
					MerdererAI.StopAIRoutine ();
					MerdererAI.Attack (Survivor.transform);
				}
				if (MurdererAISatetes == MurdererAIState.IDLE && idleEnd) {
					idleEnd = false;
					MurdererAISatetes = MurdererAIState.PATROL;
					MerdererAI.StopAIRoutine ();
					MerdererAI.Patrol ();
				}
				if (MurdererAISatetes == MurdererAIState.PATROL && Vector3.Distance (transform.position, MerdererAI.CurrentPatrolPosition.position) < 0.5f) {
					MurdererAISatetes = MurdererAIState.IDLE;
					MerdererAI.StopAIRoutine ();
					MerdererAI.Stop ();
				}
				if (MurdererAISatetes == MurdererAIState.PATROL && (Survivor.Playerstate == Survivor.PlayerState.Run || Survivor.Playerstate == Survivor.PlayerState.Gram
					|| Survivor.Playerstate == Survivor.PlayerState.Radio || Survivor.Playerstate == Survivor.PlayerState.Key)) {
					MurdererAISatetes = MurdererAIState.TRACE;
					MerdererAI.StopAIRoutine ();
					MerdererAI.TracePosition.position = Survivor.transform.position;
					MerdererAI.Trace ();
				}
				if (MurdererAISatetes == MurdererAIState.IDLE && (Survivor.Playerstate == Survivor.PlayerState.Run || Survivor.Playerstate == Survivor.PlayerState.Gram
					|| Survivor.Playerstate == Survivor.PlayerState.Radio || Survivor.Playerstate == Survivor.PlayerState.Key)) {
					MurdererAISatetes = MurdererAIState.TRACE;
					MerdererAI.StopAIRoutine ();
					MerdererAI.TracePosition.position = Survivor.transform.position;
					MerdererAI.Trace ();
				}
				if (MurdererAISatetes == MurdererAIState.TRACE && Vector3.Distance (this.transform.position, MerdererAI.TracePosition.position) < 2f) {
					MurdererAISatetes = MurdererAIState.IDLE;
					MerdererAI.StopAIRoutine ();
					MerdererAI.Stop ();
				}
			}

			yield return null;

		}
	}
    public void AllStop()
    {
        StopAllCoroutines();
    }
	public void OnIdleEnd(){
		Debug.Log ("IdleEnd");
		idleEnd = true;
	}
	public void OnShoutEnd(){
		Debug.Log ("ShoutEnd");
		MurdererAISatetes = MurdererAIState.IDLE;
		MerdererAI.attacking = false;
	}
}
