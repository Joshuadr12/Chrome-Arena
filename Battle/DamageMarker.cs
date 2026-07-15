using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageMarker : MonoBehaviour
{
    /// <summary>
    /// Displays damage dealt when needed.
    /// </summary>

    // Global variables.
    public static float gravity = 1;

    // Serialized variables for the editor.
    public TMP_Text text;
    public GameObject critObj;

    // Miscellaneous variables.
    [HideInInspector] public int damage = 0;
    [HideInInspector] public bool isCritical = false;
    Vector3 velocity, spin;
    float fade;
    float timer = 2;
    float time;
    Color critLerp;

    // Start is called before the first frame update.
    void Start()
    {
        velocity = new Vector3(Random.value / 2 - 0.5f, 1.5f, 0);
        spin = new Vector3(0, 0, Random.value * 30 - 15);
        text.text = damage.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        // Update lifespan.
        time = Time.deltaTime * Master.data.battleSpeed;
        timer -= time;
        if (timer <= 0)
            Destroy(gameObject);
        else
        {
            transform.Translate(velocity * time);
            velocity += Vector3.down * gravity * time;
            transform.GetChild(0).Rotate(spin * time);
            text.color = Color.Lerp
                (Color.clear,
                Color.black,
                timer);

            // When the damage is critical.
            if (isCritical)
            {
                critObj.SetActive(true);
                critObj.transform.Rotate(spin * time * 50);

                critLerp = Color.Lerp
                    (Color.clear,
                    Color.white,
                    timer);
                critObj.GetComponent<SpriteRenderer>().color = critLerp;
            }
        }
    }
}