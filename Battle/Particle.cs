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
    public bool matchBattleSpeed = true;
    public List<Sprite> sprites;
    public bool changeSpriteOnStart;
    public float size = 1;
    [FormerlySerializedAs("spawn")] public GameObject spawnObject;
    public int spawnCount = 1;

    [HideInInspector] public Color baseColor, offColor, lerp = Color.white;
    [HideInInspector] public Vector3 startPos, endPos;
    [HideInInspector] public bool flipX;

    bool isFinished = false;
    float timer = 0, speedFactor;
    // Variables for arch trajectory
    float a, b, c, x;
    Vector3 velocity, tempVelocity;
    SpriteRenderer renderer;

    public enum AnimationType
    {
        None,
        Explosion,
        SplashScreen,
        ArchPoint,
        ArchSpin
    }
    public enum ColorType
    {
        None,
        Lerp,
        Random,
        BaseColor,
        OffColor
    }

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        switch (animationType)
        {
            case AnimationType.Explosion:
                transform.Translate(Random.value - 0.5f, Random.value - 0.5f, 0);
                transform.Rotate(0, 0, Random.value * 360);
                transform.localScale = Vector3.one
                    * Mathf.Sqrt(size / Mathf.PI) / 2;
                renderer.flipX = Random.value <= 0.5f;

                velocity = new Vector3
                    (Random.value - 0.5f,
                    Random.value - 0.5f,
                    0);
                velocity *= size;
                break;

            case AnimationType.SplashScreen:
                DontDestroyOnLoad(gameObject);
                animationTime = Random.Range(0.4f, 0.6f);
                transform.Rotate(0, 0, Random.value * 360);
                transform.localScale = Vector3.one * Random.Range(0.5f, 1) * size;
                renderer.flipX = Random.value <= 0.5f;

                startPos = Vector3.zero;
                endPos = Camera.main.ScreenToWorldPoint(new Vector2(
                    Random.value * Camera.main.pixelWidth,
                    Random.value * Camera.main.pixelHeight));
                endPos.z = 0;
                break;

            case AnimationType.ArchPoint:
                transform.localScale = Vector3.one * size;
                renderer.flipX = flipX;
                CalculateArch();
                break;

            case AnimationType.ArchSpin:
                transform.Rotate(0, 0, Random.value * 360);
                transform.localScale = Vector3.one * size;
                renderer.flipX = Random.value <= 0.5f;
                CalculateArch();
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
                List<Color> colours = new List<Color>();
                foreach (Colour colour in Master.colours.Values)
                    if (colour.createPaint)
                        colours.Add(colour.physicalColour);
                lerp = colours[Random.Range(0, colours.Count)];
                break;

            case ColorType.BaseColor:
                lerp = baseColor;
                break;

            case ColorType.OffColor:
                lerp = offColor;
                break;

            default:
                break;
        }
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
                    Time.deltaTime
                    / animationTime;
                if (matchBattleSpeed) 
                    speedFactor *= Master.data.battleSpeed;
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

                    case AnimationType.SplashScreen:
                        speedFactor = Master.AnimationCurve
                            (timer, easeOut: true);
                        transform.position = Vector3.Lerp
                            (startPos, endPos, speedFactor);
                        break;

                    case AnimationType.ArchPoint:
                        x = timer * endPos.x + (1 - timer) * startPos.x;
                        transform.position = new Vector2(x,
                            a * Mathf.Pow(x, 2) + b * x + c);
                        transform.rotation = Quaternion.Euler(0, 0,
                            Mathf.Atan(2 * transform.position.x * a + b) * Mathf.Rad2Deg);
                        break;

                    case AnimationType.ArchSpin:
                        x = timer * endPos.x + (1 - timer) * startPos.x;
                        transform.position = new Vector2(x,
                            a * Mathf.Pow(x, 2) + b * x + c);
                        transform.Rotate(0, 0,
                            speedFactor * 720 * (flipX ? 1 : -1));
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
                {
                    switch (animationType)
                    {
                        case AnimationType.SplashScreen:
                            transform.localScale *= 2;
                            renderer.sortingOrder = paintDepth++;
                            break;

                        case AnimationType.ArchPoint:
                            Destroy(gameObject);
                            break;

                        case AnimationType.ArchSpin:
                            Destroy(gameObject);
                            break;

                        default:
                            renderer.sortingOrder = Mathf.RoundToInt(transform.position.y * -999);
                            break;
                    }
                }

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

    void CalculateArch()
    {
        ///<summary>Calculate the trajectory for an arch animation.</summary>

        // Calculate the points for vertex form.
        Vector2 vertex, point;
        if (startPos.y > endPos.y)
        {
            vertex = startPos;
            point = endPos;
        }
        else if (startPos.y == endPos.y)
        {
            vertex = new Vector2
                ((startPos.x + endPos.x) / 2,
                startPos.y + 1);
            point = startPos;
        }
        else
        {
            vertex = endPos;
            point = startPos;
        }

        // Convert vertex form to standard form.
        a = (point.y - vertex.y) / Mathf.Pow(point.x - vertex.x, 2);
        b = -2 * a * vertex.x;
        c = a * Mathf.Pow(vertex.x, 2) + vertex.y;
    }

    public IEnumerator Fade()
    {
        while (Time.deltaTime > 0.4f)
            yield return null;
        yield return new WaitForSeconds(animationTime - 0.4f);
        Destroy(gameObject);
    }
}