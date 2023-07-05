using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NukeTextManager : MonoBehaviour
{
    public void ChangeNumber(string number)
    {
        if (number == "NOW")
        {
            GetComponent<Text>().text = "Falling nuke\n\n" + number;
            GameObject.FindWithTag("NukeWeapon").GetComponent<NukeManager>().enabled = true;
            StartCoroutine(DisableText());
        }
        else
        {
            GetComponent<Text>().text = "Falling nuke in\n\n" + number;
        }
    }
    private IEnumerator DisableText()
    {
        yield return new WaitForSeconds(1.75f);
        gameObject.SetActive(false);
    }
}
