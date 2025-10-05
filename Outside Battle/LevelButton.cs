using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public Level level;
    public TMP_Text title;
    public List<Image> stars;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (level)
        {
            if (level.requirements.RequirementsMet())
            {
                if (title.text == "")
                    title.text = level.levelName;
                for (int i = 0; i < Master.GetStars(level); i++)
                    stars[i].color = Master.goldColor;
            }
            else
                gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}