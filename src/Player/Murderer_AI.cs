using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Murderer_AI : MonoBehaviour, IListener {
	
	private bool attacking;
    private bool Attack22;
    private int damage;
	[SerializeField]
	Transform[] PatrolPosition;
	[SerializeField]
	public Transform CurrentPatrolPosition;
	[SerializeField]
	public Transform TracePosition;
	NavMeshAgent NavMeshAgnt;
	Animator Animat;
	private string attackType;

    public Murderer_STATE murder_state;
    // Use this for initialization
	void Initialize(){
		attacking = false;
		attackType = "Attack1";
	}
	void Awake(){
		Initialize ();
	}
    void Start () {
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_OVER, this);
        EventManager.Instance.AddListener(EVENT_TYPE.TIME_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_DIE, this);
        
        Attack22 = false;
        damage = 25;
		NavMeshAgnt = GetComponent<NavMeshAgent> ();
		Animat = GetComponent<Animator> ();
	}
	IEnumerator Walk(){

		NavMeshAgnt.Stop ();
		NavMeshAgnt.Resume();
		Animat.SetTrigger ("Walk");
		int index = (int)Random.Range (0, PatrolPosition.Length);
		CurrentPatrolPosition = PatrolPosition [index];
		NavMeshAgnt.SetDestination (PatrolPosition [index].position);

        yield return null;
	}
	IEnumerator Run(){
		NavMeshAgnt.Stop ();
		NavMeshAgnt.Resume();
		Animat.SetTrigger ("Run");
		NavMeshAgnt.SetDestination (TracePosition.position);

        yield return null;
	}
	IEnumerator Attack1(){
		NavMeshAgnt.Stop ();
		Animat.SetTrigger (attackType);
		yield return null;
	}
	IEnumerator Attack2(){
		NavMeshAgnt.Stop ();
		Animat.SetTrigger (attackType);
		yield return null;
	}
	IEnumerator Attack3(){
		NavMeshAgnt.Stop ();
		Animat.SetTrigger (attackType);
		yield return null;
	}
	IEnumerator Idle(){
		NavMeshAgnt.Stop ();
		Animat.SetTrigger ("Idle");
		yield return null;

	}
	public void Patrol(){
		
		StartCoroutine (Walk ());
	}
	public void Trace(){
		StartCoroutine (Run ());
	}
	public void Attack(Transform _survivor){
        
		if (!attacking) {
            attacking = true;
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
		return Animat;
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
        Attack22 = true;
    }
    void OnTimmerEnd()
    {
        StopAIRoutine();
        Stop();
        murder_state.AllStop();
    }
    
}
