using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Decoration : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();

    int spriteIndex = 0;
    float windOffset;
    SpriteRenderer renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        windOffset = (transform.position.x * Mathf.Cos(Background.windDirection)
            + transform.position.y * Mathf.Sin(Background.windDirection) + 20) / 5;
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprites[0];
        renderer.flipX = Mathf.Cos(Background.windDirection) < 0;
    }

    // Update is called once per frame
    void Update()
    {
        spriteIndex = Mathf.FloorToInt((Background.windTimer + windOffset) % sprites.Count);
        renderer.sprite = sprites[spriteIndex];
    }
}