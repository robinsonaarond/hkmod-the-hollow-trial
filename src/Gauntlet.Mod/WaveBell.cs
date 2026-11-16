using System;
using System.Linq;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class WaveBell : MonoBehaviour
    {
        private ArenaController arena;
        private PlayMakerFSM control;
        private bool ringing;
        private float readyAt;
        internal void Configure(ArenaController owner)
        {
            arena = owner;
            control = GetComponents<PlayMakerFSM>().FirstOrDefault(f => f.FsmName == "Bell Control");
            if (control == null) throw new InvalidOperationException("Native bell control missing.");
            foreach (var state in control.FsmStates)
                state.Actions = state.Actions.Where(a => !(a is SendEventByName send && send.sendEvent.Value == "STAG BELL RUNG")).ToArray();
        }
        private void Update()
        {
            bool now = control != null && (control.ActiveStateName == "Ring R" || control.ActiveStateName == "Ring L");
            if (now && !ringing && Time.time >= readyAt && arena != null && arena.Run.CanCallNextWave)
            {
                arena.CallNextWave(); readyAt = Time.time + 0.5f;
            }
            ringing = now;
        }
    }
}
