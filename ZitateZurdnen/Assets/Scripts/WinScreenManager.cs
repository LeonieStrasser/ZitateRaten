using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenManager : MonoBehaviour
{
    [SerializeField] TMPro.TMP_Text scoreText;
    [SerializeField] TMPro.TMP_Text highScoreText;

    public Image slowestImage;
    public Image fastestImage;

    private void Start()
    {
        scoreText.text = GameManager.reachedPoints.ToString();
        highScoreText.text = SavegameManager.GetHighscore().ToString();

        slowestImage.sprite = QuoteTimes.slowestQuote.picture;
        fastestImage.sprite = QuoteTimes.fastestQuote.picture;
    }

    public void ButtonNextRound()
    {
        SceneLoad.TryLoadScene(SceneLoad.SceneID.GuessAuthor);
    }
}
