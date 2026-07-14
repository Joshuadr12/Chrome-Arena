using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [FormerlySerializedAs("level")] public BattleLevel battle;
    public TMP_Text title;
    public List<Image> stars;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (battle)
        {
            if (battle.requirements.RequirementsMet())
            {
                if (title.text == "")
                    title.text = battle.battleName;
                for (int i = 0; i < Master.GetStars(battle); i++)
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