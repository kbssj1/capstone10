using UnityEngine;
using System.Collections;

public class Sword : Weapon {

    private bool IsEnterAttackKeyboard = false;
    private Transform parent_tr;
    

    void Start()
    {
        parent_tr = transform.parent;
    }

    public void EnterAttackKey()
    {
        if (IsEnterAttackKeyboard == false)
            IsEnterAttackKeyboard = true;
    }

	void Update()
    {
        if(IsEnterAttackKeyboard == true)
        {
            this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x + 4.0f, parent_tr.transform.rotation.eulerAngles.y, 0.0f);

            if (80.0f <= this.transform.rotation.eulerAngles.x && this.transform.rotation.eulerAngles.x <= 90.0f)
                IsEnterAttackKeyboard = false;           
        }
        else
        {
            if (10.0f <= this.transform.rotation.eulerAngles.x && this.transform.rotation.eulerAngles.x <= 90.0f)
            {
                this.transform.rotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x - 4.0f, parent_tr.transform.rotation.eulerAngles.y, 0.0f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("BOSS"))
        {
            other.GetComponent<Character>().ByAttacked(1);
            parent_tr.GetComponent<Hero>().pv.RPC("BossisAttacked", PhotonTargets.Others, null);
        }
    }
}
