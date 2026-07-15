using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Particle : MonoBehaviour
{
    public static int paintDepth = 0;

    public bool isBase = true;
    public bool isPaint = true;
    public float animationTime = 0.5f;
    public List<Sprite> sprites;
    public GameObject spawn;
    [Tooltip("The material to coat decoration in upon collision.")] public Material decorMaterial;

    [HideInInspector] public float size = 1;
    [HideInInspector] public Color baseColor, offColor, lerp;

    bool isFinished = false;
    float timer = 0, speedFactor;
    Vector3 velocity, tempVelocity;
    SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        transform.Rotate(0, 0, Random.value * 360);
        transform.localScale = Vector3.one
            * Mathf.Sqrt(size / Mathf.PI) / 2;
        renderer = GetComponent<SpriteRenderer>();
        renderer.flipX = Random.value <= 0.5f;
        if (!isPaint)
            renderer.sprite = sprites[Random.Range(0, sprites.Count)];

        velocity = new Vector3
            (Random.value - 0.5f,
            Random.value - 0.5f,
            0);
        velocity *= size;

        if (isBase)
        {
            lerp = Color.Lerp
                (baseColor,
                offColor,
                Random.value * 0.5f);
            if (isPaint)
                timer = 1;
        }

        renderer.color = lerp;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFinished)
        {
            speedFactor =
                Master.data.battleSpeed
                * Time.deltaTime
                / animationTime;
            timer += speedFactor;
            tempVelocity = Vector3.Lerp
                (velocity,
                Vector3.zero,
                timer);
            transform.position += tempVelocity * speedFactor * 2;

            if (timer >= 1)
            {
                isFinished = true;
                if (isPaint)
                {
                    transform.localScale *= 2;
                    renderer.sortingOrder = paintDepth++;
                    renderer.sprite = sprites[Random.Range(0, sprites.Count)];
                    GetComponent<Rigidbody2D>().simulated = true;
                }
                else
                    renderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -999);

                if (spawn)
                {
                    Particle particle;
                    for (int s = 0; s < size; s++)
                    {
                        particle = Instantiate
                            (spawn,
                            transform.position,
                            Quaternion.identity)
                            .GetComponent<Particle>();
                        particle.size = size;
                        particle.lerp = lerp;
                        particle.GetComponent<SpriteRenderer>().sortingOrder = paintDepth;
                    }
                }
            }
        }
    }
}