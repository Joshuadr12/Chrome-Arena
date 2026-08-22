using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(SpriteRenderer))]
public class Particle : MonoBehaviour
{
    public static int paintDepth = 0;

    public AnimationType animationType;
    public ColorType colorType;
    [Tooltip("Whether or not to color other objects that it collides with.")] public bool spreadColor;
    [Tooltip("The material to coat decoration in upon collision.")] public Material decorMaterial;
    public float animationTime = 0.5f;
    public List<Sprite> sprites;
    public bool changeSpriteOnStart;
    [FormerlySerializedAs("spawn")] public GameObject spawnObject;
    public int spawnCount = 1;

    [HideInInspector] public float size = 1;
    [HideInInspector] public Color baseColor, offColor, lerp = Color.white;

    bool isFinished = false;
    float timer = 0, speedFactor;
    Vector3 velocity, tempVelocity;
    SpriteRenderer renderer;

    public enum AnimationType
    {
        None,
        Explosion
    }
    public enum ColorType
    {
        None,
        Lerp,
        Random
    }

    // Start is called before the first frame update
    void Start()
    {
        switch (animationType)
        {
            case AnimationType.Explosion:
                transform.Rotate(0, 0, Random.value * 360);
                transform.localScale = Vector3.one
                    * Mathf.Sqrt(size / Mathf.PI) / 2;
                renderer = GetComponent<SpriteRenderer>();
                renderer.flipX = Random.value <= 0.5f;

                velocity = new Vector3
                    (Random.value - 0.5f,
                    Random.value - 0.5f,
                    0);
                velocity *= size;
                break;

            default:
                break;
        }

        switch (colorType)
        {
            case ColorType.Lerp:
                lerp = Color.Lerp(baseColor, offColor, Random.value * 0.6f);
                break;

            case ColorType.Random:
                lerp = new Color(Random.value, Random.value, Random.value);
                break;

            default:
                break;
        }
        renderer = GetComponent<SpriteRenderer>();
        renderer.color = lerp;

        if (changeSpriteOnStart && sprites.Count > 0)
            renderer.sprite = sprites[Random.Range(0, sprites.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFinished)
        {
            // Animation.
            if (animationTime <= 0)
                timer = 1;
            else
            {
                speedFactor =
                    Master.data.battleSpeed
                    * Time.deltaTime
                    / animationTime;
                timer += speedFactor;

                switch (animationType)
                {
                    case AnimationType.Explosion:
                        tempVelocity = Vector3.Lerp
                            (velocity,
                            Vector3.zero,
                            timer);
                        transform.position += tempVelocity * speedFactor * 2;
                        break;

                    default:
                        break;
                }
            }

            // Animation finished.
            if (timer >= 1)
            {
                isFinished = true;
                if (spreadColor)
                {
                    transform.localScale *= 2;
                    renderer.sortingOrder = paintDepth++;
                    GetComponent<Rigidbody2D>().simulated = true;
                }
                else
                    renderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -999);

                if (!changeSpriteOnStart && sprites.Count > 0)
                    renderer.sprite = sprites[Random.Range(0, sprites.Count)];

                if (spawnObject)
                {
                    Particle particle;
                    for (int s = 0; s < spawnCount; s++)
                    {
                        particle = Instantiate
                            (spawnObject,
                            transform.position,
                            Quaternion.identity)
                            .GetComponent<Particle>();
                        particle.size = size;
                        particle.lerp = lerp;
                        if (spreadColor)
                            particle.GetComponent<SpriteRenderer>().sortingOrder = paintDepth;
                    }
                }
            }
        }
    }
}