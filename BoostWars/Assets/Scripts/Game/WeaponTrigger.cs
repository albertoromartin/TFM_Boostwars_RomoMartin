using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrigger : MonoBehaviour
{
    [SerializeField]
    private WeaponInfo localInfo;
    private void Start()
    {
        switch (gameObject.name)
        {
            case "bazooka(Clone)":
                localInfo = new WeaponInfo(transform.position, 4);
                break;
            case "escopeta(Clone)":
                localInfo = new WeaponInfo(transform.position, 1);
                break;
            case "miniCanyon(Clone)":
                localInfo = new WeaponInfo(transform.position, 3);
                break;
            case "pesada(Clone)":
                localInfo = new WeaponInfo(transform.position, 1);
                break;
            case "rifle(Clone)":
                localInfo = new WeaponInfo(transform.position, 2);
                break;
            case "miniNukeSprite(Clone)":
                localInfo = new WeaponInfo(transform.position, 5);
                break;
        }

        GameManager.instance.weaponList.Add(this.localInfo);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            switch (gameObject.name)
            {
                case "bazooka(Clone)":
                    GameManager.instance.actualWeaponPlayer = 0;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "escopeta(Clone)":
                    GameManager.instance.actualWeaponPlayer = 1;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "miniCanyon(Clone)":
                    GameManager.instance.actualWeaponPlayer = 2;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "pesada(Clone)":
                    GameManager.instance.actualWeaponPlayer = 3;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "rifle(Clone)":
                    GameManager.instance.actualWeaponPlayer = 4;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "miniNukeSprite(Clone)":
                    //GameManager.instance.actualWeaponPlayer = 5;
                    GameObject.Find("NukeCanvas").transform.GetChild(0).gameObject.SetActive(true);
                    AudioManager.instance.playSound("NukeAlarm");
                    GameManager.instance.nukeLock = true;
                    GameManager.instance.turn = 2;
                    //Enable camera before pick a nuke
                    GameManager.instance.modeButton.SetActive(false);
                    GameManager.instance.changeMode();
                    break;
            }
            GameManager.instance.weaponList.Remove(this.localInfo);
            GameManager.instance.updatePlayerArm();
            transform.parent.GetComponent<WeaponManager>().gotWeapon();
            GameObject.Destroy(this.gameObject);
            
        }
        else if(other.gameObject.tag == "CPU")
        {
            switch (gameObject.name)
            {
                case "bazooka(Clone)":
                    GameManager.instance.actualWeaponCpu = 0;
                    GameManager.instance.cpu.GetComponent<CpuManager>().weaponTier = 4;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "escopeta(Clone)":
                    GameManager.instance.actualWeaponCpu = 1;
                    GameManager.instance.cpu.GetComponent<CpuManager>().weaponTier = 1;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "miniCanyon(Clone)":
                    GameManager.instance.actualWeaponCpu = 2;
                    GameManager.instance.cpu.GetComponent<CpuManager>().weaponTier = 3;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "pesada(Clone)":
                    GameManager.instance.actualWeaponCpu = 3;
                    GameManager.instance.cpu.GetComponent<CpuManager>().weaponTier = 1;
                    AudioManager.instance.playSound("PickUp");
                    break;
                case "rifle(Clone)":
                    GameManager.instance.actualWeaponCpu = 4;
                    GameManager.instance.cpu.GetComponent<CpuManager>().weaponTier = 2;
                    break;
                case "miniNukeSprite(Clone)":
                    //GameManager.instance.actualWeaponCpu = 5;
                    GameObject.Find("NukeCanvas").transform.GetChild(0).gameObject.SetActive(true);
                    GameManager.instance.nukeLock = true;
                    GameManager.instance.turn = 0;
                    AudioManager.instance.playSound("NukeAlarm");
                    //GameManager.instance.cpu.GetComponent<CpuManager>().weaponTier = 5;
                    break;
            }
            GameManager.instance.weaponList.Remove(this.localInfo);
            GameManager.instance.updateCpuArm();
            transform.parent.GetComponent<WeaponManager>().gotWeapon();
            GameObject.Destroy(this.gameObject);
        }
    }
}
