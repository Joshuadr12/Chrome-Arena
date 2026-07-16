using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollPanel : MonoBehaviour
{
    public GameObject item;
    public GridLayoutGroup layout;
    public Vector2 cellSize;

    List<GameObject> items = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        layout.cellSize = cellSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<GameObject> Populate(int population)
    {
        foreach (GameObject obj in items)
            Destroy(obj);
        items = new List<GameObject>();
        for (int obj = 0; obj < population; obj++)
            items.Add(Instantiate(item, layout.transform));

        layout.GetComponent<RectTransform>().sizeDelta = new Vector2
            (population * (cellSize.x + layout.spacing.x)
            + layout.spacing.x, 0);

        return items;
    }
}