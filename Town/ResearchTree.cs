using System;
using System.Collections.Generic;
using UnityEngine;

public class ResearchTree : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // TODO: Move this code when there are more research trees.
        UpdateTree();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateTree()
    {
        foreach (CharacterButton button in GetComponentsInChildren<CharacterButton>())
            button.UpdateTree();
    }
}