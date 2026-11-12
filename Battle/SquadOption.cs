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
    [HideInInspector] public Colour colour;
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
        colour = Master.colours[squad.colour];
        button.GetComponent<Image>().color = Master
        .colours[squad.colour].physicalColour;
        squadText.text = squad.squadName;
        if (Master.colours[squad.colour].whiteText)
            squadText.color = Color.white;
        squadText.text = squad.squadName;
    }

    public void ShowAdvantage(Colour c)
    {
        int advantage = Mathf.RoundToInt(c.Advantage(colour) * 100);
        if (isLeft)
            advantage *= -1;
        Color textColor = advantage == 0 ? Color.white :
            advantage > 0 ? Color.green : Color.red;
        
        if (isLeft)
        {
            leftText.text = $"{advantage}%";
            leftText.color = textColor;
            leftText.gameObject.SetActive(true);
        }
        else
        {
            rightText.text = $"{advantage}%";
            rightText.color = textColor;
            rightText.gameObject.SetActive(true);
        }
    }
    public void HideAdvantage()
    {
        leftText.gameObject.SetActive(false);
        rightText.gameObject.SetActive(false);
    }

    public void HoverEnter()
    {
        if (Master.FinishedTutorial()
            || (SquadSelect.roundsDone >= 1))
            manager.ShowAdvantages(this);
    }
    public void HoverExit()
    {
        manager.HideAdvantages();
    }

    public void SetChoice()
    {
        if (isLeft)
            manager.SetChoice(index);
    }
}