using System;
using System.Collections;
using System.Linq;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Gauntlet.Mod
{
    // Failed Champion requires FK Floor, barrel/rubble and crack pools belonging
    // to his original room. Two independent Broken Vessels are the fallback.
    internal sealed class BrokenVessel : MonoBehaviour
    {
        private PlayMakerFSM control;
        internal static void Validate(GameObject prefab)
        {
            var fsm = prefab.GetComponents<PlayMakerFSM>().FirstOrDefault(f => f.FsmName == "IK Control");
            if (fsm == null || prefab.GetComponent<HealthManager>() == null ||
                !new[] { "Init", "Close Gates", "Set Pos", "Intro Fall", "Idle" }.All(n => fsm.FsmStates.Any(s => s.Name == n)))
                throw new InvalidOperationException("Unsupported Broken Vessel control layout.");
        }
        internal void Configure(float left, float right, float floor)
        {
            Validate(gameObject);
            control = GetComponents<PlayMakerFSM>().Single(f => f.FsmName == "IK Control");
            // Native arena floor was 26.99. Preserve attack-relative heights.
            foreach (string name in new[] { "Air Dash Height", "Min Dstab Height" })
            {
                var v = control.FsmVariables.FindFsmFloat(name);
                if (v != null) v.Value += floor - 26.99f;
            }
            control.FsmVariables.FindFsmFloat("Left X").Value = left + 2;
            control.FsmVariables.FindFsmFloat("Right X").Value = right - 2;
            var position = control.FsmStates.Single(s => s.Name == "Set Pos");
            position.Actions = position.Actions.Where(a => !(a is SetPosition)).ToArray();
            // Both copies retain independent native AI, stun, attacks and HP.
            // The room controller owns music, gates, rewards and completion.
            foreach (var fsm in GetComponentsInChildren<PlayMakerFSM>(true))
                foreach (var state in fsm.FsmStates)
                    state.Actions = state.Actions.Where(a => !(a is SetPlayerDataBool)
                        && !(a is SendEventByName send && (send.sendEvent.Value == "BG CLOSE" ||
                            send.sendEvent.Value == "BG QUICK CLOSE" || send.sendEvent.Value == "BOSS DEATH" ||
                            send.sendEvent.Value == "DREAM RETURN"))).ToArray();
            NativeBoss.SuppressCompletion(gameObject);
        }
        private IEnumerator Start()
        {
            // Allow the native FSM to bind Self and child hitboxes first.
            yield return null;
            yield return null;
            if (control != null)
            {
                var renderer = GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = true;
                control.SetState("Close Gates");
            }
        }
    }
}
