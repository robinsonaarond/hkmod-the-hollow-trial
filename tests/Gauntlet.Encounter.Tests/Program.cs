using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Gauntlet.Core;
using Gauntlet.Mod;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;

static class Program
{
    private static int checks;
    private static void Check(bool value, string message) { checks++; if (!value) throw new Exception(message); }
    private static void Call(object value, string method) => value.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(value, null);
    static void Main()
    {
        var arena = new ArenaController(); arena.Run.Start(1);
        HeroController.instance.transform.position = new Vector3(50, 3);
        foreach (var kind in new[] { EnemyKind.Crawlid, EnemyKind.Hornhead, EnemyKind.Vengefly, EnemyKind.Aspid, EnemyKind.PrimalAspid, EnemyKind.Hopper, EnemyKind.GreatHopper, EnemyKind.MantisWarrior, EnemyKind.WingedSentry, EnemyKind.HeavySentry })
        {
            var enemy = new GameObject(kind.ToString()); enemy.transform.position = new Vector3(36, 3);
            var hp = enemy.AddComponent<HealthManager>();
            var fsm = enemy.AddComponent<PlayMakerFSM>(); fsm.FsmName = kind == EnemyKind.Crawlid ? "Crawler" : kind == EnemyKind.PrimalAspid ? "spitter" : "Test";
            if (kind == EnemyKind.PrimalAspid) fsm.ActiveStateName = "Sleep";
            fsm.FsmVariables.Objects["Hero"] = new FsmGameObject();
            foreach (string name in new[] { "Start Alert", "startAlert", "Alerted", "Roams Within Range", "Start Idle", "Spawner" }) fsm.FsmVariables.Bools[name] = new FsmBool { Value = name == "Roams Within Range" };
            fsm.FsmVariables.Floats["Unalert Range"] = new FsmFloat { Value = 8 };
            var sleep = new FloatCompare { float2 = new FsmFloat { Value = 12 } };
            var primalSleep = new FloatCompare { float2 = new FsmFloat { Value = 20 } };
            var pacification = new PlayerDataBoolTest { isTrue = "TRUE", isFalse = "FALSE" };
            var attack = new FloatCompare { float2 = new FsmFloat { Value = 3 } };
            fsm.FsmStates = new[] { new FsmState { Name = "Range?", Actions = new[] { sleep } }, new FsmState { Name = "Attack", Actions = new[] { attack } }, new FsmState { Name = "Idle", Actions = new[] { primalSleep } }, new FsmState { Name = "Lords Defeated?", Actions = new FsmStateAction[] { pacification } } };
            var walker = enemy.AddComponent<Walker>();
            foreach (string name in new[] { "Alert Range", "Alert Range New", "Unalert Range", "Attack Range", "Unrelated" })
            {
                var child = new GameObject(name); child.transform.lossyScale = new Vector3(-2, 0.5f);
                var circle = child.AddComponent<CircleCollider2D>(); circle.radius = 2; circle.isTrigger = true;
                var box = child.AddComponent<BoxCollider2D>(); box.size = new Vector2(2, 2); box.isTrigger = true;
                var solid = child.AddComponent<BoxCollider2D>(); solid.size = new Vector2(1, 1);
                enemy.transform.Children.Add(child);
            }
            var engagement = enemy.AddComponent<EnemyEngagement>(); engagement.Configure(arena, kind, 34);
            Check(hp.hp == 325 && attack.float2.Value == 3, "Engagement altered HP or attack distance");
            Check(fsm.FsmVariables.Objects["Hero"].Value == HeroController.instance.gameObject, "Arrival has no Knight target");
            Check(fsm.FsmVariables.Bools["Start Alert"].Value && fsm.FsmVariables.Bools["startAlert"].Value && !walker.startInactive, "Arrival starts asleep");
            Check(fsm.FsmVariables.Floats["Unalert Range"].Value >= 34, "Enemy can lose aggro across the arena");
            Check(primalSleep.float2.Value == (kind == EnemyKind.PrimalAspid ? 42 : 20), "Primal sleep distance changed outside its Idle state");
            Check(sleep.float2.Value == (kind == EnemyKind.Hopper ? 42 : 12), "Sleep threshold changed outside Hopper's range state");
            Check(fsm.FsmVariables.Bools["Alerted"].Value == (kind == EnemyKind.GreatHopper), "Giant Hopper wake-up flag is wrong");
            foreach (var child in enemy.transform.Children)
            {
                bool awareness = child.name != "Attack Range" && child.name != "Unrelated";
                Check(child.GetComponent<CircleCollider2D>().radius == (awareness ? 84 : 2), "Circle awareness/attack range scope is wrong");
                var boxes = child.GetComponents<BoxCollider2D>();
                Check(boxes[0].size.x == (awareness ? 42 : 2) && boxes[0].size.y == (awareness ? 168 : 2), "Box awareness/attack range scope is wrong");
                Check(boxes[1].size.x == 1 && boxes[1].size.y == 1, "Solid terrain/body collider was resized");
            }
            Check(pacification.isTrue.Equals(kind == EnemyKind.MantisWarrior ? "FALSE" : "TRUE"), "Mantis save pacification was not isolated to trial warriors");
            if (kind == EnemyKind.WingedSentry) Check(fsm.FsmVariables.Bools["Start Idle"].Value, "Winged Sentry remained attached to an old room wall");
            var start = (IEnumerator)typeof(EnemyEngagement).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(engagement, null);
            Check(start.MoveNext() && walker.LastGo == 0, "Walker was driven before native Start");
            start.MoveNext(); Check(walker.LastGo == 1, "Walker did not head toward Knight");
            if (kind == EnemyKind.WingedSentry) Check(fsm.Events.Contains("WAKE"), "Winged Sentry did not wake");
            if (kind == EnemyKind.HeavySentry) Check(fsm.Events.Contains("ALERT"), "Heavy Sentry did not chase");
            if (kind == EnemyKind.PrimalAspid) Check(fsm.Events.SequenceEqual(new[] { "WAKE" }), "Primal did not wake on arrival");
            if (kind == EnemyKind.Crawlid)
            {
                Check(fsm.Events.SequenceEqual(new[] { "GO RIGHT" }), "Crawlid did not initially target Knight");
                HeroController.instance.transform.position = new Vector3(32, 3); Time.time = 1;
                Call(engagement, "Update"); Check(fsm.Events.Last() == "GO LEFT", "Crawlid never reacquired Knight on the other side");
                int count = fsm.Events.Count; fsm.ActiveStateName = "Start L";
                HeroController.instance.transform.position = new Vector3(50, 3); Time.time = 2;
                Call(engagement, "Update"); Check(fsm.Events.Count == count, "Aggro interrupted a native turn animation");
                arena.Run.Abandon(); fsm.ActiveStateName = "Maintain"; Time.time = 3;
                Call(engagement, "Update"); Check(fsm.Events.Count == count, "Aggro continued after run end");
                arena.Run.Start(2);
            }
        }
        var boss = new GameObject("Gruz Mother"); var health = boss.AddComponent<HealthManager>();
        var death = boss.AddComponent<EnemyDeathEffects>(); var control = boss.AddComponent<PlayMakerFSM>();
        control.FsmName = "Big Fly Control";
        var cameraShake = new SendEventByName { sendEvent = new FsmString { Value = "AverageShake" } };
        var battleStart = new SendEventByName { sendEvent = new FsmString { Value = "START" } };
        var animation = new FsmStateAction();
        var titleVisited = new SetFsmBool();
        var titleKey = new SetFsmString { variableName = new FsmString { Value = "Area Event" }, setValue = new FsmString { Value = "BIGFLY" } };
        var wake = new FsmState { Name = "Wake", Actions = new FsmStateAction[] { titleVisited, titleKey, new DestroyObject(), battleStart, cameraShake, animation } };
        control.FsmStates = new[] { new FsmState { Name = "Init" }, new FsmState { Name = "GG Boss Wake" }, wake, new FsmState { Name = "Buzz" } };
        NativeBoss.Validate(boss); Check(death.HasCorpse && wake.Actions.Length == 6, "Validation mutated the preload template");
        var corpse = NativeBoss.Prepare(boss);
        Check(wake.Actions.SequenceEqual(new FsmStateAction[] { titleVisited, titleKey, cameraShake, animation }), "Boss adaptation removed native title/animation/shake or kept old arena actions");
        Check(titleKey.variableName.Value == "Area Event" && titleKey.setValue.Value == "BIGFLY", "Boss adaptation lost Gruz Mother's native localized title key");
        Check(death.HasCorpse && corpse != null && death.Broadcast == "", "Native corpse was removed or native completion broadcast remained");
        Check(death.PrefabInitActions == 2, "Preparing the death copy mutated the shared native prefab");
        var corpseControl = corpse.GetComponent<PlayMakerFSM>();
        Check(corpseControl.FsmStates[0].Actions.Length == 1 && corpseControl.FsmStates.Any(state => state.Name == "Steam") && corpseControl.FsmStates.Any(state => state.Name == "Blow"), "Corpse adaptation removed visual death states");
        corpse.SetActive(true); Check(!NativeBoss.DeathFinished(corpse), "Death sequence was considered finished while active");
        corpse.SetActive(false); Check(NativeBoss.DeathFinished(corpse), "Completed death sequence was never acknowledged");
        var camera = new GameObject("CameraParent"); var shake = camera.AddComponent<PlayMakerFSM>(); shake.FsmName = "CameraShake";
        shake.FsmVariables.Bools["RumblingMed"] = new FsmBool { Value = true };
        NativeBoss.StopDeathEffects();
        Check(!shake.FsmVariables.Bools["RumblingMed"].Value && shake.Events.Last() == "StopRumble", "Leaving during death kept camera rumbling");
        Check(health.hp == 325 && control.FsmStates.Length == 4, "Boss adaptation changed native HP or control states");
        bool rejected = false;
        try { NativeBoss.Validate(new GameObject("Missing boss FSM")); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "Unsupported boss layout was accepted");
        VerifyCardFreeze(); VerifyCharmMenu(); VerifyBell(); VerifyVessels(); VerifyStage(); VerifyCeiling(); VerifyLayouts(); VerifyHud(); VerifyUiClock(); VerifyBlueHud();
        Console.WriteLine($"PASS: {checks} real-adapter encounter assertions (awareness, target binding, attack/HP preservation, turns, boss adaptation).");
    }
    private static void VerifyCharmMenu()
    {
        var obj = new GameObject("Charm pane"); var menu = obj.AddComponent<PlayMakerFSM>(); menu.FsmName = "UI Charms";
        var bench = new PlayerDataBoolTest { boolName = new FsmString { Value = "atBench" } };
        var other = new PlayerDataBoolTest { boolName = new FsmString { Value = "brokenCharm_23" } };
        var refill = new SendEventByName { sendEvent = new FsmString { Value = "UPDATE BLUE HEALTH" } };
        var sound = new FsmStateAction();
        var actions = new FsmStateAction[] { bench, other, refill, sound };
        var state = new FsmState { Name = "Deactivate UI", Actions = actions }; menu.FsmStates = new[] { state };
        var inventory = new GameObject("Inventory").AddComponent<PlayMakerFSM>(); inventory.FsmName = "Inventory Control";
        var regain = new PlayerDataBoolTest { boolName = new FsmString { Value = "atBench" } };
        inventory.FsmStates = new[] { new FsmState { Name = "Regain Control", Actions = new FsmStateAction[] { regain } } };
        var patch = new BenchlessCharms(); patch.Apply(new[] { menu, inventory }); patch.Apply(new[] { menu, inventory });
        Check(bench.boolName.Value == "hasCharm" && regain.boolName.Value == "atBench" && other.boolName.Value == "brokenCharm_23", "Benchless patch changed unrelated bench/broken checks");
        Check(state.Actions.SequenceEqual(new FsmStateAction[] { bench, other, sound }), "Charm menu still broadcasts a second Joni refill or lost unrelated actions");
        patch.Restore(); patch.Restore();
        Check(bench.boolName.Value == "atBench" && ReferenceEquals(state.Actions, actions), "Charm menu patch did not restore original gates/events");
    }
    private static void VerifyCardFreeze()
    {
        var hero = new GameObject("Knight for freeze");
        var hatch = hero.AddComponent<PlayMakerFSM>(); hatch.FsmName = "Hatchling Spawn";
        var disabled = hero.AddComponent<PlayMakerFSM>(); disabled.FsmName = "Hatchling Spawn"; disabled.enabled = false;
        var spell = hero.AddComponent<PlayMakerFSM>(); spell.FsmName = "Spell Control";
        var freeze = new CardFreeze(); Time.timeScale = 0.7f;
        freeze.Apply(true, false, hero);
        Check(Time.timeScale == 0 && !hatch.enabled && !disabled.enabled && spell.enabled, "Card freeze missed hatching or disabled unrelated input");
        Time.timeScale = 1; freeze.Apply(true, false, hero);
        Check(Time.timeScale == 0, "Native pause-menu resume escaped card freeze");
        freeze.Apply(true, true, hero); freeze.Restore(true);
        Check(Time.timeScale == 0 && hatch.enabled && !disabled.enabled, "Card cleanup unpaused the pause menu or enabled an originally disabled hatch controller");
        Time.timeScale = 0.7f; freeze.Apply(true, false, hero); freeze.Apply(true, false, hero);
        freeze.Apply(false, false, hero);
        Check(Time.timeScale == 0.7f && hatch.enabled, "Repeated banked choices lost the original gameplay speed");
        freeze.Restore(false); Check(Time.timeScale == 0.7f, "Repeated restore changed gameplay speed");
        Time.timeScale = 1;
    }
    private static void VerifyBell()
    {
        var arena = new ArenaController(); arena.Run.Start(1);
        var obj = new GameObject("Bell"); var fsm = obj.AddComponent<PlayMakerFSM>(); fsm.FsmName = "Bell Control";
        var ring = new FsmState { Name = "Ring R", Actions = new FsmStateAction[] {
            new SendEventByName { sendEvent = new FsmString { Value = "STAG BELL RUNG" } }, new FsmStateAction() } };
        fsm.FsmStates = new[] { ring };
        var bell = obj.AddComponent<WaveBell>(); bell.Configure(arena);
        Check(ring.Actions.Length == 1, "Trial bell can summon a stag");
        fsm.ActiveStateName = "Detect"; Time.time = 0; Call(bell, "Update");
        fsm.ActiveStateName = "Ring R"; Call(bell, "Update");
        Check(arena.Run.CurrentWave.Number == 2 && arena.Run.EnemiesRemaining == 6, "Bell discarded the opening wave");
        Time.time = 1; Call(bell, "Update");
        Check(arena.Run.CurrentWave.Number == 2, "One ring called multiple waves");
        fsm.ActiveStateName = "Detect"; Call(bell, "Update"); fsm.ActiveStateName = "Ring L"; Call(bell, "Update");
        Check(arena.Run.CurrentWave.Number == 3 && arena.Run.EnemiesRemaining == 9, "Repeated rings cannot stack waves");
        arena.Run.Defeat(); fsm.ActiveStateName = "Detect"; Call(bell, "Update");
        fsm.ActiveStateName = "Ring R"; Time.time = 2; Call(bell, "Update");
        Check(arena.Run.Phase == RunPhase.Defeat, "Bell revived an ended run");
    }
    private static void VerifyVessels()
    {
        var boss = new GameObject("Broken Vessel"); var hp = boss.AddComponent<HealthManager>();
        var death = boss.AddComponent<EnemyDeathEffects>(); var control = boss.AddComponent<PlayMakerFSM>(); control.FsmName = "IK Control";
        foreach (var name in new[] { "Left X", "Right X", "Air Dash Height", "Min Dstab Height" }) control.FsmVariables.Floats[name] = new FsmFloat { Value = 30 };
        var sound = new FsmStateAction();
        control.FsmStates = new[] {
            new FsmState { Name = "Init", Actions = new FsmStateAction[] { new SetPlayerDataBool(), sound } },
            new FsmState { Name = "Close Gates", Actions = new FsmStateAction[] { new SendEventByName { sendEvent = new FsmString { Value = "BG CLOSE" } }, sound } },
            new FsmState { Name = "Set Pos", Actions = new FsmStateAction[] { new SetPosition(), sound } },
            new FsmState { Name = "Intro Fall" }, new FsmState { Name = "Idle" } };
        var vessel = boss.AddComponent<BrokenVessel>(); vessel.Configure(3, 37, 28);
        Check(control.FsmVariables.Floats["Left X"].Value == 5 && control.FsmVariables.Floats["Right X"].Value == 35, "Vessel retained its old arena limits");
        Check(Math.Abs(control.FsmVariables.Floats["Air Dash Height"].Value - 31.01f) < 0.001f, "Vessel attack heights did not follow the new floor");
        Check(control.FsmStates.Take(3).All(s => s.Actions.SequenceEqual(new[] { sound })) && hp.hp == 325 && death.Broadcast == "", "Vessel changed HP or retained old scene/save writes");
        var start = (IEnumerator)typeof(BrokenVessel).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(vessel, null);
        Check(start.MoveNext() && start.MoveNext() && control.StateChanges.Count == 0, "Vessel skipped native initialization");
        start.MoveNext(); Check(control.StateChanges.Last() == "Close Gates", "Vessel remained asleep");
        var corpse = NativeBoss.PrepareVesselCorpse(boss);
        var fsm = corpse.GetComponent<PlayMakerFSM>();
        fsm.ActiveStateName = "Steam"; corpse.SetActive(true);
        Check(!NativeBoss.DeathFinished(corpse), "Vessel death animation was skipped");
        fsm.ActiveStateName = "BG Open";
        Check(NativeBoss.DeathFinished(corpse), "Vessel death waits forever for a removed scene return");
    }
    private static void VerifyHud()
    {
        GameCameras.instance = null; HealthHud.Refresh();
        var hud = new GameObject("Hud Canvas"); GameCameras.instance = new GameCameras { hudCanvas = hud };
        int reserveMax = 99;
        var vessels = Enumerable.Range(1, 4).Select(n =>
        {
            var obj = new GameObject("Vessel " + n); hud.transform.Children.Add(obj);
            var fsm = obj.AddComponent<PlayMakerFSM>(); fsm.FsmName = "vessel_orb";
            fsm.FsmStates = new[] { new FsmState { Name = "Init" } };
            fsm.FsmVariables.Ints["Reserve MP Max"] = new FsmInt { Value = 99 };
            // Model the inspected native Init -> ownership comparison. Mesh
            // visibility changes, while the FSM stays available for later cards.
            fsm.OnState = _ => fsm.FsmVariables.Ints["Reserve MP Max"].Value = reserveMax >= n * 33 ? 1 : 0;
            return fsm;
        }).ToArray();
        var mask = hud.AddComponent<PlayMakerFSM>(); mask.FsmName = "health_display";
        mask.FsmVariables.Ints["Health Number"] = new FsmInt { Value = 9 };
        mask.FsmStates = new[] { new FsmState { Name = "Check Type" }, new FsmState { Name = "Check Max HP" } };
        mask.FsmVariables.Bools["Initialised"] = new FsmBool { Value = false };
        var maskRenderer = hud.AddComponent<Renderer>(); maskRenderer.enabled = false;
        var unrelated = hud.AddComponent<PlayMakerFSM>(); unrelated.FsmName = "Soul Orb Control";
        unrelated.FsmStates = new[] { new FsmState { Name = "Init" } };
        var blue = hud.AddComponent<PlayMakerFSM>(); blue.FsmName = "blue_health_display";
        blue.FsmStates = new[] { new FsmState { Name = "Break?" } };
        foreach (int capacity in new[] { 0, 33, 66, 99, 0, 99 })
        {
            reserveMax = capacity; blue.ActiveStateName = "Idle"; HealthHud.Refresh();
            Check(vessels.Count(v => v.FsmVariables.Ints["Reserve MP Max"].Value == 1) == capacity / 33,
                "Soul vessel ownership was not refreshed after reset/upgrade/restoration");
            Check(vessels.All(v => v.enabled && v.gameObject.activeInHierarchy && v.StateChanges.Last() == "Init"),
                "Vessel refresh disabled future upgrades or bypassed native ownership check");
            Check(mask.StateChanges.Count == 0, "HUD refresh interrupted a native health animation");
            maskRenderer.enabled = false;
            mask.FsmVariables.Bools["Initialised"].Value = false;
            Check(unrelated.StateChanges.Count == 0, "Refresh restarted the main Soul meter");
            Check(blue.StateChanges.Count == 0, "HUD refresh interrupted a blue-health animation");
        }
        vessels[0].enabled = false; int count = vessels[0].StateChanges.Count;
        HealthHud.Refresh(); Check(vessels[0].StateChanges.Count == count, "Refresh drove a disabled HUD controller");
        GameCameras.instance = null;
    }
    private static void VerifyUiClock()
    {
        var root = new GameObject("HUD clock fixture");
        var health = root.AddComponent<PlayMakerFSM>(); health.FsmName = "health_display";
        var wait = new Wait(); var shared = new FsmBool();
        health.FsmStates = new[] { new FsmState { Actions = new FsmStateAction[] { wait,
            new iTweenFsmAction { realTime = shared }, new iTweenFsmAction { realTime = shared } } } };
        var other = root.AddComponent<PlayMakerFSM>(); other.FsmName = "Soul gameplay controller";
        var otherWait = new Wait(); other.FsmStates = new[] { new FsmState { Actions = new FsmStateAction[] { otherWait } } };
        var settings = new UiClockSettings(); settings.Prepare(root, healthOnly: true);
        Check(wait.realTime && shared.Value && !otherWait.realTime, "Health animation waits still depend on the paused combat clock");
        settings.Prepare(root, healthOnly: true); settings.Restore();
        Check(!wait.realTime && !shared.Value && !otherWait.realTime, "UI clock did not restore original/shared timing settings");
        settings.Prepare(root); Check(otherWait.realTime, "Inventory transition wait was not prepared"); settings.Restore();
    }
    private static void VerifyBlueHud()
    {
        var root = new GameObject("Blue HUD fixture");
        var fsm = root.AddComponent<PlayMakerFSM>(); fsm.FsmName = "Blue Health Control";
        var blueCounter = new FsmInt(); var joniCounter = new FsmInt();
        var reset = new SendMessage { functionCall = new FunctionCall { FunctionName = "UpdateBlueHealth" } };
        var readBlue = new GetPlayerDataInt { intName = new FsmString { Value = "healthBlue" }, storeValue = blueCounter };
        var readJoni = new GetPlayerDataInt { intName = new FsmString { Value = "joniHealthBlue" }, storeValue = joniCounter };
        var increment = new IncrementPlayerDataInt { intName = new FsmString { Value = "healthBlue" } };
        var retained = new FsmStateAction();
        var set = new FsmState { Name = "Set Blue", Actions = new FsmStateAction[] { reset, readBlue, readJoni, retained } };
        var add = new FsmState { Name = "Add Joni Health", Actions = new FsmStateAction[] { retained, increment } };
        fsm.FsmStates = new[] { set, add }; var originals = set.Actions;
        var patch = new BlueHealthHud(); patch.Prepare(root); patch.Prepare(root);
        PlayerData.instance.equippedCharm_27 = true; PlayerData.instance.maxHealth = 1; PlayerData.instance.healthBlue = 2;
        foreach (var action in set.Actions) action.OnEnter();
        Check(blueCounter.Value == 0 && joniCounter.Value == 3 && PlayerData.instance.healthBlue == 2,
            "Blue-row rebuild used Joni capacity or modified wounded HP");
        Check(!set.Actions.Contains(reset) && !add.Actions.Contains(increment) && add.Actions.Contains(retained),
            "Blue-row patch retained health mutation or removed its native drawing actions");
        PlayerData.instance.equippedCharm_27 = false; PlayerData.instance.maxHealth = 7; PlayerData.instance.healthBlue = 4;
        foreach (var action in set.Actions) action.OnEnter();
        Check(blueCounter.Value == 4 && joniCounter.Value == 0, "Non-Joni blue masks were counted incorrectly");
        patch.Restore(); patch.Restore();
        Check(ReferenceEquals(set.Actions, originals) && add.Actions.Contains(increment), "Blue-row patch leaked into ordinary play");
    }

    private static void VerifyStage()
    {
        var boss = new GameObject("Dream Mage Lord"); var phase2 = new GameObject("Dream Mage Lord Phase2"); var extras = new GameObject("_Enemies");
        var manager = new GameObject("_SceneManager"); var music = manager.AddComponent<PlayMakerFSM>();
        music.FsmStates = new[] { new FsmState { Name = "Boss Statue" }, new FsmState { Name = "Wait for Event" } };
        var scenery = new GameObject("Soul_Master_scene"); var floor = new GameObject("Floor"); var roof = new GameObject("World Edge v2");
        var roots = new[] { boss, phase2, extras, manager, scenery, floor, roof };
        using (var stage = new SoulTyrantArena())
        {
            stage.Claim(roots);
            Check(!boss.activeSelf && !phase2.activeSelf && !extras.activeSelf, "Soul Tyrant phases can activate during trial");
            Check(manager.activeSelf && music.enabled && scenery.activeSelf && floor.activeSelf && roof.activeSelf, "Takeover removed native environment/music/floor/roof");
            stage.StartMusic(); Check(music.Events.SequenceEqual(new[] { "GG MUSIC" }), "Native music cue was not triggered without boss intro");
        }
        Check(boss.activeSelf && phase2.activeSelf && extras.activeSelf && music.enabled, "Unload did not restore original encounter states");
        var untouched = new GameObject("Dream Mage Lord"); bool rejected = false;
        using (var stage = new SoulTyrantArena())
            try { stage.Claim(new[] { untouched }); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected && untouched.activeSelf, "Unsupported arena was partially disabled");
    }
    private static void VerifyCeiling()
    {
        var roof = new GameObject("Native roof").AddComponent<BoxCollider2D>();
        var log = new System.Collections.Generic.List<string>();
        int calls = 0;
        Physics2D.Rays = (origin, direction, distance, mask) =>
        {
            calls++;
            Check(direction.y == 1 && mask == 256 && origin.x >= 3.5f && origin.x <= 36.5f,
                "Ceiling scan missed interior terrain");
            var barrier = GameObject.All.LastOrDefault(obj => obj.name == "Gauntlet.Ceiling" && !obj.Destroyed);
            var collider = barrier?.GetComponent<BoxCollider2D>() ?? roof;
            float y = barrier == null ? 44.1f : barrier.transform.position.y - collider.size.y / 2;
            return new[] { new RaycastHit2D { collider = collider, point = new Vector2(origin.x, y), normal = new Vector2(0, -1) } };
        };
        var ceiling = new ArenaCeiling(); ceiling.Ensure(3, 37, 28, log.Add, 40.5f);
        var added = GameObject.All.Last(obj => obj.name == "Gauntlet.Ceiling" && !obj.Destroyed);
        var solid = added.GetComponent<BoxCollider2D>();
        Check(calls >= 69 && added.layer == 8 && !solid.isTrigger && solid.size.x == 38
            && Math.Abs(added.transform.position.y - solid.size.y / 2 - (40.5f - ArenaCeiling.ScreenInset)) < 0.001f, "Lowered roof is not solid across the camera-visible arena");
        ceiling.Ensure(3, 37, 28, log.Add, 41.5f);
        Check(Math.Abs(ceiling.Top - (41.5f - ArenaCeiling.ScreenInset)) < 0.001f, "Ceiling did not follow recalibrated camera top");
        Check(GameObject.All.Count(obj => obj.name == "Gauntlet.Ceiling" && !obj.Destroyed) == 1, "Retry duplicated ceiling");
        ceiling.Dispose(); ceiling.Dispose();
        Check(added.Destroyed && !roof.gameObject.Destroyed, "Ceiling cleanup damaged native geometry");
        Physics2D.Rays = (origin, direction, distance, mask) => Array.Empty<RaycastHit2D>();
        bool rejected = false;
        using (var invalid = new ArenaCeiling())
            try { invalid.Ensure(3, 37, 28, log.Add, 40.5f); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "Missing lowered roof was accepted");
    }

    private static void VerifyLayouts()
    {
        GameObject Platform(string name, float width)
        {
            var obj = new GameObject(name); obj.transform.lossyScale = new Vector3(1, 0.95f, 1);
            var box = obj.AddComponent<BoxCollider2D>(); box.size = new Vector2(width, 1.058f); box.offset = new Vector2(0.01f, 0.144f);
            return obj;
        }
        var small = Platform("gg_plat_float_small", 2.388f); var wide = Platform("gg_plat_float_wider", 5.376f);
        var prefabs = ArenaLayouts.Resolve(new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, GameObject>>
            { ["GG_Uumuu"] = new System.Collections.Generic.Dictionary<string, GameObject> { [small.name] = small, [wide.name] = wide },
              ["Room_Colosseum_Gold"] = new System.Collections.Generic.Dictionary<string, GameObject> { ["Colosseum Manager/Ground Spikes/Colosseum Spike"] = Platform("Spike", 1.76f) },
              ["Room_Town_Stag_Station"] = new System.Collections.Generic.Dictionary<string, GameObject> { ["Station Bell/Bell"] = new GameObject("Bell") } });
        Check(prefabs != null && ArenaLayouts.Preloads().Count == 3, "Native platform preload binding failed");
        var log = new System.Collections.Generic.List<string>();
        using (var layouts = new ArenaLayouts(prefabs))
        {
            layouts.Apply(6, 69, 103, 5, 72, log.Add);
            var placed = GameObject.All.Where(go => go.name.StartsWith("Gauntlet.Platform.") && go.activeInHierarchy).ToArray();
            Check(placed.Length == 1 && placed[0].GetComponent<BoxCollider2D>() != null, "Center layout did not place native platform collision");
            var box = placed[0].GetComponent<BoxCollider2D>();
            Check(Math.Abs(placed[0].transform.position.y + box.offset.y * 0.95f + box.size.y * 0.95f / 2 - 8.8f) < 0.001f, "Platform top does not match planned jump height");
            layouts.Apply(7, 69, 103, 5, 72, log.Add);
            Check(placed[0].Destroyed && GameObject.All.Count(go => go.name.StartsWith("Gauntlet.Platform.") && go.activeInHierarchy) == 2, "Old layout collision overlapped next wave");
            layouts.Apply(6, 69, 103, 5, 86, log.Add);
            Check(!GameObject.All.Any(go => go.name.StartsWith("Gauntlet.Platform.") && go.activeInHierarchy), "Platform materialized across the Knight");
            layouts.Apply(9, 69, 103, 5, 72, log.Add);
            layouts.Apply(26, 69, 103, 5, 72, log.Add);
            Check(!GameObject.All.Any(go => go.name.StartsWith("Gauntlet.Platform.") && go.activeInHierarchy), "Boss fight did not clear added platforms");
            layouts.Apply(27, 69, 103, 5, 81, log.Add);
            var spikes = GameObject.All.Where(go => go.name.StartsWith("Gauntlet.Spike.") && go.activeInHierarchy).ToArray();
            Check(spikes.Length == 0, "Floor spikes must remain disabled");
            for (int wave = 27; wave <= 46; wave++)
            {
                layouts.Apply(wave, 69, 103, 5, 72, log.Add, 86);
                Check(GameObject.All.Where(go => go.name.StartsWith("Gauntlet.Spike.") && go.activeInHierarchy)
                    .All(go => Math.Abs(go.transform.position.x - 86) >= 4), "Spike blocks the fixed central hazard respawn");
                layouts.Apply(wave, 69, 103, 5, 72, log.Add, 91);
                Check(GameObject.All.Where(go => go.name.StartsWith("Gauntlet.Spike.") && go.activeInHierarchy)
                    .All(go => Math.Abs(go.transform.position.x - 91) >= 4 && Math.Abs(go.transform.position.x - 86) >= 4), "Spike ignores a respawn displaced from center");
            }
            layouts.Apply(47, 69, 103, 5, 72, log.Add);
            Check(spikes.All(go => go.Destroyed), "Boss finale left spike hazards alive");
            layouts.Apply(8, 69, 103, 5, 72, log.Add);
        }
        Check(!GameObject.All.Any(go => go.name.StartsWith("Gauntlet.Platform.") && go.activeInHierarchy) && !small.Destroyed && !wide.Destroyed, "Layout cleanup left collision or destroyed templates");
    }

}
