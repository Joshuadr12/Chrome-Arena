using TMPro;
using UnityEngine;

public class SquadOption_Proto : SquadOption
{
    SquadSelect_Proto managerProto;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        managerProto = FindFirstObjectByType<SquadSelect_Proto>();
        squadText = button.transform.GetChild(0)
            .GetComponent<TMP_Text>();
        button.enabled = isLeft;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    new public void SetChoice()
    {
        if (isLeft)
            managerProto.SetChoice(index);
    }
}