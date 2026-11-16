using System;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal static class NativeBoss
    {
        private static readonly FieldInfo CorpsePrefab = typeof(EnemyDeathEffects).GetField("corpsePrefab", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo Corpse = typeof(EnemyDeathEffects).GetField("corpse", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo DeathBroadcast = typeof(EnemyDeathEffects).GetField("deathBroadcastEvent", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static void Validate(GameObject boss)
        {
            var control = boss.GetComponents<PlayMakerFSM>().FirstOrDefault(fsm => fsm.FsmName == "Big Fly Control");
            if (control == null || !new[] { "Init", "GG Boss Wake", "Wake", "Buzz" }.All(name => control.FsmStates.Any(state => state.Name == name))
                || boss.GetComponent<HealthManager>() == null || boss.GetComponent<EnemyDeathEffects>() == null
                || CorpsePrefab == null || Corpse == null || DeathBroadcast == null)
                throw new InvalidOperationException("Gruz Mother prefab does not match the supported native boss layout.");
            var death = boss.GetComponent<EnemyDeathEffects>();
            var prefab = CorpsePrefab.GetValue(death) as GameObject;
            var animation = prefab != null ? prefab.GetComponents<PlayMakerFSM>().FirstOrDefault(fsm => fsm.FsmName == "corpse") : null;
            if (animation == null || !new[] { "Init", "Steam", "Ready", "Blow", "Destroy Self" }.All(name => animation.FsmStates.Any(state => state.Name == name)))
                throw new InvalidOperationException("Gruz Mother's native death animation prefab is missing or unsupported.");
        }
        internal static GameObject Prepare(GameObject boss)
        {
            Validate(boss);
            var control = boss.GetComponents<PlayMakerFSM>().Single(fsm => fsm.FsmName == "Big Fly Control");
            var wake = control.FsmStates.Single(state => state.Name == "Wake");
            // Keep the native title's Visited flag and Area Event = BIGFLY.
            // Without them, the shared Godhome title displays placeholder text.
            // Only remove dependencies on the original room's battle controller.
            wake.Actions = wake.Actions.Where(action => !(action is DestroyObject)
                && !(action is SendEventByName send && send.sendEvent.Value == "START")).ToArray();
            // Keep the normal Godhome corpse animation, blood, sound and shake.
            // PreInstantiate creates an owned copy, so editing it never changes
            // the shared corpse prefab used by ordinary Gruz Mother fights.
            var death = boss.GetComponent<EnemyDeathEffects>();
            death.PreInstantiate();
            var corpse = Corpse.GetValue(death) as GameObject;
            var corpseControl = corpse != null ? corpse.GetComponents<PlayMakerFSM>().FirstOrDefault(fsm => fsm.FsmName == "corpse") : null;
            if (corpseControl == null) throw new InvalidOperationException("The native Gruz Mother death sequence is unavailable.");
            var init = corpseControl.FsmStates.FirstOrDefault(state => state.Name == "Init");
            if (init == null) throw new InvalidOperationException("The native Gruz Mother corpse initialization is unavailable.");
            // Only this action addresses the old room's absent Battle Scene.
            // Its Godhome branch already skips the normal-room baby encounter.
            init.Actions = init.Actions.Where(action => !(action is SetFsmBool)).ToArray();
            DeathBroadcast.SetValue(death, "");
            return corpse;
        }
        internal static void SuppressCompletion(GameObject boss)
        {
            var death = boss.GetComponent<EnemyDeathEffects>();
            if (death != null) DeathBroadcast.SetValue(death, "");
        }
        internal static GameObject PrepareVesselCorpse(GameObject boss)
        {
            SuppressCompletion(boss);
            var death = boss.GetComponent<EnemyDeathEffects>();
            death.PreInstantiate();
            var corpse = Corpse.GetValue(death) as GameObject;
            if (corpse == null) throw new InvalidOperationException("Broken Vessel death prefab missing.");
            foreach (var fsm in corpse.GetComponentsInChildren<PlayMakerFSM>(true))
                foreach (var state in fsm.FsmStates)
                    state.Actions = state.Actions.Where(a => !(a is SetPlayerDataBool)
                        && !(a is EndGGBossScene) && !(a is CallMethodProper)
                        && !(a is SetFsmBool set && set.fsmName.Value == "Battle Control")
                        && !(a is FindGameObject find && find.objectName.Value == "Battle Scene")
                        && !(a is SendEventByName send && (send.sendEvent.Value == "IK GATE OPEN" || send.sendEvent.Value == "STOP SPAWN"))).ToArray();
            return corpse;
        }
        internal static bool DeathFinished(GameObject corpse) => corpse == null || !corpse.activeInHierarchy ||
            corpse.GetComponents<PlayMakerFSM>().Any(f => f.FsmName == "corpse" && f.ActiveStateName == "BG Open");
        internal static void StopDeathEffects()
        {
            var camera = GameObject.Find("CameraParent");
            if (camera == null) return;
            var shake = camera.GetComponents<PlayMakerFSM>().FirstOrDefault(fsm => fsm.FsmName == "CameraShake");
            if (shake == null) return;
            var rumble = shake.FsmVariables.FindFsmBool("RumblingMed");
            if (rumble != null) rumble.Value = false;
            shake.SendEvent("StopRumble");
        }
    }
}
