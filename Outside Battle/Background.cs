using UnityEngine;

public class Background : MonoBehaviour
{
    public static float windTimer = 0, windDirection;

    public Terrain terrain;

    float windSpeed, newSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Decoration decor;
        Vector2 decorPos;
        for (float n = terrain.decorDensity * 400; n > 0; n--)
        {
            decorPos = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
            decor = Instantiate(terrain.decorObj, decorPos, Quaternion.identity, transform)
                .GetComponent<Decoration>();
            decor.sprites = terrain.GetRandomDecor();
            decor.GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(decorPos.y * -999);
        }

        windTimer = 0;
        windDirection = Random.value * Mathf.PI * 2;
        windSpeed = Random.Range(terrain.minWind, terrain.maxWind);
        newSpeed = windSpeed;
        Invoke("ChangeWindSpeed", Random.Range(10f, 30f));
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(newSpeed - windSpeed) <= 0.1f)
            windSpeed = newSpeed;
        else
            windSpeed += Time.deltaTime * terrain.windAcceleration
                * Mathf.Sign(newSpeed - windSpeed);

        windTimer += Time.deltaTime * windSpeed;
    }

    void ChangeWindSpeed()
    {
        newSpeed = Random.Range(terrain.minWind, terrain.maxWind);
        Invoke("ChangeWindSpeed", Random.Range(10f, 30f));
    }
}