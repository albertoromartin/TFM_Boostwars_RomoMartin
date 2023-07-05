using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsManager : MonoBehaviour
{

    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 1f, player.transform.position.z);
        if (player.GetComponent<Player>().health > 0 && !player.GetComponent<Player>().dead) {
            transform.GetChild(0).GetChild(1).gameObject.GetComponent<Slider>().value = player.GetComponent<Player>().health;
        }
        else if(!player.GetComponent<Player>().dead)
        {
            player.GetComponent<Player>().dead = true;
            transform.GetChild(0).GetChild(1).GetChild(1).gameObject.SetActive(false);
            player.GetComponent<Player>().Die();
        }
    }
}
