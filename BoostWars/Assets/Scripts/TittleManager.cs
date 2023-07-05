using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class TittleManager : MonoBehaviour
{

    internal static int currentMap = 0;
    public Image imgShow;
    public Text tittleText;
    public Sprite[] maps;
    public string[] tittleMap = new string[] { "Map 1 - Classic", "Map 2 - Mine", "Map 3 - Snake" };
    public bool mapChanged = false;
    public bool pressing = false;
    private bool canChange = true;
    public GameObject mainCanvas;
    public GameObject lvlSelectorCanvas;
    public GameObject settingsCanvas;
    public GameObject diffButton;
    public bool canPlayGame = true;
    public AudioClip mainMenu;
    public AudioClip selectMapAudio;
    public AudioSource source;
    public AudioClip letsGo;

    private void Start()
    {
        currentMap--;
        ChangeMap(1);
    }

    public void settingsCall(int dir)
    {
        GameSettings.ChangeColor(dir, diffButton);
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void selectMap()
    {
        mainCanvas.SetActive(false);
        lvlSelectorCanvas.SetActive(true);
        source.clip = selectMapAudio;
        source.volume = 0.35f;
        source.Play();
    }

    public void goMainMenu()
    {
        lvlSelectorCanvas.SetActive(false);
        mainCanvas.SetActive(true);
        source.clip = mainMenu;
        source.volume = 0.07f;
        source.Play();
    }

    public void goMainMenuFromSett()
    {
        settingsCanvas.SetActive(false);
        mainCanvas.SetActive(true);
    }

    public void drag(CallbackContext context)
    {
        Vector2 delta = context.ReadValue<Vector2>();
        if((delta.x > 20 || delta.x < -20) || (delta.y > 20 || delta.y < -20))
        {
            canPlayGame = false;
        }
        if (delta.x > 50)
        {
            ChangeMap(-1);
        }
        else if(delta.x < -50)
        {
            ChangeMap(1);
        }
    }

    public void ChangeMap(int next)
    {
        if (pressing && mapChanged) return;
        mapChanged = true;
        if (currentMap == 0 && next == -1)
        {
            currentMap = 2;
        }
        else if(currentMap == 2 && next == 1)
        {
            currentMap = 0;
        }
        else
        {
            currentMap += next;
        }

        imgShow.sprite = maps[currentMap];
        tittleText.text = tittleMap[currentMap];
    }

    public void Pressing()
    {
        if(canChange)
            pressing = !pressing;
        canChange = !canChange;
        //print(pressing);
    }

    public void loadOptions()
    {
        mainCanvas.SetActive(false);
        settingsCanvas.SetActive(true);
    }

    public void loadMap()
    {
        if (canPlayGame)
        {
            source.Stop();
            source.clip = letsGo;
            source.Play();
            source.loop = false;
            Button[] buttons = lvlSelectorCanvas.GetComponentsInChildren<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].interactable = false;
            }
            StartCoroutine(waitStart(currentMap));
        }
    }

    public IEnumerator waitStart(int mapSelected)
    {
        while (source.isPlaying)
        {
            yield return new WaitForFixedUpdate();
        }
        SceneManager.LoadScene(mapSelected + 1);
    }

    private void Update()
    {
        if (!pressing)
        {
            mapChanged = false;
            canPlayGame = true;
        }
    }
}
