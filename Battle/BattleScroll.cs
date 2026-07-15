using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleScroll : MonoBehaviour
{
    /// <summary>
    /// Displays messages during a round of battle.
    /// </summary>

    // Serialized variables for the editor.
    [SerializeField] TMP_Text text;
    [SerializeField] string victoryText, failText, drawText;
    [SerializeField] AudioClip victorySound, failSound, drawSound;

    // Miscellaneous variables.
    float timer = 0;
    float curve;
    RectTransform rect;
    AudioSource audioComp;

    // Start is called before the first frame update.
    void Start()
    {
        rect = GetComponent<RectTransform>();
        audioComp = GetComponent<AudioSource>();
        audioComp.volume = Master.data.sfxVolume;
    }

    // Update is called once per frame.
    void Update()
    {
        // Update timer.
        timer += Master.data.battleSpeed * Time.deltaTime;

        // Width
        curve = Master.AnimationCurve
            (Mathf.Min(timer, 1),
            smoothEnd: true);
        rect.anchorMin = new Vector2(curve / -4 + 0.5f, 0.4f);
        rect.anchorMax = new Vector2(curve / 4 + 0.5f, 0.6f);

        // Transparency
        curve = (timer - 2) * 2;
        GetComponent<Image>().color = Color.Lerp
            (Color.white,
            new Color(1, 1, 1, 0),
            curve);
        text.color = Color.Lerp
            (Color.black,
            Color.clear,
            curve);
    }

    void PlaySound
        (AudioClip sound,
        bool pitchVaries = false,
        float pitchBase = 1)
    {
        /// <summary>Play an audio clip.</summary>
        /// <param name="sound">The audio clip to play.</param>
        /// <param name="pitchVaries">If set to true, the pitch of the sound will vary randomly.</param>
        /// <param name="pitchBase">The default pitch of the sound. If pitchVaries is set to true, this is also the offset for randomization.</param>

        audioComp.pitch = pitchBase;
        if (pitchVaries)
            audioComp.pitch *= Random.value / 2 + 0.75f;
        audioComp.PlayOneShot(sound);
    }

    public void Broadcast(float outcome, bool isStart)
    {
        /// <summary>Initiate the message sequence.</summary>
        /// <param name="outcome">The color advantage/disadvantage if isStart is set to true, or the outcome of the round if isStart is set to false.</param>
        /// <param name="isStart">If set to true, announce the color advantage. If set to false, announce the outcome of the round.</param>

        // Decide the sound to play and text to display based on the phase in the round and the outcome
        if (outcome != 0 || !isStart)
        {
            // When there is no color advantage or the round is a draw.
            if (outcome == 0)
            {
                PlaySound(drawSound);
                text.text = drawText;
            }
            // When the player is at a disadvantage or failed the round.
            else if (outcome < 0)
            {
                PlaySound(failSound);
                text.text = isStart
                    ? $"{Mathf.RoundToInt(outcome * -100)}% Disadvantage!"
                    : failText;
            }
            else
            {
                PlaySound(victorySound);
                text.text = isStart
                    ? $"{Mathf.RoundToInt(outcome * 100)}% Advantage!"
                    : victoryText;
            }
            timer = 0;
        }
        else
            timer = 4;
    }
}