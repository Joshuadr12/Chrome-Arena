using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class URLManager : MonoBehaviour, IPointerClickHandler
{
    /// <summary>
    /// Manages the hyperlinks in the credits menu.
    /// </summary>

    TMP_Text text;

    // Start is called before the first frame update.
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        /// <summary>When the player clicks a hyperlink, open it.</summary>
        /// <param name="eventData">The event data pertaining to the mouse click.</param>

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            int index = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
            if (index > -1)
                Application.OpenURL(text.textInfo.linkInfo[index].GetLinkID());
        }
    }
}