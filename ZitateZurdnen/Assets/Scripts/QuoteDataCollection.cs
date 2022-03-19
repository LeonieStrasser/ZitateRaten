using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class QuoteDataCollection : ScriptableObject
{
    public List<QuoteData> QuoteList = new List<QuoteData>();

    /// <summary>
    /// Creates a SFW-version of the QuoteList.
    /// </summary>
    // .Where() is a Linq Statement, in the brackets you can define a condition.
    // Here it's: take only the List-Elements (here named quote) where nfsw is false.
    public List<QuoteData> SFWQuoteList => (List<QuoteData>)QuoteList.Where(quote => quote.nsfw == false);

    const int authorColumn = 1; // Const = DAS WIRD SICH NIEMALS ÄNDERN!
    const int quoteColumn = 0;
    const int pictureColumn = 2;
    const int nsfwColumn = 3;

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
        myGameManager.quoteCollection.QuoteList.Clear();

        // Sprites aus dem Ordner ziehen
        string[] allSpriteIDsInFolder = AssetDatabase.FindAssets("t: Sprite", new[] { "Assets/Art/QuoteImages" });
        Dictionary<string, Sprite> pictureCollection = new Dictionary<string, Sprite>();

        foreach (string guiID in allSpriteIDsInFolder)
        {
            string path = AssetDatabase.GUIDToAssetPath(guiID);
            Sprite zitatCard = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            string filename = System.IO.Path.GetFileName(path);
            filename = filename.Replace(".jpg", "");

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

            nextData.nameOfAuthor = lineSplitter[authorColumn];
            nextData.quote = lineSplitter[quoteColumn];
            nextData.nsfw = (lineSplitter[nsfwColumn] == "1");

            string pictureName = lineSplitter[pictureColumn];

            // Wir checken ob das Dictionary den Picture Namen den wir und vorher aus der Liste geangelt haben - überhaupt beinhaltet - Wenn ja: Good work! - next Data mit dem Value füllen indem der Key benutzt wird!
            if (pictureCollection.ContainsKey(pictureName))
                nextData.picture = pictureCollection[pictureName];
            else
                Debug.LogWarning($"Kein Bild {pictureName} gefunden du Spakko!");

            //-------------------------- Und nun wird der nextData der Quote Liste hinzugefügt
            myGameManager.quoteCollection.QuoteList.Add(nextData);
        }

        // SORTIEREN NACH ALFA-BEET
        myGameManager.quoteCollection.QuoteList.Sort((x, y) => x.quote.CompareTo(y.quote));
        myGameManager.quoteCollection.QuoteList.Sort((x, y) => x.nameOfAuthor.CompareTo(y.nameOfAuthor));

        UnityEditor.EditorUtility.SetDirty(myGameManager.quoteCollection); // Markiert, dass sich was verändert hat fürs Speichern.
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
