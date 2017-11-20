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
	MurdererAIState _state;
	Murderer_AI _ai;
	public Survivor _survivor = null;
	public bool isIdleEnd = false;

    public Murder_Audio murder_Audio;

    void Start () {
       
        _state = MurdererAIState.IDLE;
		_ai = GetComponent<Murderer_AI> ();
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
			if(_survivor == null){
			_survivor = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Survivor>();
			}
		}
		catch(Exception e){
			Debug.LogError (e);
		}
    }
    IEnumerator StateChanger(){
		while (true) {
			

			if (_survivor != null) {
				//Debug.Log (Vector3.Distance (transform.position, _survivor.transform.position));
				if (Vector3.Distance (transform.position, _survivor.transform.position) < 2.0f) {
					_state = MurdererAIState.ATTACK;
					_ai.StopAIRoutine ();
					_ai.Attack (_survivor.transform);
				}
				if (_state == MurdererAIState.IDLE && isIdleEnd) {
					isIdleEnd = false;
					_state = MurdererAIState.PATROL;
					_ai.StopAIRoutine ();
					_ai.Patrol ();
				}
				if (_state == MurdererAIState.PATROL && Vector3.Distance (transform.position, _ai.currentPatPos.position) < 0.5f) {
					_state = MurdererAIState.IDLE;
					_ai.StopAIRoutine ();
					_ai.Stop ();
				}
				if (_state == MurdererAIState.PATROL && (_survivor.Playerstate == Survivor.PlayerState.Run || _survivor.Playerstate == Survivor.PlayerState.Gram
                    || _survivor.Playerstate == Survivor.PlayerState.Radio || _survivor.Playerstate == Survivor.PlayerState.Key)) {
					_state = MurdererAIState.TRACE;
					_ai.StopAIRoutine ();
					_ai.tracePos.position = _survivor.transform.position;
					_ai.Trace ();
				}
				if (_state == MurdererAIState.IDLE && (_survivor.Playerstate == Survivor.PlayerState.Run || _survivor.Playerstate == Survivor.PlayerState.Gram
                    || _survivor.Playerstate == Survivor.PlayerState.Radio || _survivor.Playerstate == Survivor.PlayerState.Key)) {
					_state = MurdererAIState.TRACE;
					_ai.StopAIRoutine ();
					_ai.tracePos.position = _survivor.transform.position;
					_ai.Trace ();
				}
				if (_state == MurdererAIState.TRACE && Vector3.Distance (this.transform.position, _ai.tracePos.position) < 2f) {
					_state = MurdererAIState.IDLE;
					_ai.StopAIRoutine ();
					_ai.Stop ();
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
		isIdleEnd = true;
	}
	public void OnShoutEnd(){
		_state = MurdererAIState.IDLE;
	}
}
