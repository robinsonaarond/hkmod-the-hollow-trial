using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Gauntlet.Mod
{
    // The native rebuild doubles as a Joni refill. During a trial the loadout
    // owns health; inventory/HUD rebuilding must only draw the remaining masks.
    internal sealed class BlueHealthHud
    {
        private readonly Dictionary<FsmState, FsmStateAction[]> original = new Dictionary<FsmState, FsmStateAction[]>();
        private sealed class ReadCounter : FsmStateAction
        {
            private readonly FsmInt target;
            private readonly Func<int> read;
            internal ReadCounter(FsmInt target, Func<int> read) { this.target = target; this.read = read; }
            public override void OnEnter() { target.Value = read(); Finish(); }
        }
        internal void Prepare(GameObject hud)
        {
            if (hud == null) return;
            foreach (var fsm in hud.GetComponentsInChildren<PlayMakerFSM>(true))
            {
                if (fsm.FsmName != "Blue Health Control") continue;
                foreach (var state in fsm.FsmStates)
                {
                    if (original.ContainsKey(state)) continue;
                    var actions = new List<FsmStateAction>();
                    foreach (var action in state.Actions)
                    {
                        if (state.Name == "Set Blue")
                        {
                            if (action is SendMessage send && send.functionCall?.FunctionName == "UpdateBlueHealth") continue;
                            if (action is GetPlayerDataInt get && get.intName.Value == "healthBlue")
                            {
                                actions.Add(new ReadCounter(get.storeValue, () => PlayerData.instance.equippedCharm_27 ? 0 : PlayerData.instance.healthBlue));
                                continue;
                            }
                            if (action is GetPlayerDataInt joni && joni.intName.Value == "joniHealthBlue")
                            {
                                actions.Add(new ReadCounter(joni.storeValue, () => PlayerData.instance.equippedCharm_27
                                    ? PlayerData.instance.healthBlue + PlayerData.instance.maxHealth : 0));
                                continue;
                            }
                        }
                        if (state.Name == "Add Joni Health" && action is IncrementPlayerDataInt add && add.intName.Value == "healthBlue") continue;
                        actions.Add(action);
                    }
                    if (!actions.SequenceEqual(state.Actions))
                    { original.Add(state, state.Actions); state.Actions = actions.ToArray(); }
                }
            }
        }
        internal void Restore()
        {
            foreach (var entry in original) entry.Key.Actions = entry.Value;
            original.Clear();
        }
    }
}
