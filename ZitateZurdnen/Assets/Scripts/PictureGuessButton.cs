using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles stuff related to the Picture Option in Picture Guess mode
/// </summary>
public class PictureGuessButton : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Button button;

    public void SetSprite(Sprite newSprite)
    {
        image.sprite = newSprite;
    }

    /// <summary>
    /// directs the press to the manager, also used by Keyboard numbers
    /// </summary>
    public void ButtonPressed()
    {
        if (!button.interactable) return;// keyboard was pressed, but button should not react

        PictureGuessGameManager.s_ButtonPressed(this);
    }

    /// <summary>
    /// sets interactivity of Button
    /// </summary>
    public void SetInteractable(bool isInteractable)
    {
        button.interactable = isInteractable;
    }

    public void SetVisible(bool isVisible)
    {
        image.enabled = isVisible;
    }
}
