using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDamager : MonoBehaviour
{
    Projectile parent;
    internal int position;
    private void Start()
    {
        parent = transform.GetComponentInParent<Projectile>();
    }

    /**
     * <summary>
     * Si choca contra un jugador, le hace daño
     * </summary>
     */
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player" || collision.gameObject.tag == "CPU")
        {
            collision.gameObject.GetComponent<Player>().health -= parent.Damage[position];
        }
    }
}
