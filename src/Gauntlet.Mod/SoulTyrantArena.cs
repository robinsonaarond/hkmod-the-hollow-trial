using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gauntlet.Mod
{
    // Preserve the Sanctum scenery, floor, palette and separate music FSM.
    // Both native boss phases stay asleep for this owned trial visit.
    internal sealed class SoulTyrantArena : IDisposable
    {
        private readonly List<(GameObject gameObject, bool active)> objects = new List<(GameObject, bool)>();
        private PlayMakerFSM music;

        internal void Claim(IEnumerable<GameObject> sceneRoots)
        {
            if (objects.Count != 0) throw new InvalidOperationException("Soul Tyrant's arena is already claimed.");
            var roots = sceneRoots.ToArray();
            var actors = new[] { "Dream Mage Lord", "Dream Mage Lord Phase2", "_Enemies" }
                .Select(name => roots.FirstOrDefault(root => root.name == name)).ToArray();
            if (actors.Any(actor => actor == null)) throw new InvalidOperationException("Expected Soul Tyrant encounter roots were not found.");
            var manager = roots.FirstOrDefault(root => root.name == "_SceneManager");
            music = manager?.GetComponents<PlayMakerFSM>().FirstOrDefault(fsm => fsm.FsmStates.Any(state => state.Name == "Boss Statue")
                && fsm.FsmStates.Any(state => state.Name == "Wait for Event"));
            if (music == null) throw new InvalidOperationException("Soul Tyrant's native music controller was not found.");
            foreach (var actor in actors)
            { objects.Add((actor, actor.activeSelf)); actor.SetActive(false); }
        }
        internal void StartMusic()
        {
            // Normally sent by the native boss's intro. Keep the music FSM alive
            // and use its own Attuned cue/snapshot without activating the boss.
            if (music != null) music.SendEvent("GG MUSIC");
        }
        public void Dispose()
        {
            foreach (var saved in objects) if (saved.gameObject != null) saved.gameObject.SetActive(saved.active);
            objects.Clear(); music = null;
        }
    }
}
