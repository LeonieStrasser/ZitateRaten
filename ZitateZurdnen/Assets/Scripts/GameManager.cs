using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // geht im Build nicht!
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public QuoteDataCollection quoteCollection;
    public List<QuoteData> QuoteList => quoteCollection.QuoteList;
    public List<string> authorList = new List<string>();

    // Verknüpfungen zum screen
    [Header("Screen")]
    public Image mainImage;
    public TextMeshProUGUI zitatText;
    public TextMeshProUGUI hiddenName;
    public Button[] Buttons = new Button[4];


    public ParticleSystem rightButtonVFX;

    public string hiddenNameString = "???";

    // Aktuelle Runde
    [Header("Current Round")]
    QuoteData zitatZumRaten;

    public int rightAnswerButtonID;
    public int wrongAttemptCounter;

    //this is the short version of a 'Property', normally it would look like this:
    // int wrongAttemptMax { get {return Buttons.Length - 1; } }
    int wrongAttemptMax => Buttons.Length - 1; 

    /// <summary>
    /// Quotes we already asked for, in appearing order
    /// </summary>
    HashSet<string> alreadyUsedQuote = new HashSet<string>();

    // UX
    [Header("UX")]
    public float quoteDelayTime = 3;

    // Start is called before the first frame update
    void Start()
    {
        SetAutorList();

        // Runde starten
        SetZitat();
    }

    void SetAutorList()
    {
        foreach (QuoteData item in QuoteList)
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

        // if we already used all existing quotes...
        if (alreadyUsedQuote.Count >= QuoteList.Count)
            ShrinkAlreadyUsedQuotes();

        do
        {
            resultQuote = QuoteList[Random.Range(0, QuoteList.Count)];
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
}
