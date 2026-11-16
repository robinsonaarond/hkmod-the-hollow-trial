using System.Collections;
using System.Linq;
using Gauntlet.Core;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Gauntlet.Mod
{
    // Extend awareness, rather than attack range or damage. The native FSMs
    // still choose attacks and move bodies against the real terrain.
    internal sealed class EnemyEngagement : MonoBehaviour
    {
        private ArenaController arena;
        private EnemyKind kind;
        private PlayMakerFSM crawler;
        private int facing;
        private float nextTurn;

        internal void Configure(ArenaController owner, EnemyKind enemyKind, float arenaWidth)
        {
            arena = owner; kind = enemyKind;
            float radius = arenaWidth + 8f;
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                // Attack Range deliberately keeps its native size.
                if (!IsAwarenessRange(child.name)) continue;
                foreach (var collider in child.GetComponents<Collider2D>())
                {
                    if (!collider.isTrigger) continue;
                    var scale = child.lossyScale;
                    float sx = Mathf.Max(0.01f, Mathf.Abs(scale.x));
                    float sy = Mathf.Max(0.01f, Mathf.Abs(scale.y));
                    if (collider is CircleCollider2D circle)
                    { circle.offset = Vector2.zero; circle.radius = radius / Mathf.Min(sx, sy); }
                    else if (collider is BoxCollider2D box)
                    { box.offset = Vector2.zero; box.size = new Vector2(2 * radius / sx, 2 * radius / sy); }
                }
            }
            foreach (var fsm in GetComponents<PlayMakerFSM>())
            {
                var hero = fsm.FsmVariables.FindFsmGameObject("Hero");
                if (hero != null) hero.Value = HeroController.instance.gameObject;
                if (kind == EnemyKind.HeavySentry) SetBool(fsm, "Spawner", false);
                if (kind == EnemyKind.WingedSentry) SetBool(fsm, "Start Idle", true);
                if (kind == EnemyKind.MantisWarrior)
                {
                    // Ignore the main save's pacification flag in this trial.
                    var check = fsm.FsmStates.FirstOrDefault(state => state.Name == "Lords Defeated?");
                    if (check != null)
                        foreach (var test in check.Actions.OfType<PlayerDataBoolTest>()) test.isTrue = test.isFalse;
                }
                SetBool(fsm, "Start Alert", true); SetBool(fsm, "startAlert", true);
                if (kind == EnemyKind.GreatHopper) SetBool(fsm, "Alerted", true);
                if (kind == EnemyKind.Hopper)
                {
                    SetBool(fsm, "Roams Within Range", false);
                    var range = fsm.FsmStates.FirstOrDefault(state => state.Name == "Range?");
                    if (range != null)
                        foreach (var compare in range.Actions.OfType<FloatCompare>()) compare.float2.Value = radius;
                }
                if (kind == EnemyKind.PrimalAspid)
                {
                    // Its native Idle state sends SELF SLEEP beyond old X/Y
                    // distance limits. Keep it awake throughout this arena.
                    var idle = fsm.FsmStates.FirstOrDefault(state => state.Name == "Idle");
                    if (idle != null)
                        foreach (var compare in idle.Actions.OfType<FloatCompare>()) compare.float2.Value = radius;
                }
                // Spitters otherwise give up chasing beyond their old room range.
                var unalert = fsm.FsmVariables.FindFsmFloat("Unalert Range");
                if (unalert != null) unalert.Value = radius;
                if (kind == EnemyKind.Crawlid && fsm.FsmName == "Crawler") crawler = fsm;
            }
            var walker = GetComponent<Walker>();
            if (walker != null) walker.startInactive = false;
        }
        internal static bool IsAwarenessRange(string name) => name == "Alert Range" || name == "Alert Range New" || name == "Unalert Range" || name == "Wake Region" || name == "Alert Region" || name == "Unalert Region" || name == "Lost hero Region";
        private static void SetBool(PlayMakerFSM fsm, string name, bool value)
        {
            var variable = fsm.FsmVariables.FindFsmBool(name);
            if (variable != null) variable.Value = value;
        }
        private IEnumerator Start()
        {
            // Let native Start initialize Walker and FSM-owned references first.
            yield return null;
            if (!Engaged) yield break;
            var walker = GetComponent<Walker>();
            if (walker != null) walker.Go(HeroController.instance.transform.position.x > transform.position.x ? 1 : -1);
            if (kind == EnemyKind.PrimalAspid)
                foreach (var fsm in GetComponents<PlayMakerFSM>())
                    if (fsm.FsmName == "spitter" && fsm.ActiveStateName == "Sleep") fsm.SendEvent("WAKE");
            foreach (var fsm in GetComponents<PlayMakerFSM>())
            {
                if (kind == EnemyKind.WingedSentry) fsm.SendEvent("WAKE");
                if (kind == EnemyKind.HeavySentry) fsm.SendEvent("ALERT");
            }
            TurnCrawler(true);
        }
        private bool Engaged => arena != null && arena.Run.Phase == RunPhase.Combat && HeroController.instance != null;
        private void Update()
        {
            if (!Engaged || crawler == null || Time.time < nextTurn) return;
            nextTurn = Time.time + 0.5f;
            TurnCrawler(false);
        }
        private void TurnCrawler(bool first)
        {
            if (crawler == null || !crawler.enabled) return;
            float difference = HeroController.instance.transform.position.x - transform.position.x;
            if (!first && (Mathf.Abs(difference) < 2.5f || (crawler.ActiveStateName != "Walk" && crawler.ActiveStateName != "Maintain"))) return;
            int direction = difference > 0 ? 1 : -1;
            if (!first && direction == facing) return;
            facing = direction;
            crawler.SendEvent(direction > 0 ? "GO RIGHT" : "GO LEFT");
        }
    }
}
