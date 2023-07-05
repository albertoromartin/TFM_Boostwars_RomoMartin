using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] weaponPrefabs;
    public bool occupied = false;
    [SerializeField]
    private int randomTime;
    public bool isNuke = false;
    private int playersNear = 0;
    public bool canPlayerSpawn = true;
    public bool forceNuke = false;

    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(randomStart());
    }

    private IEnumerator randomStart()
    {
        randomTime = Random.Range(20, 36);
        while (true)
        {
            yield return new WaitForSeconds(randomTime);
            if (!occupied)
            {
                int weaponsActive = 0;
                for(int i = 0; i < transform.parent.childCount; i++)
                {
                    if (transform.parent.GetChild(i).GetComponent<WeaponManager>().occupied)
                    {
                        weaponsActive++;
                    }
                }
                if(weaponsActive < 3)
                {
                    if (playersNear == 0)
                    {
                        spawnWeapon();
                    }
                }
            }
        }
    }

    private void spawnWeapon()
    {
        int v = 6;
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetChild(i).GetComponent<WeaponManager>().isNuke)
            {
                v--;
                break;
            }
        }
        int randomWeapon = Random.Range(0, v);
        if (forceNuke)
        {
            randomWeapon = v - 1;
        }
        switch (randomWeapon)
        {
            case 0:
                if(Random.Range(0, 100) > 55)
                {
                    print("Falle 0");
                    return;
                }
                break;
            case 1:
                if (Random.Range(0, 100) > 80)
                {
                    print("Falle 1");
                    return;
                }
                break;
            case 2:
                if (Random.Range(0, 100) > 70)
                {
                    print("Falle 2");
                    return;
                }
                break;
            case 3:
                if (Random.Range(0, 100) > 80)
                {
                    print("Falle 3");
                    return;
                }
                break;
            case 4:
                if (Random.Range(0, 100) > 75)
                {
                    print("Falle 4");
                    return;
                }
                break;
            case 5:
                isNuke = true;
                if (Random.Range(0, 100) > 25)
                {
                    print("Falle 5");
                    return;
                }
                break;
        }
        if(!(GameManager.instance.player.GetComponent<Player>().dead || GameManager.instance.cpu.GetComponent<Player>().dead))
        {
            AudioManager.instance.playSound("SpawnGun");
        }
        GameObject go = Instantiate(weaponPrefabs[randomWeapon], this.transform);
        occupied = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" || other.gameObject.tag == "CPU")
        {
            //print("entro");
            playersNear++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" || other.gameObject.tag == "CPU")
        {
            //print("salgo");
            playersNear--;
        }
    }

    public void gotWeapon()
    {
        isNuke = false;
        occupied = false;
    }
}
