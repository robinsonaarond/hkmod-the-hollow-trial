using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class UiClockSettings
    {
        private readonly Dictionary<Wait, bool> waits = new Dictionary<Wait, bool>();
        private readonly Dictionary<FsmBool, bool> tweens = new Dictionary<FsmBool, bool>();
        internal void Prepare(GameObject root, bool healthOnly = false)
        {
            if (root == null) return;
            foreach (var fsm in root.GetComponentsInChildren<PlayMakerFSM>(true))
            {
                if (healthOnly && fsm.FsmName != "health_display" && fsm.FsmName != "blue_health_display"
                    && fsm.FsmName != "blue_health_display_joni" && fsm.FsmName != "Blue Health Control"
                    && fsm.FsmName != "Joni Health" && fsm.FsmName != "Low Health FX") continue;
                foreach (var state in fsm.FsmStates)
                    foreach (var action in state.Actions)
                    {
                        if (action is Wait wait && !waits.ContainsKey(wait))
                        { waits.Add(wait, wait.realTime); wait.realTime = true; }
                        if (action is iTweenFsmAction tween && tween.realTime != null && !tweens.ContainsKey(tween.realTime))
                        { tweens.Add(tween.realTime, tween.realTime.Value); tween.realTime.Value = true; }
                    }
            }
        }
        internal void Restore()
        {
            foreach (var entry in waits) entry.Key.realTime = entry.Value;
            foreach (var entry in tweens) entry.Key.Value = entry.Value;
            waits.Clear(); tweens.Clear();
        }
    }
}
