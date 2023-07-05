using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GameSettings
{
    public static int difficulty = 1;
    public static string[] nameDiff = new string[] { "Easy", "Normal", "Hard", "Extreme" };
    public static Color[] colorsDiff = new Color[] { new Color(0.1254902f, 0.9058824f, 0.8759409f, 0.945098f), new Color(0.1254902f, 0.9058824f, 0.2588235f, 0.945098f), new Color(0.9058824f, 0.8223792f, 0.1254902f, 0.945098f), new Color(0.9058824f, 0.1254902f, 0.1254902f, 0.945098f) };
    public static float[] difficultyMod = new float[] { 2.5f, 1.5f, 0.5f, 0f };

    public static void ChangeColor(int next, GameObject difficultyPanel)
    {
        if (difficulty == 0 && next == -1)
        {
            difficulty = 3;
        }
        else if (difficulty == 3 && next == 1)
        {
            difficulty = 0;
        }
        else
        {
            difficulty += next;
        }

        difficultyPanel.GetComponent<Image>().color = colorsDiff[difficulty];
        difficultyPanel.transform.GetChild(0).gameObject.GetComponent<Text>().text = nameDiff[difficulty];
    }

    public static float getDifficultyMod()
    {
        return difficultyMod[difficulty];
    }
}
