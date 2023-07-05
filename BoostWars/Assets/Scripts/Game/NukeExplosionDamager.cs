using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeExplosionDamager : MonoBehaviour
{
    NukeManager parent;
    internal int position;
    private void Start()
    {
        parent = transform.GetComponentInParent<NukeManager>();
    }

    /**
     * <summary>
     * Si choca contra un jugador, le hace da�o
     * </summary>
     */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "CPU")
        {
            print("Da�o a " + collision.gameObject.tag + " -> " + parent.Damage[position] + " (" + parent.name + ")");
            collision.gameObject.GetComponent<Player>().health -= parent.Damage[position];
        }
    }
}
