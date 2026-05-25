using UnityEngine;

public class Background : MonoBehaviour
{
    public static float windTimer = 0, windDirection;
    public static Vector2 cameraMin, cameraMax;

    public Terrain terrain;

    float windSpeed, newSpeed;
    SpriteRenderer renderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Decoration
        Decoration decor;
        Vector2 decorPos;
        cameraMin = Camera.main.ScreenToWorldPoint(Vector2.zero);
        cameraMax = Camera.main.ScreenToWorldPoint(new Vector2(
            Camera.main.pixelWidth,
            Camera.main.pixelHeight));
        float area = (cameraMax.x - cameraMin.x)
            * (cameraMax.y - cameraMin.y);
        for (float n = terrain.decorDensity * area; n > 0; n--)
        {
            decorPos = new Vector2(
                Random.Range(cameraMin.x, cameraMax.x),
                Random.Range(cameraMin.y, cameraMax.y));
            decor = Instantiate(terrain.decorObj, decorPos, Quaternion.identity, transform)
                .GetComponent<Decoration>();
            decor.sprites = terrain.GetRandomDecor();
            decor.GetComponent<SpriteRenderer>().sortingOrder = Mathf.RoundToInt(decorPos.y * -999);
        }

        // Wind speed
        windTimer = 0;
        windDirection = Random.value * Mathf.PI * 2;
        windSpeed = Random.Range(terrain.minWind, terrain.maxWind);
        newSpeed = windSpeed;
        renderer = GetComponent<SpriteRenderer>();
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