using UnityEngine;
using System.Collections;

public class Boss_weapon : Weapon {

    public Vector3 vector3;

    void Start()
    {
        GetComponent<Rigidbody>().AddRelativeForce(vector3);
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag.Equals("MAN") || other.collider.tag.Equals("WOMAN"))
        {
            other.collider.GetComponent<Character>().ByAttacked(1);
            Destroy(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
        
        
    }
}
