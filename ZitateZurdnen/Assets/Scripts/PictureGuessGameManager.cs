using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PictureGuessGameManager : MonoBehaviour
{
    static PictureGuessGameManager instance;

    [SerializeField] QuoteDataCollection quoteCollection;
    List<QuoteData> QuoteList => nsfw ? quoteCollection.quoteList : quoteCollection.SFWQuoteList;

    [SerializeField] TMPro.TMP_Text quoteText;
    [SerializeField] Image quoteRightImage;
    [SerializeField] TMPro.TMP_Text quoteRightText;
    [SerializeField] PictureGuessButton[] guessButtons;

    [SerializeField] QuoteData nextQuoteToGuess;

    [Header("Options")]
    [SerializeField] bool nsfw = true;

    int rightGuessButtonID;
    HashSet<Sprite> usedImages = new HashSet<Sprite>();
    Sprite[] existingImages;

    Coroutine loadNext;

    int wrongGuess = 0;
    int wrongGuessMax => guessButtons.Length - 1;

    private void Awake()
    {
        instance = this;
        quoteRightImage.gameObject.SetActive( false);
    }

    private void Start()
    {
        existingImages = quoteCollection.AllQuotePictures(nsfw);

        SetupNewGuess();
    }

    private void Update()
    {
        if (Input.anyKeyDown) CheckKeyPress();
    }

    void SetupNewGuess()
    {
        SetupNextQuote();

        quoteText.text = nextQuoteToGuess.quote;
        quoteRightImage.sprite = nextQuoteToGuess.picture;
        quoteRightText.text = quoteText.text;

        rightGuessButtonID = Random.Range(0, guessButtons.Length);

        guessButtons[rightGuessButtonID].SetSprite(nextQuoteToGuess.picture);

        SetupFalseButtons();

        wrongGuess = 0;
    }

    void SetupNextQuote()
    {
        var quoteList = QuoteList;
        var quoteListCount = quoteList.Count;

        if (usedImages.Count >= quoteListCount)
        {
            usedImages.Clear();
            usedImages.Add(nextQuoteToGuess.picture);
        }

        do
        {
            nextQuoteToGuess = quoteList[Random.Range(0, quoteListCount)];
        }
        while (usedImages.Contains(nextQuoteToGuess.picture));

        usedImages.Add(nextQuoteToGuess.picture);
    }

    void SetupFalseButtons()
    {
        var quoteList = QuoteList;
        var quoteListCount = quoteList.Count;

        HashSet<Sprite> blockedImages;
        if (usedImages.Count > existingImages.Length - guessButtons.Length)
        {
            blockedImages = new HashSet<Sprite>();
            Sprite[] alreadyUsedImages = usedImages.ToArray();
            for (int i = existingImages.Length / 2; i < alreadyUsedImages.Length; i++)
            {
                blockedImages.Add(alreadyUsedImages[i]);
            }
        }
        else
            blockedImages = new HashSet<Sprite>(usedImages);

        for (int i = 0; i < guessButtons.Length; i++)
        {
            if (i == rightGuessButtonID) continue;

            Sprite nextImage;
            do
            {
                nextImage = existingImages[Random.Range(0, existingImages.Length)];
            } while (blockedImages.Contains(nextImage));
            blockedImages.Add(nextImage);

            guessButtons[i].SetSprite(nextImage);
        }
    }

    public static void s_ButtonPressed(PictureGuessButton pressedButton) => instance.ButtonPressed(pressedButton);

    void ButtonPressed(PictureGuessButton pressedButton)
    {
        if (pressedButton != guessButtons[rightGuessButtonID])
        {
            pressedButton.SetInteractable(false);
            wrongGuess++;

            if (wrongGuess < wrongGuessMax) return;
        }

        ShowRight();
    }

    void ShowRight()
    {
        guessButtons[rightGuessButtonID].SetVisible(false);
        quoteRightImage.gameObject.SetActive(true);

        for (int i = 0; i < guessButtons.Length; i++)
            guessButtons[i].SetInteractable(false);
    }

    public void ButtonContinueAfterRight()
    {
        foreach (var button in guessButtons)
            button.SetInteractable(true);

        guessButtons[rightGuessButtonID].SetVisible(true);
        quoteRightImage.gameObject.SetActive(false);

        SetupNewGuess();
    }

    void CheckKeyPress()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) QuitGame();

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            guessButtons[0].ButtonPressed();
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            guessButtons[1].ButtonPressed();
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            guessButtons[2].ButtonPressed();
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            guessButtons[3].ButtonPressed();
    }

    void QuitGame()
    {
        Application.Quit();
    }
}