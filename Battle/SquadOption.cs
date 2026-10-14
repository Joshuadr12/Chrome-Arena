using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquadOption : MonoBehaviour
{
    public bool isLeft;
    public int index;
    public Button button;
    [Tooltip("The text for displaying color advantage if this is a player squad.")] public TMP_Text leftText;
    [Tooltip("The text for displaying color advantage if this is an enemy squad.")] public TMP_Text rightText;

    SquadSelect manager;
    TMP_Text squadText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        manager = FindFirstObjectByType<SquadSelect>();
        squadText = button.transform.GetChild(0)
            .GetComponent<TMP_Text>();
        button.enabled = isLeft;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSquad(Squad squad)
    {
        button.GetComponent<Image>().color = Master
        .colours[squad.colour].physicalColour;
        squadText.text = squad.squadName;
        if (Master.colours[squad.colour].whiteText)
            squadText.color = Color.white;
        squadText.text = squad.squadName;
    }

    public void SetChoice()
    {
        if (isLeft)
            manager.SetChoice(index);
    }
}