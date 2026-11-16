using System;
using System.Collections.Generic;
using System.Linq;

namespace Gauntlet.Core
{
    public enum RunPhase { Idle, Combat, ChoosingCard, Victory, Defeat, Abandoned }
    public enum Upgrade { Nail, Soul, Spell, Recovery, Shell, LastStand, Mask, QuickSlash, Longnail, ShamanStone, SpellTwister, SoulCatcher, Grubsong, QuickFocus, SteadyBody, Focus, Dash, WallJump, DoubleJump, ShadowDash, SuperDash, AcidArmour, DreamNail, Fireball, Quake, Scream, Cyclone, DashSlash, GreatSlash, Strength, Joni, FragileHeart, MarkOfPride, Sprintmaster, Dashmaster, SharpShadow, NailmastersGlory, DreamWielder, SoulEater, DeepFocus, Thorns, DefenderCrest, Flukenest, HeavyBlow, Fury, BaldurShell, Hiveblood, Grubberfly, Weaversong, GlowingWomb, Grimmchild, Vessel, Kingsoul }

    public sealed class Card
    {
        public Upgrade Id { get; }
        public string Title { get; }
        public string Description { get; }
        public int MaxStacks { get; }
        public int CharmId { get; }
        public Card(Upgrade id, string title, string description, int maxStacks = 3, int charmId = 0)
        { Id = id; Title = title; Description = description; MaxStacks = maxStacks; CharmId = charmId; }
    }

    // A bucket is one upgrade family, not one lottery entry per remaining rank.
    // Only its next rank can be taken. Hiveblood (1) and Pale Ore (8) get one vote.
    public sealed class UpgradeBucket
    {
        public Card Card { get; }
        public Upgrade Id => Card.Id;
        public int RankCount => Card.MaxStacks;
        public UpgradeBucket(Card card) { Card = card; }
        public bool HasNextRank(int rank) => rank < RankCount;
    }

    public static class Cards
    {
        public static bool IsMainStat(Upgrade id) => id == Upgrade.Nail || id == Upgrade.Mask || id == Upgrade.Soul || id == Upgrade.Spell;
        public static IReadOnlyList<Card> All { get; } = Array.AsReadOnly(new[]
        {
            new Card(Upgrade.Nail, "Pale Ore", "+4 nail damage per rank, through 37 damage at rank 8.", 8),
            new Card(Upgrade.Soul, "Soul Well", "+3 Soul per nail hit per stack.", 3),
            new Card(Upgrade.Spell, "Arcane Spark", "+5 spell damage per stack.", 3),
            new Card(Upgrade.Recovery, "Quiet Respite", "Heal 1 mask per stack after choosing a card."),
            new Card(Upgrade.Shell, "Patient Shell", "Block every 6th / 5th / 4th damaging hit."),
            new Card(Upgrade.LastStand, "Last Light", "+15 nail damage per stack at 1 mask."),
            new Card(Upgrade.Mask, "Mask Shards", "Four shards form a vessel: +1 maximum mask and heal 1.", 4),
            new Card(Upgrade.QuickSlash, "Quick Slash", "Charm boon: slash faster. No notch cost.", 1, 32),
            new Card(Upgrade.Longnail, "Longnail", "Charm boon: increase nail reach. No notch cost.", 1, 18),
            new Card(Upgrade.ShamanStone, "Shaman Stone", "Empower spells. Ranks 2–5 add +10 spell damage each.", 5, 19),
            new Card(Upgrade.SpellTwister, "Spell Twister", "Charm boon: reduce spell Soul cost. No notch cost.", 1, 33),
            new Card(Upgrade.SoulCatcher, "Soul Catcher", "Charm boon: gain more Soul from hits. No notch cost.", 1, 20),
            new Card(Upgrade.Grubsong, "Grubsong", "Charm boon: gain Soul when hurt. No notch cost.", 1, 3),
            new Card(Upgrade.QuickFocus, "Quick Focus", "Charm boon: focus to heal faster. No notch cost.", 1, 7),
            new Card(Upgrade.SteadyBody, "Steady Body", "Charm boon: resist nail-hit recoil. No notch cost.", 1, 14),
            new Card(Upgrade.Vessel, "Vessel Fragments", "Three fragments form a vessel: +33 reserve Soul capacity.", 3),
            new Card(Upgrade.Focus, "Focus", "Unlock holding Focus to heal. Joni prevents healing.", 1, 0),
            new Card(Upgrade.Dash, "Mothwing Cloak", "Unlock dash.", 1, 0),
            new Card(Upgrade.DoubleJump, "Monarch Wings", "Unlock double jump.", 1, 0),
            new Card(Upgrade.ShadowDash, "Shade Cloak", "Upgrade your dash to pass through enemies.", 1, 0),
            new Card(Upgrade.SuperDash, "Crystal Heart", "Unlock crystal dash.", 1, 0),
            new Card(Upgrade.AcidArmour, "Isma’s Tear", "Unlock acid resistance.", 1, 0),
            new Card(Upgrade.DreamNail, "Dream Nail", "Unlock the Dream Nail to harvest Soul.", 1, 0),
            new Card(Upgrade.Fireball, "Vengeful Spirit", "Unlock projectile spell; rank 2 grants Shade Soul.", 2, 0),
            new Card(Upgrade.Quake, "Desolate Dive", "Unlock downward spell; rank 2 grants Descending Dark.", 2, 0),
            new Card(Upgrade.Scream, "Howling Wraiths", "Unlock upward spell; rank 2 grants Abyss Shriek.", 2, 0),
            new Card(Upgrade.Cyclone, "Cyclone Slash", "Unlock charged Cyclone Slash.", 1, 0),
            new Card(Upgrade.DashSlash, "Dash Slash", "Unlock charged Dash Slash. Requires dash.", 1, 0),
            new Card(Upgrade.GreatSlash, "Great Slash", "Unlock charged Great Slash.", 1, 0),
            new Card(Upgrade.Strength, "Fragile Strength", "+50% nail damage. Breaks on damage; repairs after the wave.", 1, 25),
            new Card(Upgrade.Joni, "Joni’s Blessing", "Convert masks to blue health. Refill every 5 cleared waves; cannot heal.", 1, 27),
            new Card(Upgrade.FragileHeart, "Fragile Heart", "+2 masks. Breaks on damage; repairs after the wave.", 1, 23),
            new Card(Upgrade.MarkOfPride, "Mark of Pride", "Greatly increase nail reach.", 1, 13),
            new Card(Upgrade.Sprintmaster, "Sprintmaster", "Move faster.", 1, 37),
            new Card(Upgrade.Dashmaster, "Dashmaster", "Dash more often and downward.", 1, 31),
            new Card(Upgrade.SharpShadow, "Sharp Shadow", "Damage enemies with Shade Cloak.", 1, 16),
            new Card(Upgrade.NailmastersGlory, "Nailmaster’s Glory", "Charge nail arts faster.", 1, 26),
            new Card(Upgrade.DreamWielder, "Dream Wielder", "Faster Dream Nail, more Soul.", 1, 30),
            new Card(Upgrade.SoulEater, "Soul Eater", "Gain much more Soul on nail hits.", 1, 21),
            new Card(Upgrade.DeepFocus, "Deep Focus", "Focus heals two masks, more slowly.", 1, 34),
            new Card(Upgrade.Thorns, "Thorns of Agony", "Retaliate when hurt. +10 thorn damage per extra rank.", 5, 12),
            new Card(Upgrade.DefenderCrest, "Defender’s Crest", "Emit a damaging cloud.", 1, 10),
            new Card(Upgrade.Flukenest, "Flukenest", "Replace projectile spells with flukes.", 1, 11),
            new Card(Upgrade.HeavyBlow, "Heavy Blow", "Increase nail knockback.", 1, 15),
            new Card(Upgrade.Fury, "Fury of the Fallen", "More nail damage at one mask.", 1, 6),
            new Card(Upgrade.BaldurShell, "Baldur Shell", "Protection while focusing; renews each wave.", 1, 5),
            new Card(Upgrade.Hiveblood, "Hiveblood", "Regenerate the most recently lost mask.", 1, 29),
            new Card(Upgrade.Grubberfly, "Grubberfly’s Elegy", "Nail beams at full health.", 1, 35),
            new Card(Upgrade.Weaversong, "Weaversong", "Summon little weaver allies.", 1, 39),
            new Card(Upgrade.GlowingWomb, "Glowing Womb", "Spend Soul to hatch attacking allies.", 1, 22),
            new Card(Upgrade.Grimmchild, "Grimmchild", "Summon a fully grown companion.", 1, 40),
            new Card(Upgrade.Kingsoul, "Kingsoul", "Generate Soul over time. No notch cost.", 1, 36)
        });
    }

    public static class RewardBuckets
    {
        public static IReadOnlyList<UpgradeBucket> All { get; } = Array.AsReadOnly(Cards.All.Select(card => new UpgradeBucket(card)).ToArray());
    }

    public enum EnemyKind { Hornhead, Aspid, Hopper, GreatHopper, Crawlid, Vengefly, GruzMother, PrimalAspid, MantisWarrior, WingedSentry, HeavySentry, BrokenVessel }

    public sealed class EncounterEnemy
    {
        public EnemyKind Kind { get; }
        // Zero preserves the prefab’s native health.
        public int Health { get; }
        public EncounterEnemy(EnemyKind kind, int health = 0)
        {
            if (health < 0) throw new ArgumentOutOfRangeException(nameof(health));
            Kind = kind; Health = health;
        }
    }

    public sealed class Wave
    {
        public int Number { get; }
        public string Title { get; }
        public IReadOnlyList<EncounterEnemy> Enemies { get; }
        public int EnemyCount => Enemies.Count;
        public Wave(int number, int enemyCount, int enemyHealth)
            : this(number, "Trial " + number, Enumerable.Range(0, enemyCount)
                .Select(_ => new EncounterEnemy(EnemyKind.Hornhead, enemyHealth)).ToArray()) { }
        public Wave(int number, string title, params EncounterEnemy[] enemies)
        {
            if (number < 1 || enemies.Length == 0 || enemies.Any(e => e == null))
                throw new ArgumentOutOfRangeException(nameof(number));
            Number = number; Title = title; Enemies = Array.AsReadOnly(enemies.ToArray());
        }
    }

    // Arrivals are terrain-safe, but crowding never blocks reinforcements.
    public static class SpawnSafety
    {
        public const float WarningSeconds = 0.2f;
        public const float WallInset = 4f;
        public const float MinimumArenaWidth = 14f;
        public const float StaggerSeconds = 0.8f;
        public static float Pick(float left, float right, int index)
        {
            if (right - left < MinimumArenaWidth) throw new ArgumentOutOfRangeException(nameof(right));
            return index % 2 == 0 ? left + WallInset : right - WallInset;
        }
        public static float FitHeight(float root, float bottom, float top, float floor, float roof)
        {
            if (top - bottom > roof - floor - 0.3f)
                throw new InvalidOperationException("Enemy collider does not fit the arena height.");
            float shift = Math.Max(0, floor + 0.15f - bottom);
            shift = Math.Min(shift, roof - 0.15f - top);
            return root + shift;
        }
    }

    // Pure rules: scene objects never enter this class. Tokens reject pooled or
    // delayed callbacks from an earlier wave/run, even if enemy indices repeat.
    public sealed class Run
    {
        private readonly Dictionary<Upgrade, int> stacks = new Dictionary<Upgrade, int>();
        private sealed class ActiveWave
        {
            public Wave Definition { get; }
            public ActiveWave(Wave definition) { Definition = definition; }
            public readonly HashSet<int> Living = new HashSet<int>();
        }
        private readonly Dictionary<long, ActiveWave> activeWaves = new Dictionary<long, ActiveWave>();
        private int pendingRewards;
        private int rewardLevel;
        public int RewardLevel => rewardLevel;
        public int ClearedWaveCount { get; private set; }
        public int TemporaryXpBonusPercent { get; private set; }
        public int PermanentXpBonusPercent { get; private set; }
        public int TotalXpBonusPercent => checked(TemporaryXpBonusPercent + PermanentXpBonusPercent);
        public int Experience { get; private set; }
        public int Level => CharacterLevels.ForExperience(Experience);
        public int PendingRewards => pendingRewards;
        public bool CanCallNextWave => Phase == RunPhase.Combat && WaveIndex < waves.Count - 1;
        private readonly HashSet<int> disabledCharms = new HashSet<int>();
        private readonly HashSet<Upgrade> lastOffer = new HashSet<Upgrade>();
        private readonly IReadOnlyList<Wave> waves;
        private uint randomState;
        private int hitCount;
        public RunPhase Phase { get; private set; } = RunPhase.Idle;
        public int Seed { get; private set; }
        public long WaveToken { get; private set; }
        public int WaveIndex { get; private set; }
        public Wave CurrentWave => waves[WaveIndex];
        public int WaveCount => waves.Count;
        public int EnemiesRemaining => activeWaves.Values.Sum(w => w.Living.Count);
        public bool IsActive => Phase == RunPhase.Combat || Phase == RunPhase.ChoosingCard;
        public IReadOnlyList<Card> Offer { get; private set; } = Array.Empty<Card>();
        public int Stack(Upgrade id) => stacks.TryGetValue(id, out int value) ? value : 0;

        public Run() : this(TrialWaves.Create()) { }

        public Run(IEnumerable<Wave> definitions)
        {
            var copy = definitions.ToArray();
            if (copy.Length == 0) throw new ArgumentException("At least one wave is required.");
            waves = Array.AsReadOnly(copy);
        }

        public void Start(int seed)
        {
            if (IsActive) throw new InvalidOperationException("End the current run first.");
            stacks.Clear(); disabledCharms.Clear(); lastOffer.Clear(); hitCount = 0; Seed = seed;
            randomState = unchecked((uint)seed) ^ 0x9E3779B9u;
            if (randomState == 0) randomState = 1;
            WaveIndex = 0; Experience = 0; pendingRewards = 0; rewardLevel = 0;
            ClearedWaveCount = 0; TemporaryXpBonusPercent = PermanentXpBonusPercent = 0;
            stacks[Upgrade.WallJump] = 1;
            activeWaves.Clear();
            BeginWave();
        }

        private void BeginWave()
        {
            WaveToken = checked(WaveToken + 1);
            var wave = new ActiveWave(CurrentWave);
            for (int i = 0; i < CurrentWave.EnemyCount; i++) wave.Living.Add(i);
            activeWaves.Add(WaveToken, wave);
            Offer = Array.Empty<Card>();
            Phase = RunPhase.Combat;
        }

        public bool CallNextWave()
        {
            if (!CanCallNextWave) return false;
            TemporaryXpBonusPercent = checked(TemporaryXpBonusPercent + 40);
            WaveIndex++; BeginWave();
            return true;
        }

        public int KillExperience(EnemyKind kind) => checked((int)(
            EnemyExperience.For(kind) * (100L + TotalXpBonusPercent) / 100));

        public bool EnemyKilled(long token, int enemyId)
        {
            // More than one native death callback can arrive in the frame that
            // triggers a level-up. Keep accepting owned kills during that freeze.
            if (!IsActive || !activeWaves.TryGetValue(token, out var wave)
                || !wave.Living.Remove(enemyId)) return false;
            int previousLevel = Level;
            Experience = checked(Experience + KillExperience(wave.Definition.Enemies[enemyId].Kind));
            pendingRewards = checked(pendingRewards + Level - previousLevel);
            if (wave.Living.Count == 0)
            {
                activeWaves.Remove(token);
                ClearedWaveCount++;
                // Credit the clearing kill with the current bonus, then reset.
                TemporaryXpBonusPercent = 0;
            }
            if (pendingRewards > 0)
            {
                if (Phase != RunPhase.ChoosingCard)
                {
                    rewardLevel = previousLevel + 1;
                    Offer = DraftCards();
                    Phase = RunPhase.ChoosingCard;
                }
            }
            else ContinueCombat();
            return true;
        }
        private void ContinueCombat()
        {
            Offer = Array.Empty<Card>();
            Phase = RunPhase.Combat;
            if (EnemiesRemaining > 0) return;
            if (WaveIndex == waves.Count - 1) End(RunPhase.Victory);
            else { WaveIndex++; BeginWave(); }
        }

        public IReadOnlyList<UpgradeBucket> EligibleBuckets => Array.AsReadOnly(RewardBuckets.All
            .Where(bucket => bucket.HasNextRank(Stack(bucket.Id)) && CanOffer(bucket.Id)).ToArray());

        private IReadOnlyList<Card> DraftCards()
        {
            if (rewardLevel == 2)
                return Remember(new[] { Upgrade.Focus, Upgrade.Dash, Upgrade.Fireball }
                    .Select(id => Cards.All.First(card => card.Id == id)).ToArray());
            var eligible = EligibleBuckets.ToList();
            var fresh = eligible.Where(bucket => !lastOffer.Contains(bucket.Id)).ToList();
            var offer = new List<Card>();
            while (offer.Count < 3 && eligible.Count > 0)
            {
                // Equal probability per family. Avoid the preceding offer when
                // possible; fall back only when the remaining pool is too small.
                var pool = fresh.Count > 0 ? fresh : eligible;
                var bucket = pool[Next(pool.Count)];
                offer.Add(bucket.Card); fresh.Remove(bucket); eligible.Remove(bucket);
            }
            return Remember(offer.ToArray());
        }
        private IReadOnlyList<Card> Remember(Card[] offer)
        {
            lastOffer.Clear();
            foreach (var card in offer) lastOffer.Add(card.Id);
            return Array.AsReadOnly(offer);
        }
        public static bool IsAbility(Upgrade id) => id >= Upgrade.Focus && id <= Upgrade.GreatSlash;
        private bool CanOffer(Upgrade id)
        {
            if (id == Upgrade.ShadowDash || id == Upgrade.Dashmaster || id == Upgrade.DashSlash) return Stack(Upgrade.Dash) > 0;
            if (id == Upgrade.SharpShadow) return Stack(Upgrade.ShadowDash) > 0;
            if (id == Upgrade.QuickFocus || id == Upgrade.DeepFocus || id == Upgrade.BaldurShell) return Stack(Upgrade.Focus) > 0 && !IsCharmEnabled(27);
            if (id == Upgrade.NailmastersGlory) return Stack(Upgrade.Cyclone) + Stack(Upgrade.GreatSlash) + Stack(Upgrade.DashSlash) > 0;
            if (id == Upgrade.DreamWielder) return Stack(Upgrade.DreamNail) > 0;
            if (id == Upgrade.ShamanStone || id == Upgrade.SpellTwister || id == Upgrade.Flukenest || id == Upgrade.Spell)
                return Stack(Upgrade.Fireball) + Stack(Upgrade.Quake) + Stack(Upgrade.Scream) > 0;
            return true;
        }
        // Explicit xorshift32 avoids depending on framework-specific Random behavior.
        private int Next(int exclusiveMax)
        {
            randomState ^= randomState << 13;
            randomState ^= randomState >> 17;
            randomState ^= randomState << 5;
            return (int)(randomState % (uint)exclusiveMax);
        }

        public void Choose(Upgrade id)
        {
            if (Phase != RunPhase.ChoosingCard || !Offer.Any(c => c.Id == id))
                throw new InvalidOperationException("Choose one of the offered cards.");
            stacks[id] = Stack(id) + 1;
            Advance();
        }

        public void Skip()
        {
            if (Phase != RunPhase.ChoosingCard) throw new InvalidOperationException("No reward pending.");
            PermanentXpBonusPercent = checked(PermanentXpBonusPercent + 10);
            Advance();
        }

        private void Advance()
        {
            if (--pendingRewards > 0)
            {
                rewardLevel++;
                Offer = DraftCards();
                return;
            }
            ContinueCombat();
        }
        public void Defeat() { if (IsActive) End(RunPhase.Defeat); }
        public void Abandon() { if (IsActive) End(RunPhase.Abandoned); }
        private void End(RunPhase phase)
        {
            Phase = phase; activeWaves.Clear(); pendingRewards = 0; TemporaryXpBonusPercent = PermanentXpBonusPercent = 0; Offer = Array.Empty<Card>();
            stacks.Clear(); disabledCharms.Clear(); lastOffer.Clear(); hitCount = 0;
        }

        public int NailBonus(int health) => !IsActive ? 0 :
            health == 1 ? 15 * Stack(Upgrade.LastStand) : 0;
        public int NailTier => IsActive ? Stack(Upgrade.Nail) : 0;
        public int BaseNailDamage => 5 + 4 * NailTier;
        public int MaximumMasks => 5 + (IsActive ? Stack(Upgrade.Mask) : 0);
        public IReadOnlyList<int> OwnedCharmIds => Array.AsReadOnly(Cards.All
            .Where(card => IsActive && card.CharmId != 0 && Stack(card.Id) > 0)
            .Select(card => card.CharmId).ToArray());
        public IReadOnlyList<int> CharmIds => Array.AsReadOnly(OwnedCharmIds.Where(IsCharmEnabled).ToArray());
        public bool IsCharmEnabled(int id) => IsActive && !disabledCharms.Contains(id) &&
            Cards.All.Any(c => c.CharmId == id && Stack(c.Id) > 0);
        public bool SetCharmEnabled(int id, bool enabled)
        {
            if (!IsActive || !OwnedCharmIds.Contains(id)) return false;
            if (enabled) disabledCharms.Remove(id); else disabledCharms.Add(id);
            return true;
        }
        public int SpellBonus => IsActive ? 5 * Stack(Upgrade.Spell) + (IsCharmEnabled(19) ? 10 * Math.Max(0, Stack(Upgrade.ShamanStone) - 1) : 0) : 0;
        public int ThornsBonus => IsCharmEnabled(12) ? 10 * Math.Max(0, Stack(Upgrade.Thorns) - 1) : 0;
        public int SoulBonus => IsActive ? 3 * Stack(Upgrade.Soul) : 0;
        public bool CanFocus => IsActive && Stack(Upgrade.Focus) > 0 && !IsCharmEnabled(27);
        public int Recovery => IsActive ? Stack(Upgrade.Recovery) : 0;

        public int IncomingDamage(int amount)
        {
            if (Phase != RunPhase.Combat || amount <= 0 || Stack(Upgrade.Shell) == 0) return amount;
            hitCount++;
            return hitCount % (7 - Stack(Upgrade.Shell)) == 0 ? 0 : amount;
        }
    }
}
