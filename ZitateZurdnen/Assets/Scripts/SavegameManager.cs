using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class SavegameManager
{
    static string filePath => Application.streamingAssetsPath + "/save.save";
    

    public static void StoreHighscore(int newHighscore)
    {
        string fileText = newHighscore.ToString();

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Nimmt NUR die Ordner aus dem filePath und kreiert sie - Wenn sie schon existieren passiert nix!
        File.WriteAllText(filePath, fileText, Encoding.UTF8); // UTF8 legt fest, wie der Text als 1en und 0en abgelegt würd - damit alle Zeichen richtig gelesen werden können und aus Frau Müller kein Käse wird!
    }

    public static int GetHighscore()
    {
        if (!File.Exists(filePath))
             return  0;

        string fileText = File.ReadAllText(filePath).Trim();

        if (int.TryParse(fileText, out int score))
            return score;
        else
            Debug.LogError("File war keine Zahl du sehr intelligenter User ;) <3  in der Save Datei steht grade: " + fileText);
 
        return -1;
    }

}
