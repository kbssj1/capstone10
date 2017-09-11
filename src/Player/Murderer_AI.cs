using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Murderer_AI : MonoBehaviour, IListener {
	[SerializeField]
	Transform[] patrolPos;

	NavMeshAgent _naviAgnt;
	Animator _animator;
	public bool isAttacking = false;
	public Transform currentPatPos;
	public Transform tracePos;
    private bool isAttack;
    private int m_Damage;
    public Murderer_STATE murder_state;
    // Use this for initialization
    void Start () {
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_OVER, this);
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_DIE, this);
        
        isAttack = false;
        m_Damage = 25;
		_naviAgnt = GetComponent<NavMeshAgent> ();
		_animator = GetComponent<Animator> ();
	}
	IEnumerator Walk(){

		_naviAgnt.Stop ();
		_naviAgnt.Resume();
		_animator.SetTrigger ("Walk");

        int index = (int)Random.Range (0, patrolPos.Length);
		currentPatPos = patrolPos [index];
		_naviAgnt.SetDestination (patrolPos [index].position);

        yield return null;
	}
	IEnumerator Run(){
		_naviAgnt.Stop ();
		_naviAgnt.Resume();
		_animator.SetTrigger ("Run");


		_naviAgnt.SetDestination (tracePos.position);

        yield return null;
	}
	IEnumerator Attack1(){
		_naviAgnt.Stop ();
		_animator.SetTrigger ("Attack1");
		yield return null;
	}
	IEnumerator Attack2(){
		_naviAgnt.Stop ();
		_animator.SetTrigger ("Attack2");
		yield return null;
	}
	IEnumerator Attack3(){
		_naviAgnt.Stop ();
		_animator.SetTrigger ("Attack3");
		yield return null;
	}
	IEnumerator Idle(){
		_naviAgnt.Stop ();
		_animator.SetTrigger ("Idle");
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
        return _animator;
    }
    public bool getAttacked()
    {
        return isAttack;
    }
    public void OnAttackEnd()
    {

        isAttack = false;
    }
    public int getDamage()
    {
        return m_Damage;
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
        isAttack = true;
    }
    void OnTimmerEnd()
    {
        StopAIRoutine();
        Stop();
        murder_state.AllStop();
    }
    
}
