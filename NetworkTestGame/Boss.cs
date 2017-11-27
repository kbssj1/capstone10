using UnityEngine;
using System.Collections;

public class Boss : Character
{
    public GameObject Prefabs_Boss_waepon;

    void Update()
    {
        if(Random.value < 0.05)
        {
            if (PhotonInit.def == PhotonInit.NetDef.Server && PhotonNetwork.playerList.Length == 2)
            {
                Vector3 vec = new Vector3(Random.Range(-30.0f, -10.0f), 25.0f, Random.Range(-30.0f, -10.0f));
                
                GameObject.FindGameObjectWithTag("MAN").transform.GetComponent<Man>()
                    .pv.RPC("CreateAttack", PhotonTargets.Others, vec);
                    
                CreateAttack(vec);
            }
        }
    }

    public void CreateAttack(Vector3 vector3)
    {
        Instantiate(Prefabs_Boss_waepon, new Vector3(this.transform.position.x, this.transform.position.y + 2.0f, this.transform.position.z), Quaternion.identity);
        Prefabs_Boss_waepon.GetComponent<Boss_weapon>().vector3 = vector3;
    }
}
