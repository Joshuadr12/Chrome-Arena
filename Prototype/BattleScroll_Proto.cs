using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleScroll_Proto : BattleScroll
{
    /// <summary>
    /// Displays messages during a round of battle.
    /// </summary>

    new public static BattleScroll_Proto main;

    // Start is called before the first frame update.
    void Start()
    {
        main = this;
        audioComp = GetComponent<AudioSource>();
        audioComp.volume = Master.data.sfxVolume;
    }

    // Update is called once per frame.
    void Update()
    {
        // Update timer.
        timer += Master.data.battleSpeed * Time.deltaTime;

        if (timer >= 2)
            gameObject.SetActive(false);
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

        if (audioComp == null)
        {
            audioComp = GetComponent<AudioSource>();
            audioComp.volume = Master.data.sfxVolume;
        }

        audioComp.pitch = pitchBase;
        if (pitchVaries)
            audioComp.pitch *= Random.value / 2 + 0.75f;
        audioComp.PlayOneShot(sound);
    }

    new public void Broadcast(float outcome, bool isStart)
    {
        /// <summary>Initiate the message sequence.</summary>
        /// <param name="outcome">The color advantage/disadvantage if isStart is set to true, or the outcome of the round if isStart is set to false.</param>
        /// <param name="isStart">If set to true, announce the color advantage. If set to false, announce the outcome of the round.</param>

        gameObject.SetActive(true);
        // Decide the sound to play and text to display based on the phase in the round and the outcome
        // When there is no color advantage or the round is a draw.
        if (outcome == 0)
        {
            PlaySound(drawSound);
            text.text = isStart ? "0% Advantage!" : drawText;
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
}