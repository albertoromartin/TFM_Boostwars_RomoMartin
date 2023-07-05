using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject normalCanvas;

    public void pauseGame()
    {
        AudioManager.instance.pauseMusicIngame("PauseSound");
        normalCanvas.SetActive(false);
        gameObject.SetActive(true);
        GameManager.instance.isGamePaused = true;
        Time.timeScale = 0f;
    }

    public void continueGame()
    {
        AudioManager.instance.playSound("PauseSound");
        AudioManager.instance.resumeMusic();
        gameObject.SetActive(false);
        normalCanvas.SetActive(true);
        GameManager.instance.isGamePaused = false;
        Time.timeScale = 1f;
        
    }

    public void restartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(TittleManager.currentMap + 1);
    }

    public void exitGametoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
