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
	[SerializeField]
	MurdererAIState state;
	Murderer_AI ai;
	public Survivor survivor = null;
	public bool idleEnd = false;

    public Murder_Audio murder_Audio;

    void Start () {

        state = MurdererAIState.IDLE;
        ai = GetComponent<Murderer_AI> ();
        StartCoroutine(WaitTime());
        
	}
    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(3.0f);
        print("countdown");
        EventManager.Instance.PostNotification(EVENT_TYPE.COUNT_DOWN, this, true);
    }
    public void RealStart()
    {
        StartCoroutine(StateChanger());
    }
    
	void Update(){
		try{
			if(survivor == null){
                survivor = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Survivor>();
			}
		}
		catch(Exception e){
			Debug.LogError (e);
		}
        #region

        if (ai.IsAni().GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            murder_Audio.PlayAudio("WALK");
        }
        else if (ai.IsAni().GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            murder_Audio.PlayAudio("RUN");
        }
        else
        {
            murder_Audio.PlayAudio("NOT");
        }

        #endregion
    }
    IEnumerator StateChanger(){
		while (true) {
			

			if (survivor != null) {
				//Debug.Log (Vector3.Distance (transform.position, _survivor.transform.position));
				if (Vector3.Distance (transform.position, survivor.transform.position) < 2.0f) {
                    state = MurdererAIState.ATTACK;
                    ai.StopAIRoutine ();
                    ai.Attack (survivor.transform);
				}
				if (state == MurdererAIState.IDLE && idleEnd) {
                    idleEnd = false;
                    state = MurdererAIState.PATROL;
                    ai.StopAIRoutine ();
                    ai.Patrol ();
				}
				if (state == MurdererAIState.PATROL && Vector3.Distance (transform.position, ai.GetCurrentPosition().position) < 0.5f) {
                    state = MurdererAIState.IDLE;
                    ai.StopAIRoutine ();
                    ai.Stop ();
				}
				if (state == MurdererAIState.PATROL && (survivor.playerState == Survivor.PlayerState.Run || survivor.playerState == Survivor.PlayerState.Gram
                    || survivor.playerState == Survivor.PlayerState.Radio || survivor.playerState == Survivor.PlayerState.Key)) {
                    state = MurdererAIState.TRACE;
                    ai.StopAIRoutine ();
                    ai.GetTracePos().position = survivor.transform.position;
                    ai.Trace ();
				}
				if (state == MurdererAIState.IDLE && (survivor.playerState == Survivor.PlayerState.Run || survivor.playerState == Survivor.PlayerState.Gram
                    || survivor.playerState == Survivor.PlayerState.Radio || survivor.playerState == Survivor.PlayerState.Key)) {
                    state = MurdererAIState.TRACE;
                    ai.StopAIRoutine ();
                    ai.GetTracePos().position = survivor.transform.position;
                    ai.Trace ();
				}
				if (state == MurdererAIState.TRACE && Vector3.Distance (this.transform.position, ai.GetTracePos().position) < 2f) {
                    state = MurdererAIState.IDLE;
                    ai.StopAIRoutine ();
                    ai.Stop ();
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
        state = MurdererAIState.IDLE;
        ai.SetisAttacking(false);
	}
}
