using System.Collections.Generic;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class CardFreeze
    {
        private float? previousScale;
        private readonly HashSet<PlayMakerFSM> hatchers = new HashSet<PlayMakerFSM>();
        internal void Apply(bool selecting, bool paused, GameObject hero)
        {
            if (!selecting) { Restore(paused); return; }
            if (!previousScale.HasValue) previousScale = Time.timeScale > 0 ? Time.timeScale : 1f;
            if (!paused) Time.timeScale = 0;
            if (hero == null) return;
            // FSM transitions can still run on an Update with zero delta time.
            // Suspend the native Soul-consuming hatch controller explicitly.
            foreach (var fsm in hero.GetComponentsInChildren<PlayMakerFSM>(true))
                if (fsm.FsmName == "Hatchling Spawn" && fsm.enabled)
                { hatchers.Add(fsm); fsm.enabled = false; }
        }
        internal void Restore(bool paused)
        {
            if (!previousScale.HasValue) return;
            if (!paused) Time.timeScale = previousScale.Value;
            previousScale = null;
            foreach (var fsm in hatchers) if (fsm != null) fsm.enabled = true;
            hatchers.Clear();
        }
    }
}
