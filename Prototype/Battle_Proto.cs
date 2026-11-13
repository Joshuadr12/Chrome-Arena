using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle_Proto : Battle
{
    /// <summary>
    /// Manages the battles in the prototype scene.
    /// </summary>

    [SerializeField] SquadSelect_Proto squadSelectProto;
    [HideInInspector] public List<Lane> lanesProto = new List<Lane>();

    //Start is called before the first frame update.
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.volume = Master.data.sfxVolume;
    }

    //Update is called once per frame.
    void Update()
    {
        if (inBattle)
        {
            // Display paint and artifact points.
            leftPaintText.text = $"${leftSide.paint}";
            rightPaintText.text = $"${rightSide.paint}";
        }
    }

    new public void StartBattle()
    {
        inBattle = true;
        outcome = -2;
        leftSide.paint = leftSide.startMoney;
        leftSide.paint += leftSide.paint / 5;
        rightPaintInit = rightSide.paint;

        lanesProto.Clear();
        Lane newLane;
        for (int l = 0; l < settings.lanes; l++)
        {
            newLane = new Lane();
            newLane.posY = settings.lanes - 2 * (l + 1);
            lanesProto.Add(newLane);
        }

        StartCoroutine(BattleFlow());
    }

    List<Fighter> SummonFighters
        (Lane lane,
        bool isLeft,
        Line units,
        int index = 99,
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
            Fighter newFighter;
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
                        Quaternion.identity)
                        .GetComponent<Fighter>();
                    newFighter.ChangeUnit(u, leftSide.colour, true);
                    lane.left.Insert(index, newFighter.gameObject);
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
                        Quaternion.identity)
                        .GetComponent<Fighter>();
                    newFighter.ChangeUnit(u, rightSide.colour, true);
                    newFighter.SwitchSides();
                    lane.right.Insert(index, newFighter.gameObject);
                }
                // Increase the index to ensure that the fighters are placed in the right order.
                index++;

                // Set the variables for the new fighter.
                Fighter comp = newFighter.GetComponent<Fighter>();
                summoned.Add(comp);
                //comp.health = u.health;
                //comp.attack = u.attack;
                comp.NewPos(spawnPoint, false);
            }

            // Charge money if applicable.
            if (isLeft)
                leftSide.paint -= units.TotalPrice();
            else
                rightSide.paint -= units.TotalPrice();
        }
        return summoned;
    }

    new public void DealDamage
        (Fighter source,
        Fighter target,
        int damage,
        bool tooFast = false)
    {
        /// <summary>Deal damage from one fighter to another based on their stats.</summary>
        /// <param name="source">The fighter dealing damage.</param>
        /// <param name="target">The righter receiving the damage.</param>
        /// <param name="damage">The amount of damage to deal.</param>
        /// <param name="tooFast">Whether or not the target is Fast and already killed the source.</param>

        bool isCritical = false;

        // Check for critical hits.
        float advantage = source.colour.Advantage(target.colour);
        if (UnityEngine.Random.value <= advantage / 2f)
        {
            damage *= 2;
            isCritical = true;
        }

        // Deal damage.
        if (damage > 0)
        {

            target.health -= damage;

            // Create a damage marker.
            GameObject marker = Instantiate
                (flyingText,
                target.transform.position,
                Quaternion.identity);
            marker.GetComponent<FlyingText>().value = damage;
            marker.GetComponent<FlyingText>().isCritical = isCritical;
            marker.SetActive(false);
            target.markers.Add(marker);
        }
    }

    new public void Fight(Fighter left, Fighter right)
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

        if (right.isAttacking)
            DealDamage(right, left, right.attack,
                left.fast && !right.fast && right.health <= 0);

        left.UpdateDisplayStats();
        right.UpdateDisplayStats();
    }

    new public class Lane
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
            f.RemoveFromBattle();
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

        // Summon the fighters, then undergo the battle loop.
        yield return StartCoroutine(SummonArmy(lanesProto));
        while (outcome < -1)
            yield return StartCoroutine(Attack());

        // Updates SquadStatus with the outcome of the battle.
        foreach (SquadStatus squad in squadSelectProto.leftSquads)
            if (squad.squad == leftSide)
                squad.outcome = outcome;
        foreach (SquadStatus squad in squadSelectProto.rightSquads)
            if (squad.squad == rightSide)
                squad.outcome = -outcome;

        BattleScroll_Proto.main.Broadcast(outcome, false);
        yield return Master.SetTimer(2, false);

        if (outcome == 0)
            rightSide.paint = rightPaintInit;

        SquadSelect.roundsDone++;
        squadSelectProto.NextRound();
        inBattle = false;
    }

    IEnumerator SummonArmy(List<Lane> lanes)
    {
        /// <summary>Summon as many fighters as possible in the given lanes and activate their summon abilities as needed.</summary>

        Lane laneWaiting;
        bool ready = false;
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
            {
                foreach (Fighter f in SummonFighters
                    (laneWaiting,
                    true,
                    leftSide.RandomUnits(settings.laneCapacity - laneWaiting.left.Count)));
            }

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
            {
                foreach (Fighter f in SummonFighters
                    (laneWaiting,
                    false,
                    rightSide.RandomUnits(settings.laneCapacity - laneWaiting.right.Count)));
            }

            yield return Master.SetTimer(0.2f);
        }

        if (lanes.Count > 0)
            yield return Master.SetTimer(2);
    }

    IEnumerator Attack()
    {
        /// <summary>The fighters fight each other in all lanes.</summary>

        Fighter leftFighter = null, rightFighter;
        bool laneFighting = false;

        // Initial swing
        foreach (Lane l in lanesProto)
        {
            if (l.Fighting())
            {
                laneFighting = true;

                leftFighter = l.left[0].GetComponent<Fighter>();
                leftFighter.isAttacking = true;
                leftFighter.NewPos
                    (new Vector3(-0.5f, l.posY),
                    false,
                    true);

                rightFighter = l.right[0].GetComponent<Fighter>();
                rightFighter.isAttacking = true;
                rightFighter.NewPos
                    (new Vector3(0.5f, l.posY),
                    false,
                    true);
            }
        }

        // Fight
        if (laneFighting)
        {
            yield return Master.SetTimer(1);

            foreach (Lane l in lanesProto)
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
            yield return Master.SetTimer(1);
        }

        // Reset animation
        foreach (Lane l in lanesProto)
        {
            if (l.Fighting())
            {
                l.left[0].GetComponent<Fighter>()
                    .SetAnimation();
                l.right[0].GetComponent<Fighter>()
                    .SetAnimation();
            }
        }

        yield return StartCoroutine(CleanAndCheck());
    }

    IEnumerator CleanAndCheck()
    {
        /// <summary>Dispose of dead fighters, relocate persisting ones, and summon more where needed.
        /// Retreat and get rid of lanes that can no longer continue, and decide the outcome of the battle when it's time.</summary>

        bool cleanup = false;
        GameObject obj;
        Fighter f;
        List<Lane> lanesDown = new List<Lane>();
        bool summonFighters = false;

        // Dispose of any dead fighters and relocate persisting fighters accordingly.
        foreach (Lane lane in lanesProto)
        {
            // Left side.
            for (int n = 0; n < lane.left.Count; n += 0)
            {
                obj = lane.left[n];
                f = obj.GetComponent<Fighter>();
                if (f.health <= 0)
                {
                    lane.left.Remove(obj);
                    f.RemoveFromBattle();
                    cleanup = true;
                }
                else
                {
                    f.SetAnimation();
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
                    f.RemoveFromBattle();
                    cleanup = true;
                }
                else
                {
                    f.SetAnimation();
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
        
        // Retreat where lanes can no longer continue. If no more lanes can continue, then this will not apply to the last one.
        List<Lane> temp = new List<Lane>();
        foreach (Lane l in lanesProto)
            if
                (!l.CanContinue(this)
                && (temp.Count < lanesProto.Count - 1))
                temp.Add(l);

        // Remove said lanes.
        foreach (Lane l in temp)
        {
            leftSide.paint += l.RetreatAll(true);
            rightSide.paint += l.RetreatAll(false);
            lanesProto.Remove(l);
        }

        // Decide outcome when it's time.
        if (lanesProto.Count <= 0)
            outcome = 0;
        else if (lanesProto.Count == 1)
        {
            if (lanesProto[0].CanContinue(this))
            {
                if (!lanesProto[0].Fighting())
                {
                    temp.Clear();
                    temp.Add(lanesProto[0]);
                    yield return StartCoroutine(SummonArmy(lanesProto));
                }
            }
            else
            {
                temp.Clear();
                temp.Add(lanesProto[0]);
                leftSide.paint += lanesProto[0].RetreatAll(true);
                rightSide.paint += lanesProto[0].RetreatAll(false);
                yield return Master.SetTimer(0.5f);
                if (leftSide.paint == rightSide.paint)
                    outcome = 0;
                else
                    outcome = (int)Mathf.Sign(leftSide.paint - rightSide.paint);
            }
        }
    }
}