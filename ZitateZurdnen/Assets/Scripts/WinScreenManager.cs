using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreenManager : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text scoreText;
    [SerializeField] TMPro.TMP_Text highScoreText;

    private void Start()
    {
        scoreText.text = GameManager.reachedPoints.ToString();
        highScoreText.text = SavegameManager.GetHighscore().ToString();
    }

    public void ButtonNextRound()
    {
        SceneLoad.TryLoadScene(SceneLoad.SceneID.GuessAuthor);
    }
}
