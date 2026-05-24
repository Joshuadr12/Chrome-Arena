using UnityEngine;

public class Background : MonoBehaviour
{
    public Terrain terrain;
    public GameObject windObj;
    public Transform windParent;

    float windSpeed = 0;

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

        if (terrain.maxWind > 0)
            ChangeWindSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        foreach (CircleCollider2D wind in windParent.GetComponentsInChildren<CircleCollider2D>())
            wind.radius += Time.deltaTime * 2;
    }

    void ChangeWindSpeed()
    {
        CancelInvoke("CreateWind");
        windSpeed = Random.Range(terrain.minWind, terrain.maxWind);
        Invoke("CreateWind", 1);
        Invoke("ChangeWindSpeed", 10);
    }

    void CreateWind()
    {
        Instantiate(windObj, Vector2.right * 20, Quaternion.identity, windParent);
        Invoke("CreateWind", 1);
    }
}