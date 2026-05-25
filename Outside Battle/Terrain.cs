using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Chrome Arena/Terrain")]
public class Terrain : ScriptableObject
{
    public GameObject decorObj;
    public List<Sprite> decorSprites;
    [Range(0, 10)] public float decorDensity;
    public int decorFrames = 2;
    [Tooltip("The wind speed in m/s.")] public float minWind, maxWind;
    [Tooltip("The acceleration of wind speed change in m/s2.")] public float windAcceleration = 1f;

    public List<Sprite> GetRandomDecor()
    {
        List<Sprite> result = new List<Sprite>();
        int options = decorSprites.Count / decorFrames;
        for (int sprite = Random.Range(0, options);
            sprite < decorSprites.Count;
            sprite += options)
            result.Add(decorSprites[sprite]);
        return result;
    }
}