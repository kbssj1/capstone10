using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {


    private int HP = 10;

    public void ByAttacked(int _damage)
    {
        HP -= _damage;
    }

    public int GetHp()
    {
        return HP;
    }
}
