using UnityEngine;
using System.Collections;

public class Hero : Character {

    protected Transform tr;
    private Rigidbody rbody;
    private float h, v;
    public PhotonView pv = null;
    private float MoveSpeed = 4.0f;
    protected Vector3 currPos = Vector3.zero;
    protected Quaternion currRot = Quaternion.identity;
    private Sword Weapon;
    protected Boss boss;

    protected void Init()
    {
        Weapon = transform.GetChild(0).GetComponent<Sword>();
        tr = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
        rbody = GetComponent<Rigidbody>();
        boss = GameObject.FindGameObjectWithTag("BOSS").GetComponent<Boss>();

        pv.synchronization = ViewSynchronization.UnreliableOnChange;
        pv.ObservedComponents[0] = this;

        if (pv.isMine)
        {
           
        }
        else
        {
            this.transform.GetChild(0).gameObject.GetComponent<CapsuleCollider>().enabled = false;
            rbody.isKinematic = true;
        }

        currPos = tr.position;
        currRot = tr.rotation;
    }

    void Update()
    {
        if (pv.isMine)
        {
            if (Input.GetKeyDown("space"))
            {
                EnterAttackKey();
                pv.RPC("EnterAttackKey", PhotonTargets.Others, null);
            }
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            tr.Rotate(Vector3.up * h * Time.deltaTime * MoveSpeed * 50);

            tr.Translate(Vector3.forward * v * Time.deltaTime * MoveSpeed);
            tr.Translate(Vector3.right * h * Time.deltaTime * MoveSpeed);
        }
        else
        {
            tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 3.0f);
            tr.rotation = Quaternion.Slerp(tr.rotation, currRot, Time.deltaTime * 3.0f);
        }
    }

    [PunRPC]
    public void EnterAttackKey()
    {
        Weapon.EnterAttackKey();
    }

    [PunRPC]
    public void BossisAttacked()
    {
        boss.ByAttacked(1);
    }

}
