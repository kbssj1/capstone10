using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_AI : MonoBehaviour {

    public Murderer_AI murderer;
    public Attack_Audio attack_audio;

    private bool tmp = true;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("SURVIVOR") && tmp)
        {
           
            if (murderer.getAttacked())
            {
                print("OnTriger");
                StartCoroutine(Block2Attack());
                EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_HIT, this, murderer.getDamage());
            }
        }

    }

    void Start()
    {
        attack_audio.PlayAudio("START", true);
    }


    public void PlayNormalAudio()
    {
        if (attack_audio.GetCheck())
            attack_audio.PlayAudio("NORMAL");
    }

    protected IEnumerator Block2Attack()
    {
        tmp = false;

        yield return new WaitForSeconds(2.0f);

        tmp = true;
    }
}
