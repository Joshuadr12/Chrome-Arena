using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Decoration : MonoBehaviour
{
    public List<Sprite> sprites = new List<Sprite>();

    int spriteIndex = 0;
    float windOffset;
    SpriteRenderer spriteComp;
    Particle particle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        windOffset = (transform.position.x * Mathf.Cos(Background.windDirection)
            + transform.position.y * Mathf.Sin(Background.windDirection) + 20) / 5;
        spriteComp = GetComponent<SpriteRenderer>();
        spriteComp.sprite = sprites[0];
        spriteComp.flipX = Mathf.Cos(Background.windDirection) < 0;
    }

    // Update is called once per frame
    void Update()
    {
        spriteIndex = Mathf.FloorToInt((Background.windTimer + windOffset) % sprites.Count);
        try
        {
            spriteComp.sprite = sprites[spriteIndex];
        }
        catch
        {
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            particle = collision.GetComponent<Particle>();
            if (particle != null && particle.spreadColor)
            {
                spriteComp.material = particle.decorMaterial;
                spriteComp.color = particle.lerp;
            }
        }
    }
}