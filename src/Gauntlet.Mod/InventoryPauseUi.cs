using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Gauntlet.Mod
{
    // Keep native inventory and health animations running during combat freezes.
    internal sealed class InventoryPauseUi : IDisposable
    {
        private readonly Func<bool> frozen;
        private readonly UiClockSettings settings = new UiClockSettings();
        internal InventoryPauseUi(Func<bool> frozen)
        {
            this.frozen = frozen;
            IL.FadeGroup.Update += UnscaledUi;
            IL.InvAnimateUpAndDown.Update += UnscaledUi;
            IL.tk2dSpriteAnimator.LateUpdate += UnscaledUi;
        }
        private void UnscaledUi(ILContext il)
        {
            var cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instruction => instruction.MatchCall(typeof(Time), "get_deltaTime")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Component, float>>(Delta);
            }
        }
        private float Delta(float scaled, Component component)
        {
            if (!frozen()) return scaled;
            for (var parent = component.transform; parent != null; parent = parent.parent)
                if (parent.name == "Inventory" || parent.name == "HudCamera" || parent.name == "Hud Camera")
                    return Time.unscaledDeltaTime;
            return scaled;
        }
        internal void Prepare(GameObject root, bool healthOnly = false) => settings.Prepare(root, healthOnly);
        internal void Restore() => settings.Restore();
        public void Dispose()
        {
            Restore();
            IL.FadeGroup.Update -= UnscaledUi;
            IL.InvAnimateUpAndDown.Update -= UnscaledUi;
            IL.tk2dSpriteAnimator.LateUpdate -= UnscaledUi;
        }
    }
}
