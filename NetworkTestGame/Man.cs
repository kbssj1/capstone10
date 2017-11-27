using UnityEngine;
using System.Collections;

public class Man : Hero
{
    void Awake()
    {
        Init();
        GameObject.FindGameObjectWithTag("GameController").GetComponent<MyGameManager>().man = this;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void CreateAttack(Vector3 vec3)
    {
        boss.CreateAttack(vec3);
    }
}
