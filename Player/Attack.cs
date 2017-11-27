using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {

    public Murderer murderer;
    public Attack_Audio attack_audio;

    private bool tmp = true;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("SURVIVOR") && tmp && murderer.getAttacked())
        {          
            StartCoroutine(Block2Attack());
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_HIT, this, murderer.getDamage());   
        }

    }

    void Start()
    {
        attack_audio.PlayAudio("START",true);
    }


    public void PlayIdleWeaponAudio()
    {
        if (attack_audio.isAudioPlay())
            attack_audio.PlayAudio("NORMAL");
    }

    protected IEnumerator Block2Attack()
    {
        tmp = false;

        yield return new WaitForSeconds(2.0f);

        tmp = true;
    }
}
