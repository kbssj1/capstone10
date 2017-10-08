using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Murderer_AI : MonoBehaviour, IListener {
	[SerializeField]
	Transform[] patrolPos;

	NavMeshAgent _naviAgnt;
	Animator Animator;
	public bool Attacking = false;
	public Transform currentPatPos;
	public Transform tracePos;
    private bool Attack22;
    private int Damage;
    public Murderer_STATE murder_state;
    // Use this for initialization
    void Start () {
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_OVER, this);
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_DIE, this);
        
        Attack22 = false;
        Damage = 25;
		_naviAgnt = GetComponent<NavMeshAgent> ();
		Animator = GetComponent<Animator> ();
	}
	IEnumerator Walk(){

		_naviAgnt.Stop ();
		_naviAgnt.Resume();
		Animator.SetTrigger ("Walk");

        int index = (int)Random.Range (0, patrolPos.Length);
		currentPatPos = patrolPos [index];
		_naviAgnt.SetDestination (patrolPos [index].position);

        yield return null;
	}
	IEnumerator Run(){
		_naviAgnt.Stop ();
		_naviAgnt.Resume();
		Animator.SetTrigger ("Run");


		_naviAgnt.SetDestination (tracePos.position);

        yield return null;
	}
	IEnumerator Attack1(){
		_naviAgnt.Stop ();
		Animator.SetTrigger ("Attack1");
		yield return null;
	}
	IEnumerator Attack2(){
		_naviAgnt.Stop ();
		Animator.SetTrigger ("Attack2");
		yield return null;
	}
	IEnumerator Attack3(){
		_naviAgnt.Stop ();
		Animator.SetTrigger ("Attack3");
		yield return null;
	}
	IEnumerator Idle(){
		_naviAgnt.Stop ();
		Animator.SetTrigger ("Idle");
		yield return null;

	}
	public void Patrol(){
		
		StartCoroutine (Walk ());
	}
	public void Trace(){
		StartCoroutine (Run ());
	}
	public void Attack(Transform _survivor){
        
		if (!Attacking) {
			Attacking = true;
            int tmpRandomAttackIndex = (int)Random.Range (1, 4);
			this.transform.LookAt (_survivor.position);
			StartCoroutine ("Attack" + tmpRandomAttackIndex);
		}
	}
	public void Stop(){
		
		StartCoroutine (Idle ());
	}
	public void StopAIRoutine(){
		
		StopAllCoroutines ();
	}
    public Animator GetAni()
    {
        return Animator;
    }
    public bool getAttacked()
    {
        return Attack22;
    }
    public void OnAttackEnd()
    {

        Attack22 = false;
    }
    public int getDamage()
    {
        return Damage;
    }
    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.TIME_OVER:
                OnTimmerEnd();
                break;
            case EVENT_TYPE.TIME_START:
                murder_state.RealStart();
                break;
            case EVENT_TYPE.SURVIVOR_DIE:
                StopAllCoroutines();
                murder_state.AllStop();
                break;

        };
    }
    public void OnAttackStart()
    {
        Attack22 = true;
    }
    void OnTimmerEnd()
    {
        StopAIRoutine();
        Stop();
        murder_state.AllStop();
    }
    
}
