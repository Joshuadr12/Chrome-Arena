using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    /// <summary>
    /// Used to change the player's display unit.
    /// </summary>

    [SerializeField] Slider musicSlider, sfxSlider;
    [SerializeField] UnitOptions unitOptions;
    [SerializeField] List<TMP_Dropdown> defaultSquads;

    List<string> squadsChosen = new List<string>(),
        allSquads = new List<string>();

    public void Start()
    {
        // Volume Settings
        musicSlider.value = Master.data.musicVolume;
        sfxSlider.value = Master.data.sfxVolume;

        // Default Squads
        foreach (Squad squad in Master.data.squads)
            allSquads.Add(squad.squadName);
        squadsChosen.Clear();
        foreach (int n in Master.data.defaultSquads)
            squadsChosen.Add(allSquads[n]);
        Master.RenderSquadDropdowns(defaultSquads, squadsChosen);

        unitOptions.UpdateUnitOptions("basic", canBeBig: false);
    }

    public void LevelSelect()
    {
        Master.Save();
        Master.GotoScene("LevelSelect");
    }

    public void MusicVolume()
    {
        Master.data.musicVolume = musicSlider.value;
    }

    public void SFXVolume()
    {
        Master.data.sfxVolume = sfxSlider.value;
    }

    public void MakeChoice(int choiceIndex)
    {
        /// <summary>Select the given button for the list of chosen squads.</summary>
        /// <param name="buttonIndex">The index of the button being selected.</param>

        TMP_Dropdown dropdown = defaultSquads[choiceIndex];
        squadsChosen[choiceIndex] = dropdown.captionText.text;
        Master.data.defaultSquads[choiceIndex] = allSquads
            .IndexOf(squadsChosen[choiceIndex]);
        Master.RenderSquadDropdowns(defaultSquads, squadsChosen);
    }

    public void SelectCollection(int index)
    {
        unitOptions.UpdateUnitOptions(unitOptions.collections[index], canBeBig: false);
    }

    public void SelectCharacter(int index)
    {
        /// <summary>Select and change the player's display unit, then return to the level selection menu.</summary>
        /// <param name="unit">The unit to select.</param>

        Master.data.character = unitOptions.GetUnit(index);
    }
}