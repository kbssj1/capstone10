using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public class Murderer_AI : MonoBehaviour, IListener {

    private const int murderAI_damage = 25;
    private const string attackType1 = "Attack1";
    private const string attackType2 = "Attack2";
    private const string attackType3 = "Attack3";
    [SerializeField]
	//Transform[] patrolPos;
	List<Transform> patrolPos;
	NavMeshAgent naviAgnt;
	Animator animator;
	private bool isAttacking = false;
	public Transform currentPatPos;
    [SerializeField]
    private Transform tracePos;
    private bool attack;
    private int damage;
    private Murderer_STATE murder_state;
	void Awake(){
		Transform[] obj;
		Debug.Log (transform.name);
		obj = transform.parent.FindChild ("AI_Patrol_pos").transform.GetComponentsInChildren<Transform> ();
		for (int i = 1; i < obj.Length-1; i++) {
			patrolPos.Add (obj [i]);
		}
		tracePos = obj [obj.Length - 1];
	}
    // Use this for initialization
    void Start () {
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_OVER, this);
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_DIE, this);
        murder_state = GetComponent<Murderer_STATE>();
        attack = false;
        damage = murderAI_damage;
        naviAgnt = GetComponent<NavMeshAgent> ();
        animator = GetComponent<Animator> ();
	}
	IEnumerator Walk(){

        naviAgnt.Stop ();
        naviAgnt.Resume();
        animator.SetTrigger ("Walk");

        int index = (int)Random.Range (0, patrolPos.Count);
		currentPatPos = patrolPos [index];
        naviAgnt.SetDestination (patrolPos [index].position);

        yield return null;
	}
	IEnumerator Run(){
        naviAgnt.Stop ();
        naviAgnt.Resume();
        animator.SetTrigger ("Run");
        naviAgnt.SetDestination (tracePos.position);

        yield return null;
	}
	IEnumerator Attack1(){
        naviAgnt.Stop ();
        animator.SetTrigger (attackType1);
		yield return null;
	}
	IEnumerator Attack2(){
        naviAgnt.Stop ();
        animator.SetTrigger (attackType2);
		yield return null;
	}
	IEnumerator Attack3(){
        naviAgnt.Stop ();
        animator.SetTrigger (attackType3);
		yield return null;
	}
	IEnumerator Idle(){
        naviAgnt.Stop ();
        animator.SetTrigger ("Idle");
		yield return null;

	}
	public void Patrol(){
		
		StartCoroutine (Walk ());
	}
	public void Trace(){
		StartCoroutine (Run ());
	}
	public void Attack(Transform _survivor){
        
		if (!isAttacking) {
			isAttacking = true;
            int tmpRandomAttackIndex = (int)Random.Range (1, 3);
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
    public Animator IsAni()
    {
        return animator;
    }
    public bool getAttacked()
    {
        return attack;
    }
    public void OnAttackEnd()
    {

        attack = false;
    }
    public int getDamage()
    {
        return damage;
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
        attack = true;
    }
    public Transform GetTracePos()
    {
        return tracePos;
    }
	public Transform GetCurrentPosition(){
		return currentPatPos;
	}
    public void SetisAttacking(bool para)
    {
        isAttacking = para;
    }
    void OnTimmerEnd()
    {
        StopAIRoutine();
        Stop();
        murder_state.AllStop();
    }
    
}
