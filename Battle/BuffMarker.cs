using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffMarker : MonoBehaviour
{
    /// <summary>
    /// Displays health and attack buffs when a fighter obtains it.
    /// </summary>

    // Serialized varialbes for the editor.
    public GameObject healthObj, attackObj;
    public TMP_Text healthText, attackText, freeText;

    // Miscellaneous variables.
    [HideInInspector] public int health = 0;
    [HideInInspector] public int attack = 0;
    [HideInInspector] public string free;
    Vector2 velocity;
    float fade;
    float timer = 2;
    float time;

    // Start is called before the first frame update.
    void Start()
    {
        // Set random velocity.
        velocity = new Vector2(0, Random.value / 2);

        // Set health and attack.
        if (health == 0)
            healthObj.SetActive(false);
        else
            healthText.text = health.ToString();
        if (attack == 0)
            attackObj.SetActive(false);
        else
            attackText.text = attack.ToString();

        // Set free text.
        freeText.text = free;
    }

    // Update is called once per frame.
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
            healthObj.GetComponent<Image>().color = Color.Lerp
                (Color.clear,
                Color.white,
                timer);
            attackObj.GetComponent<Image>().color = Color.Lerp
                (Color.clear,
                Color.white,
                timer);
            healthText.color = Color.Lerp
                (Color.clear,
                Color.black,
                timer);
            attackText.color = Color.Lerp
                (Color.clear,
                Color.black,
                timer);
        }
    }

    public void SwitchSides()
    {
        /// <summary>Swap the health/attack positions.</summary>

        Vector3 temp = healthObj.transform.position;
        healthObj.transform.position = attackObj.transform.position;
        attackObj.transform.position = temp;
    }
}