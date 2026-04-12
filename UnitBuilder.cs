using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBuilder : MonoBehaviour
{
    /// <summary>
    /// Used in editor mode to set up a unit's appearance after designing it in the SPUM editor.
    /// </summary>

    public GameObject body;
    public SPUM_SpriteList spriteList;
    public SPUM_HorseSpriteList horseList;
    public Unit unit;

    // Start is called before the first frame update
    void Start()
    {
        UpdateList(body.GetComponentsInChildren<SpriteRenderer>(),
            unit.bodySet);
    }

    void UpdateList(SpriteRenderer[] reference, List<SpriteSet> list)
    {
        list.Clear();
        foreach (SpriteRenderer renderer in reference)
        {
            SpriteSet set = new SpriteSet();
            set.sprite = renderer.sprite;
            set.color = renderer.color;
            list.Add(set);
        }
    }
}
