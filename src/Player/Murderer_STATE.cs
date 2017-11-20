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
	MurdererAIState MurdererAISatete;
	Murderer_AI MerdererAI;
    public Murder_Audio murder_Audio;
	void Initialize(){
		idleEnd = false;
	}
	void Awake(){
		Initialize ();
	}
    void Start () {
       
		MurdererAISatete = MurdererAIState.IDLE;
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
					MurdererAISatete = MurdererAIState.ATTACK;
					MerdererAI.StopAIRoutine ();
					MerdererAI.Attack (Survivor.transform);
				}
				if (MurdererAISatete == MurdererAIState.IDLE && idleEnd) {
					idleEnd = false;
					MurdererAISatete = MurdererAIState.PATROL;
					MerdererAI.StopAIRoutine ();
					MerdererAI.Patrol ();
				}
				if (MurdererAISatete == MurdererAIState.PATROL && Vector3.Distance (transform.position, MerdererAI.CurrentPatrolPosition.position) < 0.5f) {
					MurdererAISatete = MurdererAIState.IDLE;
					MerdererAI.StopAIRoutine ();
					MerdererAI.Stop ();
				}
				if (MurdererAISatete == MurdererAIState.PATROL && (Survivor.Playerstate == Survivor.PlayerState.Run || Survivor.Playerstate == Survivor.PlayerState.Gram
					|| Survivor.Playerstate == Survivor.PlayerState.Radio || Survivor.Playerstate == Survivor.PlayerState.Key)) {
					MurdererAISatete = MurdererAIState.TRACE;
					MerdererAI.StopAIRoutine ();
					MerdererAI.TracePosition.position = Survivor.transform.position;
					MerdererAI.Trace ();
				}
				if (MurdererAISatete == MurdererAIState.IDLE && (Survivor.Playerstate == Survivor.PlayerState.Run || Survivor.Playerstate == Survivor.PlayerState.Gram
					|| Survivor.Playerstate == Survivor.PlayerState.Radio || Survivor.Playerstate == Survivor.PlayerState.Key)) {
					MurdererAISatete = MurdererAIState.TRACE;
					MerdererAI.StopAIRoutine ();
					MerdererAI.TracePosition.position = Survivor.transform.position;
					MerdererAI.Trace ();
				}
				if (MurdererAISatete == MurdererAIState.TRACE && Vector3.Distance (this.transform.position, MerdererAI.TracePosition.position) < 2f) {
					MurdererAISatete = MurdererAIState.IDLE;
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
		idleEnd = true;
	}
	public void OnShoutEnd(){
		MurdererAISatete = MurdererAIState.IDLE;
	}
}
