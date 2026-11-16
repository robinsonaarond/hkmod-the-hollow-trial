using System.Linq;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal static class HealthHud
    {
        internal static void Refresh()
        {
            var cameras = GameCameras.instance;
            if (cameras == null || cameras.hudCanvas == null) return;
            // Health slots keep their native event/animation path. Forcing
            // Check Type/Break? while a clip is running abandons its completion
            // callback and can leave a mask halfway through a break/refill.
            foreach (var fsm in cameras.hudCanvas.GetComponentsInChildren<PlayMakerFSM>(true))
            {
                if (!fsm.enabled || !fsm.gameObject.activeInHierarchy) continue;
                // Reserve vessels also cache ownership. Init reads MPReserveMax,
                // hides unearned orbs, and refreshes each earned orb's fill state.
                // Keep the GameObject/FSM active so future vessel cards can reveal it.
                if (fsm.FsmName == "vessel_orb" && fsm.FsmVariables.FindFsmInt("Reserve MP Max") != null
                    && fsm.FsmStates.Any(state => state.Name == "Init")) fsm.SetState("Init");
            }
        }
    }
}
