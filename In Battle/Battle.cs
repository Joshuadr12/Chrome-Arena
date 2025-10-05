using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class Battle : MonoBehaviour
{
    /// <summary>
    /// Manages the battles.
    /// </summary>

    // Global variables.
    public static Dictionary<Cause.CauseType, Dictionary<Effect.EffectType, List<Ability>>> fighterAbilities;
    public static List<Trigger> activeTriggers, pendingTriggers, turnTriggers;
    public static Squad leftSide, rightSide;
    public static Fighter leftArtifact, rightArtifact;

    // Serialized variables for the editor.
    [Header("Battle")]
    public BattleSettings settings;
    [SerializeField] GameObject fighter;
    [SerializeField] GameObject damageMarker, buffMarker;
    [SerializeField] GameObject paintParticle, shellParticle;
    [SerializeField] Fighter leftArtifactHolder, rightArtifactHolder;
    [Header("Audio")]
    [SerializeField] AudioClip summonSound;
    [SerializeField] AudioClip chargeSound, victorySound, failSound, drawSound;
    [SerializeField, Tooltip("Sounds to play based on total damage dealt, in descending order")] AudioClip[] damageSounds;
    [SerializeField, Tooltip("The minimum damage for the corresponding sounds, in descending order")] int[] damageMinimums;
    [Header("Interface")]
    [SerializeField] GameObject dialoguePanel;
    [SerializeField] TMP_Text leftMoney;
    [SerializeField] TMP_Text rightMoney;
    [SerializeField] GameObject artifactSelect;
    [SerializeField] Image artifactDeselectImage, artifactSelectImage;
    [SerializeField] TMP_Text confirmArtifactsText;
    [SerializeField] GameObject abilityDesc;
    [SerializeField] UnitDisplay leftAbilityUnit, rightAbilityUnit;
    [SerializeField, Tooltip("For camera shaking")] Camera cam;
    [SerializeField] float camShakeIntensity = 0.025f;
    [SerializeField] float camShakeTime = 1;
    [Header("Outcome")]
    [SerializeField] BattleScroll scroll;
    [SerializeField] TMP_Text outcomeText;
    [SerializeField] string leftWinText, rightWinText, drawText;

    // Miscellaneous variables.
    // -2 = Undecided; -1 = Right Wins; 0 = Draw; 1 = Left Wins
    int outcome = -2;
    int rightMoneyInit;
    bool waitingForArtifacts = false;
    float totalShake = 0;
    float modShake;
    float cameraShake = 0;
    [HideInInspector] public List<Lane> lanes = new List<Lane>();
    AudioSource source;
    Trigger newTrigger;
    Vector3 cameraPos, randomShake;

    //Start is called before the first frame update.
    void Start()
    {
        leftSide.money = leftSide.startMoney;
        if (!Master.FinishedTutorial())
            leftSide.money += leftSide.money / 5;
        rightMoneyInit = rightSide.money;
        if (rightSide.isBoss)
            rightSide.money += rightSide.money / 10;

        // Artifacts
        leftArtifact = leftArtifactHolder;
        leftArtifact.isLeft = true;
        leftArtifact.isArtifact = true;
        leftArtifact.hasArtifact = leftSide.artifact;
        leftArtifact.ChangeUnit
            (Master.data.character,
            leftSide.colour,
            true);

        rightArtifact = rightArtifactHolder;
        rightArtifact.isLeft = false;
        rightArtifact.isArtifact = true;
        rightArtifact.hasArtifact = rightSide.artifact;
        rightArtifact.ChangeUnit
            (Master.levelSelected.opponentDisplay,
            rightSide.colour,
            true);

        // Abilities and triggers
        fighterAbilities = new Dictionary<Cause.CauseType, Dictionary<Effect.EffectType, List<Ability>>>();
        foreach (Cause.CauseType cause in Master.abilityCauses)
        {
            fighterAbilities.Add
                (cause,
                new Dictionary<Effect.EffectType, List<Ability>>());
            foreach (Effect.EffectType effect in Master.abilityEffects)
                fighterAbilities[cause].Add
                    (effect,
                    new List<Ability>());
        }

        activeTriggers = new List<Trigger>();
        pendingTriggers = new List<Trigger>();
        turnTriggers = new List<Trigger>();
        outcomeText.enabled = false;
        Lane newLane;
        for (int l = 0; l < settings.lanes; l++)
        {
            newLane = new Lane();
            newLane.posY = settings.lanes - 2 * (l + 1);
            lanes.Add(newLane);
        }

        PaintParticle.paintDepth = 0;
        source = GetComponent<AudioSource>();
        source.volume = Master.data.sfxVolume;
        cameraPos = cam.transform.position;
        StarChallenges.tempScore.Reset();
        StartCoroutine(BattleFlow());
    }

    //Update is called once per frame.
    void Update()
    {
        // Display money.
        leftMoney.text = $"{leftSide.money}C";
        rightMoney.text = $"{rightSide.money}C";

        // Shake the camera.
        totalShake = Mathf.Max(totalShake, cameraShake);
        randomShake = new Vector3
            (UnityEngine.Random.value - 0.5f,
            UnityEngine.Random.value - 0.5f);
        randomShake *= cameraShake * camShakeIntensity;
        cam.transform.position = cameraPos + randomShake;
        if (cameraShake > 0)
        {
            modShake = totalShake
                * Time.deltaTime
                * Master.data.battleSpeed;
            cameraShake -= modShake / camShakeTime;
            cameraShake = Mathf.Max(cameraShake, 0);
        }
        else
            totalShake = 0;
    }

    List<Fighter> SummonFighters
        (Lane lane,
        bool isLeft,
        Line units,
        int index = 99,
        bool chargeMoney = true,
        float laneCenter = 0)
    {
        /// <summary>Summon the given line of fighters.</summary>
        /// <param name="lane">The lane in which to summon the fighters.</param>
        /// <param name="isLeft">Whether the fighters are on the left or the right.</param>
        /// <param name="units">The line of units to summon.</param>
        /// <param name="index">The location within the lane to summon the fighters at. If set higher than the pre-existing number of fighters, the new ones will be summoned at the end.</param>
        /// <param name="chargeMoney">If set to false, no money will be charged. Do this for summon abilities.</param>
        /// <param name="laneCenter">The x-position of the center of the lane where the two sides clash.</param>
        /// <returns>A list of the summoned fighters.</returns>

        List<Fighter> summoned = new List<Fighter>();
        if (units != null)
        {
            GameObject newFighter;
            Vector3 spawnPoint;
            foreach (Unit u in units.units)
            {
                // Summon on the left side.
                if (isLeft)
                {
                    index = Mathf.Min(index, lane.left.Count);
                    spawnPoint = new Vector3
                        (laneCenter - 1 - 2 * index,
                        lane.posY);
                    newFighter = Instantiate
                        (fighter,
                        spawnPoint + Vector3.left * 15,
                        Quaternion.identity);
                    newFighter.GetComponent<Fighter>().ChangeUnit(u, leftSide.colour, true);
                    lane.left.Insert(index, newFighter);
                }
                // Summon on the right side.
                else
                {
                    index = Mathf.Min(index, lane.right.Count);
                    spawnPoint = new Vector3
                        (laneCenter + 1 + 2 * index,
                        lane.posY);
                    newFighter = Instantiate
                        (fighter,
                        spawnPoint + Vector3.right * 15,
                        Quaternion.identity);
                    newFighter.GetComponent<Fighter>().ChangeUnit(u, rightSide.colour, true);
                    newFighter.GetComponent<Fighter>().SwitchSides();
                    lane.right.Insert(index, newFighter);
                }
                // Increase the index to ensure that the fighters are placed in the right order.
                index++;

                // Set the variables for the new fighter.
                Fighter comp = newFighter.GetComponent<Fighter>();
                summoned.Add(comp);
                comp.health = u.health;
                comp.attack = u.attack;
                comp.NewPos(spawnPoint);
            }

            // Charge money if applicable.
            if (chargeMoney)
            {
                if (isLeft)
                    leftSide.money -= units.TotalPrice();
                else
                    rightSide.money -= units.TotalPrice();
            }
        }
        return summoned;
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

        source.pitch = pitchBase;
        if (pitchVaries)
            source.pitch *= UnityEngine.Random.value / 2 + 0.75f;
        source.PlayOneShot(sound);
    }
    void PlayDamageSound()
    {
        /// <summary>Play a damage sound that varies depending on how much damage was dealt.</summary>

        for (int n = 0; n < damageSounds.Length; n++)
        {
            if (cameraShake >= damageMinimums[n])
            {
                source.PlayOneShot(damageSounds[n]);
                return;
            }
        }
    }

    public Location GetLocation(Fighter f)
    {
        /// <summary>Retrieves and returns a Location class instance corresponding to the given fighter's location.</summary>
        /// <param name="f">The fighter to locate.</param>

        GameObject obj = f.gameObject;
        Location result = new Location();

        // If the fighter is an artifact holder.
        if (f.isArtifact)
        {
            result.isLeft = f.isLeft;
            result.lane = 0;
            result.index = 0;
            return result;
        }
            
        // Search each lane.
        for (int lane = 0; lane < lanes.Count; lane++)
        {
            // Search the left side.
            for
                (int index = 0;
                index < lanes[lane].left.Count;
                index++)
            {
                if (lanes[lane].left[index] == obj)
                {
                    result.isLeft = true;
                    result.lane = lane;
                    result.index = index;
                    return result;
                }
            }
            // Search the right side.
            for
                (int index = 0;
                index < lanes[lane].right.Count;
                index++)
            {
                if (lanes[lane].right[index] == obj)
                {
                    result.isLeft = false;
                    result.lane = lane;
                    result.index = index;
                    return result;
                }
            }
        }
        return null;
    }

    public Fighter GetFighter(Location location)
    {
        /// <summary>Returns the fighter at the given location.</summary>
        /// <param name="location">The location of the fighter.</param>

        if (location.isLeft)
            if (lanes[location.lane].left.Count > location.index)
                return lanes[location.lane]
                    .left[location.index]
                    .GetComponent<Fighter>();
        if (lanes[location.lane].right.Count > location.index)
            return lanes[location.lane]
                .right[location.index]
                .GetComponent<Fighter>();
        return null;
    }

    public List<Fighter> AllFighters()
    {
        /// <summary>Compiles and returns a list of all fighters in battle.</summary>

        List<Fighter> result = new List<Fighter>();
        foreach (Lane l in lanes)
        {
            foreach (GameObject f in l.left)
                result.Add(f.GetComponent<Fighter>());
            foreach (GameObject f in l.right)
                result.Add(f.GetComponent<Fighter>());
        }
        return result;
    }

    public void CreateParticle
        (GameObject obj,
        float size,
        Fighter source,
        Fighter target)
    {
        float offsetX = UnityEngine.Random.value - 0.5f;
        float offsetY = UnityEngine.Random.value - 0.5f;
        Vector3 particleOffset = new Vector3(offsetX, offsetY, 0);
        PaintParticle particle = Instantiate
            (obj,
            target.transform.position + particleOffset,
            Quaternion.identity)
            .GetComponent<PaintParticle>();
        particle.size = size;
        particle.baseColor = target.colour.physicalColour;
        particle.offColor = source.colour.physicalColour;
    }

    public void CreateBuffText(Fighter location, string text)
    {
        GameObject marker = Instantiate
            (buffMarker,
            location.transform.position,
            Quaternion.identity);
        marker.GetComponent<BuffMarker>().free = text;
    }

    public void DealDamage
        (Fighter source,
        Fighter target,
        int damage)
    {
        /// <summary>Deal damage from one fighter to another based on their stats.</summary>
        /// <param name="source">The fighter dealing damage.</param>
        /// <param name="target">The righter receiving the damage.</param>
        /// <param name="damage">The amount of damage to deal.</param>

        bool isCritical = false;
        int multi = 1;

        // Check for critical hits.
        float advantage = source.colour.Advantage(target.colour);
        if ((source.morph && !target.morph)
            || ((source.morph || !target.morph)
            && UnityEngine.Random.value <= advantage))
        {
            multi++;
            if (source.isLeft)
                StarChallenges.tempScore.criticals++;
            isCritical = true;
        }

        // Check for Agile.
        for (int n = multi; n > 0; n--)
        {
            if (!source.antiAgile
                && UnityEngine.Random.value * (target.agile + 1) > 1)
            {
                CreateBuffText(target, "DODGED");
                multi--;
            }
        }

        // Check for Block.
        if (!source.antiBlock)
        {
            while (multi > 0 && target.block > 0)
            {
                CreateBuffText(target, "BLOCKED");
                multi--;
                target.block--;
                CreateParticle
                    (shellParticle,
                    Math.Min(damage, 5),
                    source,
                    target);
                TriggerAbilities(Cause.CauseType.Block, source, target);

                if (target.isLeft)
                    StarChallenges.tempScore.blocks++;
            }
        }

        // Check for Armor.
        bool deflect = multi > 0;
        damage *= multi;
        for (int n = 0; n < target.armor; n++)
        {
            CreateParticle
                (shellParticle,
                Math.Min(damage, 5),
                source,
                target);
        }
        damage -= target.armor;

        // Deal damage.
        if (damage > 0)
        {

            target.health -= damage;
            if (target.health > 0)
                TriggerAbilities(Cause.CauseType.Nonlethal, source, target);
            else
            {
                target.Die();
                TriggerAbilities(Cause.CauseType.Death, source, target);
            }

            // Create a damage marker.
            GameObject marker = Instantiate
                (damageMarker,
                target.transform.position,
                Quaternion.identity);
            marker.GetComponent<DamageMarker>().damage = damage;
            marker.GetComponent<DamageMarker>().isCritical = isCritical;

            CreateParticle
                (paintParticle,
                Math.Min(damage, 5),
                source,
                target);

            // Miscellaneous
            cameraShake += damage;
            if (source.isLeft)
                StarChallenges.tempScore.damageDealt += damage;
        }
        else if (deflect)
            CreateBuffText(target, "DEFLECTED");
    }

    public void Fight(Fighter left, Fighter right)
    {
        /// <summary>Cause two fighters to fight each other.</summary>
        /// <param name="left">The fighter on the left.</param>
        /// <param name="right">The fighter on the right.</param>

        // Swap parameters for simpler computation with Fast.
        if (right.fast && !left.fast)
        {
            Fight(right, left);
            return;
        }

        if (left.isAttacking)
            DealDamage(left, right, left.attack);

        // Check for Fast.
        if (!left.fast || right.fast || (right.health > 0))
        {
            if (right.isAttacking)
                DealDamage(right, left, right.attack);
        }
        else if (right.isAttacking)
                CreateBuffText(left, "FAST");
    }

    public class Lane
    {
        /// <summary>
        /// Represents a single lane in battle.
        /// </summary>

        // Variables.
        public List<GameObject> left = new List<GameObject>();
        public List<GameObject> right = new List<GameObject>();
        public bool finished = false;
        public bool fighterRetreated = false;
        public float posY;

        public bool Fighting()
        {
            /// <summary>Returns whether or not there are fighters on both sides.</summary>

            return Mathf.Min(left.Count, right.Count) > 0;
        }

        public bool CanContinue(Battle battle)
        {
            /// <summary>Returns whether or not the lane is still fighting, or if it's not, more fighters can be summoned to continue fighting.</summary>
            /// <param name="battle">The battle in question.</param>

            return
                (left.Count > 0
                || leftSide.CanSummon(battle.settings.laneCapacity))
                && (right.Count > 0
                || rightSide.CanSummon(battle.settings.laneCapacity));
        }

        public int Retreat(bool isLeft, int index)
        {
            /// <summary>Remove the fighter at the given location from play.</summary>
            /// <param name="isLeft">Whether the fighter is on the left side or the right side.</param>
            /// <param name="index">The index of the fighter.</param>
            /// <returns>The amount of money refunded.</returns>

            Fighter f;
            if (isLeft)
            {
                f = left[index].GetComponent<Fighter>();
                left.RemoveAt(index);
            }
            else
            {
                f = right[index].GetComponent<Fighter>();
                right.RemoveAt(index);
            }

            // The refund is based on the proportion of the fighter's power to its original power.
            float currentValue = f.health + f.attack;
            float unitValue = f.unit.health + f.unit.attack;
            int money = Mathf.RoundToInt(currentValue
                / unitValue
                * f.unit.price);
            f.retreated = true;
            Destroy(f.gameObject);
            return money;
        }

        public int RetreatAll(bool isLeft)
        {
            /// <summary>Retreat all fighters on a given side.</summary>
            /// <param name="isLeft">The side on which to retreat the fighters.</param>
            /// <returns>The total amount of money refunded.</returns>

            int money = 0;
            if (isLeft)
                for (int n = left.Count - 1; n >= 0; n--)
                    money += Retreat(true, n);
            else
                for (int n = right.Count - 1; n >= 0; n--)
                    money += Retreat(false, n);
            return money;
        }
    }

    IEnumerator BattleFlow()
    {
        /// <summary>The main flow of battle.</summary>

        scroll.Broadcast(Master
            .colours[leftSide.colour]
            .Advantage(Master.colours[rightSide.colour]),
            true);

        // Summon the fighters, then undergo the battle loop.
        yield return StartCoroutine(SummonArmy(lanes));
        dialoguePanel.SetActive(true);
        while (dialoguePanel.activeSelf)
            yield return null;
        while (outcome < -1)
        {
            yield return StartCoroutine(Step());
            yield return StartCoroutine(Attack());
        }

        // Updates SquadStatus with the outcome of the battle.
        foreach (SquadStatus squad in SquadSelect.leftArmy)
            if (squad.squad == leftSide)
            {
                squad.outcome = outcome;
                Master.data.AddResource(squad.squad.colour, Math.Min(leftSide.money, leftSide.startMoney));
                if (outcome > 0)
                    StarChallenges.tempScore.roundsWon++;
            }
        foreach (SquadStatus squad in SquadSelect.rightArmy)
            if (squad.squad == rightSide)
                squad.outcome = -outcome;

        scroll.Broadcast(outcome, false);
        yield return Master.SetTimer(2, false);

        if (outcome == 0)
            rightSide.money = rightMoneyInit;
        else
            StarChallenges.AddTempScore();

        //print(StarChallenges.tempScore.attacks);
        SquadSelect.roundsDone++;
        SceneManager.LoadScene("SquadSelect");
    }

    IEnumerator SummonArmy(List<Lane> lanes)
    {
        /// <summary>Summon as many fighters as possible in the given lanes and activate their summon abilities as needed.</summary>

        Lane laneWaiting;
        List<Fighter> summoned = new List<Fighter>();
        bool ready = false;
        bool marching = false;
        while (!ready)
        {
            ready = true;

            // Choose and prioritize the lane with the smallest number of fighters.
            // Left side
            laneWaiting = null;
            foreach (Lane l in lanes)
            {
                if (leftSide.CanSummon(settings.laneCapacity - l.left.Count))
                {
                    ready = false;
                    if
                        (laneWaiting == null
                        || (l.left.Count < laneWaiting.left.Count))
                        laneWaiting = l;
                }
            }
            if (laneWaiting != null)
                foreach (Fighter f in SummonFighters
                    (laneWaiting,
                    true,
                    leftSide.RandomUnits(settings.laneCapacity - laneWaiting.left.Count)))
                    summoned.Add(f);

            // Right side
            laneWaiting = null;
            foreach (Lane l in lanes)
            {
                if (rightSide.CanSummon(settings.laneCapacity - l.right.Count))
                {
                    ready = false;
                    if
                        (laneWaiting == null
                        || (l.right.Count < laneWaiting.right.Count))
                        laneWaiting = l;
                }
            }
            if (laneWaiting != null)
                foreach (Fighter f in SummonFighters
                    (laneWaiting,
                    false,
                    rightSide.RandomUnits(settings.laneCapacity - laneWaiting.right.Count)))
                    summoned.Add(f);

            if (!marching && (summoned.Count > 0))
            {
                marching = true;
                PlaySound(summonSound);
            }
            yield return Master.SetTimer(0.2f);
        }

        // Select artifacts and activate summon abilities.
        if (lanes.Count > 0)
        {
            // Stop marching!
            yield return Master.SetTimer(1);
            source.Stop();
            yield return Master.SetTimer(1);

            // Select artifacts.
            if (Master.FinishedTutorial("basic_2"))
            {
                if (leftArtifact.artifactUsed < 1)
                {
                    waitingForArtifacts = true;
                    artifactSelect.SetActive(true);
                    artifactDeselectImage.sprite = leftArtifact
                        .hasArtifact
                        .sprite;
                    artifactSelectImage.sprite = leftArtifact
                        .hasArtifact
                        .sprite;
                    leftArtifact.artifactUsed = 0;
                    while (waitingForArtifacts)
                        yield return null;

                    if (leftArtifact.artifactUsed == 0)
                    {
                        TriggerAbilities(Cause.CauseType.Artifact, null, leftArtifact);
                        leftArtifact.artifactUsed = 1;
                    }
                }
                if (rightArtifact.artifactUsed < 1)
                {
                    TriggerAbilities(Cause.CauseType.Artifact, null, rightArtifact);
                    rightArtifact.artifactUsed = 1;
                }
            }

            foreach (Fighter f in summoned)
                TriggerAbilities
                    (Cause.CauseType.Summon,
                    null,
                    f);
            if (pendingTriggers.Count > 0)
                yield return StartCoroutine(ActivateTriggers());
        }
    }

    public void SelectArtifact()
    {
        leftArtifact.artifactUsed = 0;
        confirmArtifactsText.text = "CONFIRM";
    }

    public void DeselectArtifact()
    {
        leftArtifact.artifactUsed = -1;
        confirmArtifactsText.text = "X";
    }

    public void ConfirmArtifacts()
    {
        confirmArtifactsText.text = "CONFIRM";
        artifactSelect.SetActive(false);
        waitingForArtifacts = false;
    }

    void TriggerAbilities
        (Cause.CauseType cause,
        Fighter causeSource,
        Fighter causeTarget)
    {
        /// <summary>Check if a given action triggers any abilities and submit those abilities for activation.</summary>
        /// <param name="cause">The type of action in question.</param>
        /// <param name="causeSource">The source of the action.</param>
        /// <param name="causeTarget">The target of the action.</param>

        List<Fighter> sources, targets;
        Trigger newTrigger;

        // Add the artifact ability if it is an artifact.
        if (causeTarget && causeTarget.hasArtifact)
        {
            newTrigger = new Trigger();
            newTrigger.ability = causeTarget.abilities[0];
            newTrigger.causeSource = null;
            newTrigger.causeTarget = causeTarget;
            pendingTriggers.Add(newTrigger);
            return;
        }

        // When the ability is not from an artifact.
        foreach (Effect.EffectType effect in Master.abilityEffects)
        {
            foreach (Ability ability in fighterAbilities[cause][effect])
            {
                sources = ability.cause.source.UnitsPossible
                    (this,
                    GetLocation(ability.owner));
                targets = ability.cause.target.UnitsPossible
                    (this,
                    GetLocation(ability.owner));
                if
                    (sources.Contains(causeSource)
                    && targets.Contains(causeTarget))
                {
                    newTrigger = new Trigger();
                    newTrigger.ability = ability;
                    newTrigger.causeSource = causeSource;
                    newTrigger.causeTarget = causeTarget;
                    pendingTriggers.Add(newTrigger);
                }
            }
        }
    }

    IEnumerator ActivateTriggers()
    {
        /// <summary>Activate any abilities waiting to be activated and set sprite states accordingly.</summary>

        List<Fighter> abilityFighters = new List<Fighter>();
        Unit unitTurn;
        Trigger trigger;

        while (pendingTriggers.Count > 0)
        {
            // Reference the triggers waiting to be activated and sort them based on the decided order of activation.
            pendingTriggers.Sort(Trigger.CompareTo);
            do
            {
                trigger = pendingTriggers[0];
                activeTriggers.Add(trigger);
                abilityFighters.Add(trigger.ability.owner);
                pendingTriggers.Remove(trigger);
            } while
            ((pendingTriggers.Count > 0)
            && (Trigger.CompareTo(trigger, pendingTriggers[0]) == 0)
            && !trigger.ability.owner.isArtifact);

            // Enable the ability description.
            cameraShake = 0;
            unitTurn = trigger.ability.owner.unit;
            abilityDesc.SetActive(true);
            abilityDesc.transform.GetChild(0).GetComponent<TMP_Text>().text = trigger.ability.description;
            leftAbilityUnit.ChangeUnit(unitTurn, "neutral");
            rightAbilityUnit.ChangeUnit(unitTurn, "neutral");
            foreach (Fighter f in abilityFighters)
            {
                if (f.isLeft)
                    leftAbilityUnit.gameObject.SetActive(true);
                else
                    rightAbilityUnit.gameObject.SetActive(true);
            }
            yield return Master.SetTimer(1);

            // Check if any trigger is able to activate.
            List<Trigger> triggersCantUse = new List<Trigger>();
            foreach (Trigger t in activeTriggers)
            {
                if (!t.CanUse(this))
                {
                    triggersCantUse.Add(t);
                    abilityFighters.Remove(trigger.ability.owner);
                }
            }
            foreach (Trigger t in triggersCantUse)
                activeTriggers.Remove(t);

            // Add ability triggers to Star Challenges.
            foreach (Trigger t in activeTriggers)
                if (t.ability.owner.isLeft
                    && !t.ability.owner.isArtifact)
                    StarChallenges.tempScore.abilityTriggers++;

            if (activeTriggers.Count > 0)
            {
                // Activate the ability animations.
                foreach (Trigger t in activeTriggers)
                    t.ability.owner.SetAbilityAnimation(true);
                yield return Master.SetTimer(1);

                // Activate the triggers chosen from above.
                foreach (Trigger t in activeTriggers)
                    if (!t.ability.owner.retreated)
                    {
                        Location loc = GetLocation(t.ability.owner);
                        foreach (Effect effect in t.ability.effects)
                        {
                            List<Fighter> targets = effect.target.SelectTargets
                                (this,
                                loc,
                                t.causeSource,
                                t.causeTarget);
                            switch (effect.type)
                            {
                                case Effect.EffectType.Summon: // TODO: "For Opponent" functionality.
                                    foreach (int l in effect
                                        .target
                                        .SelectLanes(this, loc.lane))
                                    {
                                        Lane lane = lanes[l];
                                        if (loc.isLeft)
                                        {
                                            int summonIndex = Mathf.Min
                                                (loc.index,
                                                lane.left.Count);
                                            SummonFighters
                                                (lane,
                                                true,
                                                effect.typeUnits,
                                                summonIndex,
                                                false);
                                            for
                                                (int i = summonIndex + 1;
                                                i < lane.left.Count;
                                                i++)
                                            {
                                                Vector3 summonPos = new Vector3
                                                    (-1 - 2 * i,
                                                    lane.posY);
                                                lane
                                                    .left[i]
                                                    .GetComponent<Fighter>()
                                                    .NewPos(summonPos, false);
                                            }
                                        }
                                        else
                                        {
                                            int summonIndex = Mathf.Min
                                                (loc.index,
                                                lane.right.Count);
                                            SummonFighters
                                                (lane,
                                                false,
                                                effect.typeUnits,
                                                summonIndex,
                                                false);
                                            for
                                                (int i = summonIndex + 1;
                                                i < lane.right.Count;
                                                i++)
                                            {
                                                Vector3 summonPos = new Vector3
                                                    (1 + 2 * i,
                                                    lane.posY);
                                                lane
                                                    .right[i]
                                                    .GetComponent<Fighter>()
                                                    .NewPos(summonPos, false);
                                            }
                                        }
                                    }
                                    break;
                                case Effect.EffectType.MoveFront:
                                    foreach (Fighter f in targets)
                                    {
                                        Location targetLoc = GetLocation(f);
                                        Lane lane = lanes[targetLoc.lane];
                                        if (targetLoc.index > 0)
                                        {
                                            if (targetLoc.isLeft)
                                            {
                                                lane.left.Remove(f.gameObject);
                                                lane.left.Insert(0, f.gameObject);
                                                for
                                                    (int i = 0;
                                                    i < lane.left.Count;
                                                    i++)
                                                {
                                                    Vector3 summonPos = new Vector3
                                                        (-1 - 2 * i,
                                                        lane.posY);
                                                    lane
                                                        .left[i]
                                                        .GetComponent<Fighter>()
                                                        .NewPos(summonPos, false);
                                                }
                                            }
                                            else
                                            {
                                                lane.right.Remove(f.gameObject);
                                                lane.right.Insert(0, f.gameObject);
                                                for
                                                    (int i = 0;
                                                    i < lane.right.Count;
                                                    i++)
                                                {
                                                    Vector3 summonPos = new Vector3
                                                        (1 + 2 * i,
                                                        lane.posY);
                                                    lane
                                                        .right[i]
                                                        .GetComponent<Fighter>()
                                                        .NewPos(summonPos, false);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case Effect.EffectType.Buff:
                                    int healthGain = effect.typeInt1;
                                    int attackGain = effect.typeInt2;
                                    foreach (Fighter f in targets)
                                    {
                                        f.health += healthGain;
                                        f.attack += attackGain;

                                        GameObject marker = Instantiate
                                            (buffMarker,
                                            f.transform.position,
                                            Quaternion.identity);
                                        marker.GetComponent<BuffMarker>().health = healthGain;
                                        marker.GetComponent<BuffMarker>().attack = attackGain;
                                        if (!GetLocation(f).isLeft)
                                            marker
                                                .GetComponent<BuffMarker>()
                                                .SwitchSides();
                                    }
                                    break;
                                case Effect.EffectType.Damage:
                                    foreach (Fighter f in targets)
                                        DealDamage
                                            (t.ability.owner,
                                            f,
                                            effect.typeInt1);
                                    break;
                                case Effect.EffectType.GainColor:
                                    if (t.ability.owner.isLeft ^ effect.forOpponent)
                                    {
                                        leftSide.money += effect.typeInt1;
                                        StarChallenges.tempScore.colorGain += effect.typeInt1;
                                    }
                                    else
                                        rightSide.money += effect.typeInt1;
                                    break;
                                case Effect.EffectType.GiveTrait:
                                    foreach (Fighter f in targets)
                                    {
                                        CreateBuffText(f, effect.typeStr.ToUpper());
                                        switch (effect.typeStr)
                                        {
                                            case "block":
                                                f.block += effect.typeInt1;
                                                break;
                                            default:
                                                Debug.LogError($"Unknown trait {effect.typeStr}");
                                                break;
                                        }
                                    }
                                    break;
                                case Effect.EffectType.Retreat:
                                    foreach (Fighter f in targets)
                                        if (!f.retreated)
                                        {
                                            Location l = GetLocation(f);
                                            if (l.isLeft)
                                                leftSide.money += lanes[l.lane]
                                                    .Retreat(true, l.index);
                                            else
                                                rightSide.money += lanes[l.lane]
                                                    .Retreat(false, l.index);
                                            lanes[l.lane].fighterRetreated = true;
                                        }
                                    break;
                                default:
                                    Debug.LogError($"Unknown effect type {effect.type}");
                                    break;
                            }
                        }
                    }
                PlayDamageSound();
                yield return Master.SetTimer(1);

                // Set sprite states after the ability animations.
                foreach (Trigger t in activeTriggers)
                {
                    Fighter f = t.ability.owner;
                    if (f != null)
                    {
                        if (f.health <= 0)
                            f.Die();
                        else
                            f.SetAbilityAnimation(false);
                    }
                }
            }

            abilityFighters.Clear();
            activeTriggers.Clear();
            abilityDesc.SetActive(false);
            leftAbilityUnit.gameObject.SetActive(false);
            rightAbilityUnit.gameObject.SetActive(false);
        }
        yield return StartCoroutine(Clean());
        yield return StartCoroutine(CheckLanes());
    }

    IEnumerator Step()
    {
        /// <summary>Activate any step abilities that the fighters may have, among other things.</summary>

        // Reset fighters' hasCombo values.
        foreach (Fighter f in AllFighters())
            f.hasCombo = false;

        // Trigger Step and Bonus abilities.
        TriggerAbilities(Cause.CauseType.Step, null, null);
        if (UnityEngine.Random.value <= 0.25f)
            TriggerAbilities(Cause.CauseType.Bonus, null, null);
        if (pendingTriggers.Count > 0)
            yield return StartCoroutine(ActivateTriggers());
    }

    IEnumerator Attack(bool isCombo = false)
    {
        /// <summary>The fighters fight each other in all lanes.</summary>

        Fighter leftFighter = null, rightFighter;
        bool laneFighting = false;
        if (!isCombo)
            StarChallenges.tempScore.attacks++;

        // Initial swing
        foreach (Lane l in lanes)
        {
            if (l.Fighting())
            {
                leftFighter = l.left[0].GetComponent<Fighter>();
                if (isCombo)
                    leftFighter.isAttacking = leftFighter.hasCombo;
                else if (!leftFighter.slow
                    || (UnityEngine.Random.value <= 0.5f))
                    leftFighter.isAttacking = true;
                else
                    CreateBuffText(leftFighter, "SLOW");

                if (leftFighter.isAttacking)
                {
                    laneFighting = true;
                    if (isCombo)
                    {
                        CreateBuffText(leftFighter, "COMBO");
                        leftFighter.hasCombo = false;
                    }
                    else
                        leftFighter.hasCombo = leftFighter.combo;

                    leftFighter.NewPos
                        (new Vector3(-0.5f, l.posY),
                        false,
                        true);
                    leftFighter.SetAttackAnimation(true);
                }

                rightFighter = l.right[0].GetComponent<Fighter>();
                if (isCombo)
                    rightFighter.isAttacking = rightFighter.hasCombo;
                else if (!rightFighter.slow
                    || (UnityEngine.Random.value <= 0.5f))
                    rightFighter.isAttacking = true;
                else
                    CreateBuffText(rightFighter, "SLOW");

                if (rightFighter.isAttacking)
                {
                    laneFighting = true;
                    if (isCombo)
                    {
                        CreateBuffText(rightFighter, "COMBO");
                        rightFighter.hasCombo = false;
                    }
                    else
                        rightFighter.hasCombo = rightFighter.combo;

                    rightFighter.NewPos
                        (new Vector3(0.5f, l.posY),
                        false,
                        true);
                    rightFighter.SetAttackAnimation(true);
                }
            }
        }

        // Fight
        if (laneFighting)
        {
            PlaySound(chargeSound);
            yield return Master.SetTimer(1);

            cameraShake = 0;
            foreach (Lane l in lanes)
            {
                if (l.Fighting())
                {
                    leftFighter = l.left[0].GetComponent<Fighter>();
                    rightFighter = l.right[0].GetComponent<Fighter>();
                    Fight(leftFighter, rightFighter);
                    leftFighter.isAttacking = false;
                    rightFighter.isAttacking = false;
                    leftFighter.NewPos
                        (new Vector3(-1, l.posY),
                        false);
                    rightFighter.NewPos
                        (new Vector3(1, l.posY),
                        false);
                }
            }
            PlayDamageSound();
            yield return Master.SetTimer(1);
        }

        // Reset animation
        foreach (Lane l in lanes)
        {
            if (l.Fighting())
            {
                l.left[0].GetComponent<Fighter>()
                    .SetAttackAnimation(false);
                l.right[0].GetComponent<Fighter>()
                    .SetAttackAnimation(false);
            }
        }

        yield return StartCoroutine(ActivateTriggers());
        if (!isCombo)
            yield return StartCoroutine(Attack(true));
    }

    IEnumerator Clean()
    {
        /// <summary>Dispose of dead fighters, relocate persisting ones, and summon more where needed.</summary>

        bool cleanup = false;
        GameObject obj;
        Fighter f;
        List<Lane> lanesDown = new List<Lane>();
        bool summonFighters = false;

        // Dispose of any dead fighters and relocate persisting fighters accordingly.
        foreach (Lane lane in lanes)
        {
            // Left side.
            for (int n = 0; n < lane.left.Count; n += 0)
            {
                obj = lane.left[n];
                f = obj.GetComponent<Fighter>();
                if (f.health <= 0)
                {
                    lane.left.Remove(obj);
                    Destroy(obj);
                    StarChallenges.tempScore.casualties++;
                    cleanup = true;
                }
                else
                {
                    // f.SetAnimation();
                    f.Die(false);
                    f.NewPos(new Vector3(-1 - 2 * n, lane.posY));
                    if (lane.fighterRetreated)
                    {
                        cleanup = true;
                        lane.fighterRetreated = false;
                    }
                    n++;
                }
            }

            // Right side.
            for (int n = 0; n < lane.right.Count; n += 0)
            {
                obj = lane.right[n];
                f = obj.GetComponent<Fighter>();
                if (f.health <= 0)
                {
                    lane.right.Remove(obj);
                    Destroy(obj);
                    cleanup = true;
                }
                else
                {
                    //f.SetAnimation();
                    f.Die(false);
                    f.NewPos(new Vector3(1 + 2 * n, lane.posY));
                    n++;
                }
            }

            // Check for any lanes that need more fighters.
            if (lane.CanContinue(this))
            {
                lanesDown.Add(lane);
                if (!lane.Fighting())
                    summonFighters = true;
            }
        }

        // Summon more fighters in any lanes that need it.
        if (summonFighters)
            yield return StartCoroutine(SummonArmy(lanesDown));
        else if (cleanup)
            yield return Master.SetTimer(2);
    }

    IEnumerator CheckLanes()
    {
        /// <summary>Retreat and get rid of lanes that can no longer continue, and decide the outcome of the battle when it's time.</summary>

        List<Lane> temp = new List<Lane>();

        // Retreat where lanes can no longer continue. If no more lanes can continue, then this will not apply to the last one.
        foreach (Lane l in lanes)
            if
                (!l.CanContinue(this)
                && (temp.Count < lanes.Count - 1))
                temp.Add(l);

        // Remove said lanes.
        foreach (Lane l in temp)
        {
            leftSide.money += l.RetreatAll(true);
            rightSide.money += l.RetreatAll(false);
            lanes.Remove(l);
        }

        // Decide outcome when it's time.
        if (lanes.Count <= 0)
            outcome = 0;
        else if (lanes.Count == 1)
        {
            if (lanes[0].CanContinue(this))
            {
                if (!lanes[0].Fighting())
                {
                    temp.Clear();
                    temp.Add(lanes[0]);
                    yield return StartCoroutine(SummonArmy(lanes));
                }
            }
            else
            {
                leftSide.money += lanes[0].RetreatAll(true);
                rightSide.money += lanes[0].RetreatAll(false);
                yield return Master.SetTimer(0.5f);
                if (leftSide.money == rightSide.money)
                    outcome = 0;
                else
                    outcome = (int)Mathf.Sign(leftSide.money - rightSide.money);
            }
        }
    }
}

[Serializable]
public class Line
{
    /// <summary>
    /// Represents a line of fighter (not a lane) to be deployed into battle.
    /// </summary>

    // Variables.
    public List<Unit> units;

    public int TotalPrice(float percentPerUnit = 0.1f)
    {
        /// <summary>Returns the total cost to deploy the line.</summary>
        /// <param name="percentPerUnit">A percentage to add to the price for each fighter after the first. This serves as a trade-off for strategy.</param>

        //Each additional unit beyond the first increases the total price by the given percentage.
        float multiplier = percentPerUnit * (units.Count - 1);
        float total = 0;
        foreach (Unit u in units)
            total += u.price;
        return Mathf.FloorToInt(total * (multiplier + 1));
    }

    public float TotalWeight()
    {
        /// <summary>Returns the total probability weight of deploying this line.</summary>

        return 1 / (float)TotalPrice();
    }
}

public class Location
{
    /// <summary>
    /// Represents the location of a fighter in battle.
    /// </summary>

    // Variables.
    public bool isLeft;
    public int lane, index;

    public string GetString()
    {
        if (isLeft)
            return $"Left, {lane}-{index}";
        return $"Right, {lane}-{index}";
    }
}