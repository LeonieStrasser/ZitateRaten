using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages loading of scenes
/// </summary>
public static class SceneLoad
{
    public enum SceneID
    {
        NONE,
        GuessAuthor,
        GuessPicture,
        WinScreen
    }

    /// <summary>
    /// Tries to load a given scene. If an error occurs, its printed and the function returns false.
    /// </summary>
    public static bool TryLoadScene(SceneID sceneToLoad)
    {
        try
        {
            SceneManager.LoadSceneAsync(sceneToLoad.ToString());
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }
    }
}
