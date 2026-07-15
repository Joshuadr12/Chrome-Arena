using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chrome Arena/Upgrade")]
public class Upgrade : ScriptableObject
{
    public string upgradeId, upgradeName;
    [TextArea(2, 4)] public string description;
    public Sprite displaySprite;
    [Tooltip("Whether or not this upgrade opens a new building.")] public bool isBuilding;
    [Tooltip("The requirements needed for this upgrade to be visible.")] public RequirementSet revealRequirement;
    [Tooltip("The cost and requirements for each upgrade; implies the number of times the player can upgrade.")] public List<Layer> layers;

    [Serializable]
    public class Layer
    {
        public int upgradeCost = 1;
        public RequirementSet requirements;
    }
}