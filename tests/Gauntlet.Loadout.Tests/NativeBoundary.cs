// Minimal native boundary for exercising the real RunLoadout adapter without
// a running Unity engine. CharmUpdate deliberately refills health like the
// inspected legacy implementation, so preservation regressions are observable.
using System;
using System.Collections.Generic;
namespace UnityEngine { internal sealed class TestBoundary { } }
public sealed class PlayerData
{
    public static PlayerData instance = new PlayerData();
    public float completionPercent = 0;
    public bool hasDash;
    public bool hasWalljump;
    public bool hasSuperDash;
    public bool hasShadowDash;
    public bool hasAcidArmour;
    public bool hasDoubleJump;
    public bool hasDreamNail;
    public bool hasCyclone;
    public bool hasDashSlash;
    public bool hasUpwardSlash;
    public bool canDash;
    public bool canBackDash;
    public bool canWallJump;
    public bool canSuperDash;
    public bool canShadowDash;
    public bool hasSpell;
    public bool hasNailArt;
    public bool hasAllNailArts;
    public bool hasDreamGate;
    public bool dreamNailUpgraded;
    public bool brokenCharm_23;
    public bool brokenCharm_24;
    public bool brokenCharm_25;
    public int fireballLevel;
    public int quakeLevel;
    public int screamLevel;
    public int grimmChildLevel;
    public int royalCharmState;
    public int blockerHits;
    public int maxMP, MPReserveMax, MPReserveCap, vesselFragments, focusMP_amount;
    public bool vesselFragmentCollected, vesselFragmentMax;
    public int unrelatedQuest = 0;
    public List<int> equippedCharms = new List<int>();
    public int health = 0;
    public int maxHealth = 0;
    public int maxHealthBase = 0;
    public int maxHealthCap = 0;
    public int healthBlue = 0;
    public int joniHealthBlue = 0;
    public int prevHealth = 0;
    public int heartPieces = 0;
    public int nailDamage = 0;
    public int nailSmithUpgrades = 0;
    public int charmSlots = 0;
    public int charmSlotsFilled = 0;
    public int charmsOwned = 0;
    public int MPCharge = 0;
    public int MPReserve = 0;
    public bool damagedBlue = false;
    public bool heartPieceCollected = false;
    public bool heartPieceMax = false;
    public bool hasCharm = false;
    public bool canOvercharm = false;
    public bool overcharmed = false;
    public bool gotCharm_1 = false;
    public bool equippedCharm_1 = false;
    public bool gotCharm_2 = false;
    public bool equippedCharm_2 = false;
    public bool gotCharm_3 = false;
    public bool equippedCharm_3 = false;
    public bool gotCharm_4 = false;
    public bool equippedCharm_4 = false;
    public bool gotCharm_5 = false;
    public bool equippedCharm_5 = false;
    public bool gotCharm_6 = false;
    public bool equippedCharm_6 = false;
    public bool gotCharm_7 = false;
    public bool equippedCharm_7 = false;
    public bool gotCharm_8 = false;
    public bool equippedCharm_8 = false;
    public bool gotCharm_9 = false;
    public bool equippedCharm_9 = false;
    public bool gotCharm_10 = false;
    public bool equippedCharm_10 = false;
    public bool gotCharm_11 = false;
    public bool equippedCharm_11 = false;
    public bool gotCharm_12 = false;
    public bool equippedCharm_12 = false;
    public bool gotCharm_13 = false;
    public bool equippedCharm_13 = false;
    public bool gotCharm_14 = false;
    public bool equippedCharm_14 = false;
    public bool gotCharm_15 = false;
    public bool equippedCharm_15 = false;
    public bool gotCharm_16 = false;
    public bool equippedCharm_16 = false;
    public bool gotCharm_17 = false;
    public bool equippedCharm_17 = false;
    public bool gotCharm_18 = false;
    public bool equippedCharm_18 = false;
    public bool gotCharm_19 = false;
    public bool equippedCharm_19 = false;
    public bool gotCharm_20 = false;
    public bool equippedCharm_20 = false;
    public bool gotCharm_21 = false;
    public bool equippedCharm_21 = false;
    public bool gotCharm_22 = false;
    public bool equippedCharm_22 = false;
    public bool gotCharm_23 = false;
    public bool equippedCharm_23 = false;
    public bool gotCharm_24 = false;
    public bool equippedCharm_24 = false;
    public bool gotCharm_25 = false;
    public bool equippedCharm_25 = false;
    public bool gotCharm_26 = false;
    public bool equippedCharm_26 = false;
    public bool gotCharm_27 = false;
    public bool equippedCharm_27 = false;
    public bool gotCharm_28 = false;
    public bool equippedCharm_28 = false;
    public bool gotCharm_29 = false;
    public bool equippedCharm_29 = false;
    public bool gotCharm_30 = false;
    public bool equippedCharm_30 = false;
    public bool gotCharm_31 = false;
    public bool equippedCharm_31 = false;
    public bool gotCharm_32 = false;
    public bool equippedCharm_32 = false;
    public bool gotCharm_33 = false;
    public bool equippedCharm_33 = false;
    public bool gotCharm_34 = false;
    public bool equippedCharm_34 = false;
    public bool gotCharm_35 = false;
    public bool equippedCharm_35 = false;
    public bool gotCharm_36 = false;
    public bool equippedCharm_36 = false;
    public bool gotCharm_37 = false;
    public bool equippedCharm_37 = false;
    public bool gotCharm_38 = false;
    public bool equippedCharm_38 = false;
    public bool gotCharm_39 = false;
    public bool equippedCharm_39 = false;
    public bool gotCharm_40 = false;
    public bool equippedCharm_40 = false;
}
public sealed class SaveGameData
{
    public PlayerData playerData;
    public SaveGameData(PlayerData data) { playerData = data; }
}
public sealed class HeroController
{
    public static HeroController instance = new HeroController();
    public int RefreshCount;
    public void CharmUpdate()
    {
        RefreshCount++;
        var pd = PlayerData.instance;
        pd.maxHealth = pd.maxHealthBase + (pd.equippedCharm_23 && !pd.brokenCharm_23 ? 2 : 0);
        pd.joniHealthBlue = pd.equippedCharm_27 ? (int)(pd.maxHealth * 1.4f) : 0;
        if (pd.equippedCharm_27) pd.maxHealth = 1;
        pd.health = pd.maxHealth;
        pd.healthBlue = 0;
    }
    public void AddHealth(int amount)
    { PlayerData.instance.health = Math.Min(PlayerData.instance.maxHealth, PlayerData.instance.health + amount); }
}
public static class PlayMakerFSM
{
    public static readonly List<string> Events = new List<string>();
    public static Action<string> OnBroadcast;
    public static void BroadcastEvent(string name)
    {
        Events.Add(name);
        OnBroadcast?.Invoke(name);
        // BlueHealthHud owns the native boundary during a run: rebuilding is visual only.
    }
}
namespace Modding
{
    public static class ModHooks
    {
        public static event Action<SaveGameData> BeforeSavegameSaveHook;
        public static int SubscriberCount => BeforeSavegameSaveHook?.GetInvocationList().Length ?? 0;
        public static SaveGameData Save()
        {
            var save = new SaveGameData(PlayerData.instance);
            BeforeSavegameSaveHook?.Invoke(save);
            return save;
        }
    }
}
