using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PaintParticle : MonoBehaviour
{
    public static int paintDepth = 0;

    public bool isBase = true;
    public float animationTime = 0.5f;
    public GameObject spawn;

    [HideInInspector] public float size = 1;
    [HideInInspector] public Color baseColor, offColor;

    float timer = 0;
    float speedFactor, scale, tempScale;
    Vector3 velocity, tempVelocity;

    // Start is called before the first frame update
    void Start()
    {
        transform.Rotate(0, 0, Random.value * 360);

        if (isBase)
        {
            scale = Mathf.Sqrt(size / Mathf.PI);
            Color lerp = Color.Lerp
                (baseColor,
                offColor,
                Random.value * 0.5f);
            GetComponent<SpriteRenderer>().color = lerp;

            PaintParticle particle;
            for (int s = 0; s < size; s++)
            {
                particle = Instantiate
                    (spawn,
                    transform.position,
                    Quaternion.identity)
                    .GetComponent<PaintParticle>();
                particle.size = size;
                particle.GetComponent<SpriteRenderer>().color = lerp;
                particle.GetComponent<SpriteRenderer>().sortingOrder = paintDepth;
            }
            GetComponent<SpriteRenderer>().sortingOrder = paintDepth++;
        }
        else
        {
            velocity = new Vector3
                (Random.value - 0.5f,
                Random.value - 0.5f,
                0);
            velocity *= size;
        }
    }

    // Update is called once per frame
    void Update()
    {
        speedFactor =
            Master.data.battleSpeed
            * Time.deltaTime
            / animationTime;
        if (isBase)
        {
            if (timer >= 1)
                transform.localScale = Vector3.one * scale;
            else
            {
                timer += speedFactor;
                tempScale = Mathf.Sqrt(size * timer / Mathf.PI);
                transform.localScale = Vector3.one * tempScale;
            }
        }
        else if (timer < 1)
        {
            timer += speedFactor;
            tempVelocity = Vector3.Lerp
                (velocity,
                Vector3.zero,
                timer);
            transform.position += tempVelocity * speedFactor * 2;
        }
    }
}