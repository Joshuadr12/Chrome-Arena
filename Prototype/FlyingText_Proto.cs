using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingText_Proto : FlyingText
{
    /// <summary>
    /// Displays damage dealt when needed in the prototype scene.
    /// </summary>

    // Start is called before the first frame update.
    void Start()
    {
        timer = 1;
        text.text = value.ToString();
        if (isCritical)
            critObj.SetActive(true);
        transform.Translate(Vector3.up / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        // Update lifespan.
        timer -= Time.deltaTime * Master.data.battleSpeed;
        if (timer <= 0)
            Destroy(gameObject);
    }
}