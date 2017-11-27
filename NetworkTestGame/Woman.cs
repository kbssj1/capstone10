using UnityEngine;
using System.Collections;

public class Woman : Hero
{

    void Awake()
    {
        Init();
        GameObject.FindGameObjectWithTag("GameController").GetComponent<MyGameManager>().woman = this;      
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

}
