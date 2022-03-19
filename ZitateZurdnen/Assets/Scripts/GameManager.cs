using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; // geht im Build nicht!
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public List<QuoteData> QuoteList = new List<QuoteData>();
    public List<string> authorList = new List<string>();

    const int authorColum = 1; // Const = DAS WIRD SICH NIEMALS ÄNDERN!
    const int quoteColum = 0;
    const int pictureColum = 2;

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

    public int rightAnswereButtonID;
    public int wrongAttempCounter;

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
        wrongAttempCounter = 0;
        zitatZumRaten = GetNewQuote();
        Debug.Log("das Zitat ist " + zitatZumRaten.quote);

        mainImage.sprite = zitatZumRaten.picture;
        zitatText.text = zitatZumRaten.quote;
        hiddenName.text = hiddenNameString;

        // Setze den Richtig-Button
        int randomButtonNr = Random.Range(0, Buttons.Length);
        rightAnswereButtonID = randomButtonNr;
        Buttons[randomButtonNr].GetComponentInChildren<TextMeshProUGUI>().text = zitatZumRaten.nameOfAuthor;

        // Die anderen falschen Buttons füllen

        HashSet<string> usedAuthors = new HashSet<string>();
        usedAuthors.Add(zitatZumRaten.nameOfAuthor);

        for (int i = 0; i < Buttons.Length; i++)
        {
            if (i == randomButtonNr)
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
        if (buttonID == rightAnswereButtonID)
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
        wrongAttempCounter++;

        if (wrongAttempCounter == Buttons.Length - 1)
        {
            StartCoroutine(NewQuoteDelay(false));
        }

        Buttons[buttonID].interactable = false;
    }

    IEnumerator<WaitForSeconds> NewQuoteDelay(bool won)
    {

        // So, jetzt kann kein Button mehr gedrückt werden! 
        foreach (Button item in Buttons)
        {
            item.interactable = false;
        }

        Buttons[rightAnswereButtonID].transform.localScale *= 1.3f;

        if (won == false)
        {
            // farbe zurück ändern
        }

        yield return new WaitForSeconds(quoteDelayTime);

        Buttons[rightAnswereButtonID].transform.localScale /= 1.3f;

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



    [System.Serializable]
    public struct QuoteData
    {
        public string quote;
        public Sprite picture;
        public string nameOfAuthor;
    }

#if UNITY_EDITOR
    #region Editor-Stuff

    [UnityEditor.MenuItem("Tools/TSV-Table Import")]
    public static void GetMyTable()
    {
        string link = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQ2QYr92_eX7nOGKovaBNxPb4v-MNu4703l1fwPkICx0EEzA9zz4D1dH0Jwvxa-xPiX9Xllvy7-N6Fr/pub?output=tsv";
        string tabelle = GetTSVTablesViaLink(link, "LeosTabelle");
        Debug.Log(tabelle);

        CreateQuoteObjects(tabelle);
    }

    private static void CreateQuoteObjects(string tabelle)
    {
        GameManager myGameManager = GameObject.FindObjectOfType<GameManager>(); // Aus einer static Methode kann ich nicht auf den Rest des Plätzchens zugreifen, weil sie eine in der Späre schwebende Plätzchenform ist!
        myGameManager.QuoteList.Clear();


        // Sprites aus dem Ordner ziehen
        string[] allSpriteIDsInFolder = AssetDatabase.FindAssets("t: Sprite", new[] { "Assets/Art/QuoteImages" });
        Dictionary<string, Sprite> pictureCollection = new Dictionary<string, Sprite>();

        foreach (string guiID in allSpriteIDsInFolder)
        {
            string path = AssetDatabase.GUIDToAssetPath(guiID);
            Sprite zitatCard = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            string filename = System.IO.Path.GetFileName(path);
            filename = filename.Replace(".jpg", "");

            Debug.Log(path + ", Filename: " + filename);
            pictureCollection.Add(filename, zitatCard);
        }

        string[] lines = tabelle.Split('\n'); // Wir nehmen die Tabelle und spalten sie immer beim Zeilenumbruch

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        // i - 0 ist tabellenheader
        for (int i = 1; i < lines.Length; i++)
        {
            QuoteData nextData = new QuoteData();
            string[] lineSplitter = lines[i].Split('\t');

            nextData.nameOfAuthor = lineSplitter[authorColum];
            nextData.quote = lineSplitter[quoteColum];
            string pictureName = lineSplitter[pictureColum];

            // Wir checken ob das Dictionary den Picture Namen den wir und vorher aus der Liste geangelt haben - überhaupt beinhaltet - Wenn ja: Good work! - next Data mit dem Value füllen indem der Key benutzt wird!
            if (pictureCollection.ContainsKey(pictureName))
                nextData.picture = pictureCollection[pictureName];
            else
                Debug.LogWarning($"Kein Bild {pictureName} gefunden du Spakko!");

            //-------------------------- Und nun wird der nextData der Quote Liste hinzugefügt
            myGameManager.QuoteList.Add(nextData);
        }

        // SORTIEREN NACH ALFA-BEET
        myGameManager.QuoteList.Sort((x, y) => x.quote.CompareTo(y.quote));
        myGameManager.QuoteList.Sort((x, y) => x.nameOfAuthor.CompareTo(y.nameOfAuthor));

        UnityEditor.EditorUtility.SetDirty(myGameManager); // Markiert, dass sich beim Gamemanager was verändert hat fürs Speichern.
    }

    // Text von einer Webseite ziehen O.O OMG!!! DAS GEHT WIRKLIICH!!!
    public static string GetTSVTablesViaLink(string link, string tableName)
    {
        if (string.IsNullOrWhiteSpace(link))
            return "";
        else
        {
#pragma warning disable CS0618 // obsolete
            WWW www = new WWW(link);
#pragma warning restore CS0618

            while (!www.isDone)
            {
                //waaaaaaaaaaaaaaaaait
            }

            if (www.error != null && www.error != "")
            {
                Debug.LogError($"Import\t{tableName}: WWW ERROR!\n\t{www.error}\n{link}\n");
                return "";
            }
            else
                return www.text;
        }
    }
    #endregion Editor-Stuff

#endif
}
