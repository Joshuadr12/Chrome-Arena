using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedButton : MonoBehaviour
{
    /// <summary>
    /// Changes the battle speed when clicked.
    /// </summary>

    // Variables.
    [SerializeField] TMP_Text text;

    // Update is called once per frame.
    void Update()
    {
        text.text = $"Speed\n{Master.data.battleSpeed}";
    }

    public void ChangeSpeed()
    {
        /// <summary>Change the battle speed.</summary>

        Master.data.battleSpeed *= 2;
        // Reset the battle speed when it's too high.
        if (Master.data.battleSpeed > 8)
            Master.data.battleSpeed = 1;
    }
}