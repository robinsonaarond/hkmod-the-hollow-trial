using System.Collections.Generic;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace Gauntlet.Mod
{
    internal sealed class BenchlessCharms
    {
        private readonly Dictionary<FsmString, string> checks = new Dictionary<FsmString, string>();
        private readonly Dictionary<FsmState, FsmStateAction[]> states = new Dictionary<FsmState, FsmStateAction[]>();
        internal void Apply(IEnumerable<PlayMakerFSM> menus)
        {
            foreach (var menu in menus)
            {
                // Only the charm pane's equip gate changes. Inventory's bench
                // checks also control hero input and must remain native.
                if (menu.FsmName != "UI Charms") continue;
                foreach (var state in menu.FsmStates)
                {
                    foreach (var action in state.Actions)
                        if (action is PlayerDataBoolTest test && test.boolName.Value == "atBench")
                        {
                            checks[test.boolName] = test.boolName.Value;
                            test.boolName.Value = "hasCharm";
                        }
                    if (states.ContainsKey(state)) continue;
                    var kept = new List<FsmStateAction>();
                    foreach (var action in state.Actions)
                    {
                        // MenuCharmUpdate applies the native conversion and then
                        // restores wounds. A second UI broadcast would refill it.
                        if (action is SendEventByName send && send.sendEvent.Value == "UPDATE BLUE HEALTH") continue;
                        kept.Add(action);
                    }
                    if (kept.Count != state.Actions.Length)
                    { states.Add(state, state.Actions); state.Actions = kept.ToArray(); }
                }
            }
        }
        internal void Restore()
        {
            foreach (var entry in checks) entry.Key.Value = entry.Value;
            foreach (var entry in states) entry.Key.Actions = entry.Value;
            checks.Clear(); states.Clear();
        }
    }
}
