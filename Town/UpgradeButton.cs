using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UpgradeButton : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] Image image;
    [SerializeField] TMP_Text description;

    [HideInInspector] public Upgrade upgrade;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Button>().interactable = upgrade != null;
        if (upgrade != null)
        {
            titleText.text = upgrade.upgradeName;
            if (upgrade.layers.Count > 1)
                titleText.text += $" {Master.data.TimesUpgraded(upgrade) + 1}";
            image.sprite = upgrade.displaySprite;
            description.text = upgrade.description;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectUpgrade()
    {
        FindFirstObjectByType<UpgradeManager>().SelectUpgrade(upgrade);
    }
}