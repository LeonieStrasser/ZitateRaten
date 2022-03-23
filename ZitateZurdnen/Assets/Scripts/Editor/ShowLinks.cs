using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkInMenu
{
    const string linkTable = "https://docs.google.com/spreadsheets/d/1Zzop0YVi42qGRN72bZIPIUN3uByRGzBZsgQeupeYHFM";

    [UnityEditor.MenuItem("Tools/Open Google Table")]
    public static void OpenHowToInk() => Application.OpenURL(linkTable);
}
