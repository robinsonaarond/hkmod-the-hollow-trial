using System.Collections;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class StageSpike : MonoBehaviour
    {
        private IEnumerator Start()
        {
            // Native Init must bind Dust before the warned expansion starts.
            yield return null;
            foreach (var fsm in GetComponents<PlayMakerFSM>()) fsm.SendEvent("EXPAND");
        }
    }
}
