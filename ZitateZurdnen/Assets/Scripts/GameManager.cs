using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // geht im Build nicht!
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public QuoteDataCollection quoteCollection;
    /// <summary>
    /// uses different quote lists depending if the game should be sfw or not.
    /// </summary>
    public List<QuoteData> UsedQuoteList => nsfw ? quoteCollection.QuoteList : quoteCollection.SFWQuoteList;
    public List<string> authorList = new List<string>();

    // Verknüpfungen zum screen
    [Header("Screen")]
    public Image mainImage;
    public TextMeshProUGUI zitatText;
    public TextMeshProUGUI hiddenName;
    public TextMeshProUGUI scoreText;
    public Button[] Buttons = new Button[4];


    public ParticleSystem rightButtonVFX;

    public string hiddenNameString = "???";

    // Aktuelle Runde
    [Header("Current Round")]
    [SerializeField] QuoteData zitatZumRaten;

    public bool nsfw = true;
    public int rightAnswerButtonID;
    public int wrongAttemptCounter;

    //this is the short version of a 'Property', normally it would look like this:
    // int wrongAttemptMax { get {return Buttons.Length - 1; } }
    int wrongAttemptMax => Buttons.Length - 1;

    /// <summary>
    /// Quotes we already asked for, in appearing order
    /// </summary>
    HashSet<string> alreadyUsedQuote = new HashSet<string>();

    int reachedPoints = 0;

    // UX
    [Header("UX")]
    public float quoteDelayTime = 3;
    [Tooltip("first entry: zero failed Attempts\nsecond entry: one failed Attempts\netc.")]
    public int[] pointsPerQuote = new int[] { 100, 50, 25 };

    // Start is called before the first frame update
    void Start()
    {
        SetAutorList();
        SetScoreText();

        // Runde starten
        SetZitat();
    }

    void SetAutorList()
    {
        foreach (QuoteData item in UsedQuoteList)
        {
            if (!authorList.Contains(item.nameOfAuthor))
            {
                authorList.Add(item.nameOfAuthor);
            }
        }
    }

    public void SetZitat()
    {
        wrongAttemptCounter = 0;
        zitatZumRaten = GetNewQuote();
        Debug.Log("das Zitat ist " + zitatZumRaten.quote);

        mainImage.sprite = zitatZumRaten.picture;
        zitatText.text = zitatZumRaten.quote;
        hiddenName.text = hiddenNameString;

        // Setze den Richtig-Button
        rightAnswerButtonID = Random.Range(0, Buttons.Length);
        Buttons[rightAnswerButtonID].GetComponentInChildren<TextMeshProUGUI>().text = zitatZumRaten.nameOfAuthor;

        // Die anderen falschen Buttons füllen

        HashSet<string> usedAuthors = new HashSet<string>();
        usedAuthors.Add(zitatZumRaten.nameOfAuthor);

        for (int i = 0; i < Buttons.Length; i++)
        {
            if (i == rightAnswerButtonID)
                continue;

            string randomAuthor;

            do
            {
                randomAuthor = authorList[Random.Range(0, authorList.Count)];
            }
            while (usedAuthors.Contains(randomAuthor)); //Es müssen mehr Autoren als es Buttons gibt in der Autorenliste sein! So! JA! NIEMAND wird das jeh lesen!!!

            usedAuthors.Add(randomAuthor);

            Buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = randomAuthor;
        }
    }

    /// <summary>
    /// returns a random quote which hasn't been asked for yet, or at least lately
    /// </summary>
    QuoteData GetNewQuote()
    {
        QuoteData resultQuote;

        // since the sfw-list is generated every time we call it.
        List<QuoteData> quoteList = UsedQuoteList;

        // if we already used all existing quotes...
        if (alreadyUsedQuote.Count >= quoteList.Count)
            ShrinkAlreadyUsedQuotes();

        do
        {
            resultQuote = quoteList[Random.Range(0, quoteList.Count)];
        }
        while (alreadyUsedQuote.Contains(resultQuote.quote));

        alreadyUsedQuote.Add(resultQuote.quote);
        return resultQuote;
    }

    /// <summary>
    /// mostly clears the list of already used quotes, but keeps the last used ones.
    /// Keeps up to the last three, but enshures the list shrinks at least by one.
    /// </summary>
    void ShrinkAlreadyUsedQuotes()
    {
        List<string> lastUsedQuotes = new List<string>(alreadyUsedQuote);
        alreadyUsedQuote.Clear();

        int keptQuotes = Mathf.Min(3, lastUsedQuotes.Count - 1);

        for (int i = lastUsedQuotes.Count - keptQuotes; i < lastUsedQuotes.Count; i++)
        {
            alreadyUsedQuote.Add(lastUsedQuotes[i]);
        }
    }

    public void ButtonClick(int buttonID)
    {
        if (buttonID == rightAnswerButtonID)
        {
            PlayWinButton();
        }
        else
        {
            PlayWrongButton(buttonID);
        }
    }

    // Das alles passiert wenn man auf den richtigen Namen klickt
    void PlayWinButton()
    {
        rightButtonVFX.Play();

        StartCoroutine(NewQuoteDelay(true));
    }

    // Das passiert wenn man auf den falschen Namen klickt
    void PlayWrongButton(int buttonID)
    {
        wrongAttemptCounter++;

        if (wrongAttemptCounter == wrongAttemptMax)
        {
            StartCoroutine(NewQuoteDelay(false));
        }

        Buttons[buttonID].interactable = false;
    }

    IEnumerator<WaitForSeconds> NewQuoteDelay(bool won)
    {
        hiddenName.text = zitatZumRaten.nameOfAuthor;

        reachedPoints += CalculateGrantedPoints(won, wrongAttemptCounter);

        SetScoreText();

        // So, jetzt kann kein Button mehr gedrückt werden! 
        foreach (Button item in Buttons)
        {
            item.interactable = false;
        }

        Buttons[rightAnswerButtonID].transform.localScale *= 1.3f;

        if (won == false)
        {
            // farbe zurück ändern
        }

        yield return new WaitForSeconds(quoteDelayTime);

        Buttons[rightAnswerButtonID].transform.localScale /= 1.3f;

        if (won == false)
        {
            // farbe zurück ändern
        }

        foreach (Button item in Buttons)
        {
            item.interactable = true;
        }

        SetZitat();
    }

    /// <summary>
    /// Grants points depending with how few errors the solution was found
    /// </summary>
    int CalculateGrantedPoints(bool isWon, int wrongAttempts)
    {
        if (!isWon || wrongAttempts >= pointsPerQuote.Length) return 0;

        return pointsPerQuote[wrongAttempts];
    }

    /// <summary>
    /// Set the score text to the currently reached points
    void SetScoreText()
    {
        scoreText.text = reachedPoints.ToString();
    }
}
