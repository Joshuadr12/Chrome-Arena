using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Decoration : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();

    int spriteIndex = 0;
    SpriteRenderer renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprites[0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Wind")
        {
            spriteIndex = (spriteIndex + 1) % sprites.Count;
            renderer.sprite = sprites[spriteIndex];
        }
    }
}