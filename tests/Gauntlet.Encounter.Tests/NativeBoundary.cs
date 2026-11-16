// Minimal Unity/FSM boundaries for the actual encounter adapters. These expose
// mutated data and calls, without substituting an alternative AI implementation.
using System;
using System.Collections.Generic;
using System.Linq;
namespace UnityEngine
{
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 up => new Vector2(0, 1);
        public static Vector2 zero => new Vector2(0, 0);
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z = 0) { this.x = x; this.y = y; this.z = z; }
    }
    public static class Mathf
    {
        public static int CeilToInt(float x) => (int)Math.Ceiling(x);
        public static float Abs(float x) => Math.Abs(x);
        public static float Max(float a, float b) => Math.Max(a, b);
        public static float Min(float a, float b) => Math.Min(a, b);
    }
    public static class Time { public static float time; public static float timeScale = 1; }
    public class Component
    {
        public GameObject gameObject;
        public Transform transform => gameObject.transform;
        public string name => gameObject.name;
        public bool enabled = true;
        public T GetComponent<T>() where T : Component => gameObject.GetComponent<T>();
        public T[] GetComponents<T>() where T : Component => gameObject.GetComponents<T>();
        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component => gameObject.GetComponentsInChildren<T>(includeInactive);
    }
    public class MonoBehaviour : Component { }
    public class Transform : Component
    {
        public Vector3 position;
        public Vector3 lossyScale = new Vector3(1, 1, 1);
        public List<GameObject> Children = new List<GameObject>();
        public Transform Find(string name) => Children.FirstOrDefault(child => child.name == name)?.transform;
    }
    public class GameObject
    {
        private readonly List<Component> components = new List<Component>();
        public static readonly List<GameObject> All = new List<GameObject>();
        public string name;
        public bool activeSelf = true, Destroyed;
        public int layer;
        public Transform transform;
        public bool activeInHierarchy => activeSelf && !Destroyed;
        public static GameObject Find(string name) => All.FirstOrDefault(go => go.name == name && go.activeInHierarchy);
        public void SetActive(bool value) { activeSelf = value; }
        public GameObject(string name = "Test") { this.name = name; transform = AddComponent<Transform>(); All.Add(this); }
        public T AddComponent<T>() where T : Component, new()
        { var value = new T { gameObject = this }; components.Add(value); return value; }
        public T GetComponent<T>() where T : Component => components.OfType<T>().FirstOrDefault();
        public T[] GetComponents<T>() where T : Component => components.OfType<T>().ToArray();
        public T[] GetComponentsInChildren<T>(bool includeInactive) where T : Component => components.OfType<T>()
            .Concat(transform.Children.SelectMany(child => child.GetComponentsInChildren<T>(includeInactive))).ToArray();
    }
    public static class Object
    {
        public static GameObject Instantiate(GameObject prefab)
        {
            var obj = new GameObject(prefab.name); obj.transform.lossyScale = prefab.transform.lossyScale;
            var source = prefab.GetComponent<BoxCollider2D>();
            if (source != null) { var box = obj.AddComponent<BoxCollider2D>(); box.size = source.size; box.offset = source.offset; }
            return obj;
        }
        public static void Destroy(GameObject obj) { obj.Destroyed = true; obj.SetActive(false); }
    }
    public struct RaycastHit2D { public Collider2D collider; public Vector2 point, normal; }
    public static class Physics2D
    {
        public static Func<Vector2, Vector2, float, int, RaycastHit2D[]> Rays;
        public static int SyncCount;
        public static RaycastHit2D[] RaycastAll(Vector2 origin, Vector2 direction, float distance, int mask) => Rays(origin, direction, distance, mask);
        public static void SyncTransforms() { SyncCount++; }
    }
    public class Renderer : Component { }
    public class Collider2D : Component { public bool isTrigger; public Vector2 offset; }
    public class CircleCollider2D : Collider2D { public float radius; }
    public class BoxCollider2D : Collider2D { public Vector2 size; }
}
namespace HutongGames.PlayMaker
{
    public class FsmStateAction { public virtual void OnEnter() { } protected void Finish() { } }
    public class FunctionCall { public string FunctionName; }
    public class FsmFloat { public float Value; }
    public class FsmInt { public int Value; }
    public class FsmBool { public bool Value; }
    public class FsmString { public string Value; }
    public class FsmGameObject { public UnityEngine.GameObject Value; }
    public class FsmState { public string Name; public FsmStateAction[] Actions = Array.Empty<FsmStateAction>(); }
    public class FsmVariables
    {
        public readonly Dictionary<string, FsmFloat> Floats = new Dictionary<string, FsmFloat>();
        public readonly Dictionary<string, FsmInt> Ints = new Dictionary<string, FsmInt>();
        public readonly Dictionary<string, FsmBool> Bools = new Dictionary<string, FsmBool>();
        public readonly Dictionary<string, FsmGameObject> Objects = new Dictionary<string, FsmGameObject>();
        public FsmFloat FindFsmFloat(string name) => Floats.GetValueOrDefault(name);
        public FsmInt FindFsmInt(string name) => Ints.GetValueOrDefault(name);
        public FsmBool FindFsmBool(string name) => Bools.GetValueOrDefault(name);
        public FsmGameObject FindFsmGameObject(string name) => Objects.GetValueOrDefault(name);
    }
}
namespace HutongGames.PlayMaker.Actions
{
    public class GetPlayerDataInt : HutongGames.PlayMaker.FsmStateAction
    { public HutongGames.PlayMaker.FsmString intName; public HutongGames.PlayMaker.FsmInt storeValue; }
    public class IncrementPlayerDataInt : HutongGames.PlayMaker.FsmStateAction
    { public HutongGames.PlayMaker.FsmString intName; }
    public class SendMessage : HutongGames.PlayMaker.FsmStateAction
    { public HutongGames.PlayMaker.FunctionCall functionCall; }
    public class Wait : HutongGames.PlayMaker.FsmStateAction { public bool realTime; }
    public class iTweenFsmAction : HutongGames.PlayMaker.FsmStateAction { public HutongGames.PlayMaker.FsmBool realTime; }
    public class FloatCompare : HutongGames.PlayMaker.FsmStateAction { public HutongGames.PlayMaker.FsmFloat float2 = new HutongGames.PlayMaker.FsmFloat(); }
    public class PlayerDataBoolTest : HutongGames.PlayMaker.FsmStateAction { public object isTrue, isFalse; public HutongGames.PlayMaker.FsmString boolName = new HutongGames.PlayMaker.FsmString(); }
    public class SetFsmBool : HutongGames.PlayMaker.FsmStateAction { public HutongGames.PlayMaker.FsmString fsmName = new HutongGames.PlayMaker.FsmString(); }
    public class SetFsmString : HutongGames.PlayMaker.FsmStateAction
    {
        public HutongGames.PlayMaker.FsmString variableName, setValue;
    }
    public class SetPosition : HutongGames.PlayMaker.FsmStateAction { }
    public class SetPlayerDataBool : HutongGames.PlayMaker.FsmStateAction { }
    public class EndGGBossScene : HutongGames.PlayMaker.FsmStateAction { }
    public class CallMethodProper : HutongGames.PlayMaker.FsmStateAction { }
    public class FindGameObject : HutongGames.PlayMaker.FsmStateAction { public HutongGames.PlayMaker.FsmString objectName = new HutongGames.PlayMaker.FsmString(); }
    public class DestroyObject : HutongGames.PlayMaker.FsmStateAction { }
    public class SendEventByName : HutongGames.PlayMaker.FsmStateAction { public HutongGames.PlayMaker.FsmString sendEvent = new HutongGames.PlayMaker.FsmString(); }
}
public class PlayMakerFSM : UnityEngine.MonoBehaviour
{
    public string FsmName;
    public string ActiveStateName = "Walk";
    public HutongGames.PlayMaker.FsmVariables FsmVariables = new HutongGames.PlayMaker.FsmVariables();
    public HutongGames.PlayMaker.FsmState[] FsmStates = Array.Empty<HutongGames.PlayMaker.FsmState>();
    public readonly List<string> Events = new List<string>();
    public readonly List<string> StateChanges = new List<string>();
    public Action<string> OnState;
    public void SendEvent(string name) { Events.Add(name); }
    public void SetState(string name) { StateChanges.Add(name); ActiveStateName = name; OnState?.Invoke(name); }
}
public class GameCameras
{
    public static GameCameras instance;
    public UnityEngine.GameObject hudCanvas;
}
public class Walker : UnityEngine.MonoBehaviour
{
    public bool startInactive = true;
    public int LastGo;
    public void Go(int direction) { LastGo = direction; }
}
public class HealthManager : UnityEngine.MonoBehaviour { public int hp = 325; }
public class EnemyDeathEffects : UnityEngine.MonoBehaviour
{
    private UnityEngine.GameObject corpsePrefab = new UnityEngine.GameObject("Corpse");
    private UnityEngine.GameObject corpse = new UnityEngine.GameObject("Instantiated corpse");
    private string deathBroadcastEvent = "BOSS DEATH";
    public bool HasCorpse => corpsePrefab != null || corpse != null;
    public EnemyDeathEffects()
    {
        var fsm = corpsePrefab.AddComponent<PlayMakerFSM>(); fsm.FsmName = "corpse";
        fsm.FsmStates = new[] { new HutongGames.PlayMaker.FsmState { Name = "Init", Actions = new HutongGames.PlayMaker.FsmStateAction[] { new HutongGames.PlayMaker.Actions.SetFsmBool(), new HutongGames.PlayMaker.FsmStateAction() } } }
            .Concat(new[] { "Steam", "Ready", "Blow", "Destroy Self" }.Select(name => new HutongGames.PlayMaker.FsmState { Name = name })).ToArray();
    }
    public int PrefabInitActions => corpsePrefab.GetComponent<PlayMakerFSM>().FsmStates[0].Actions.Length;
    public void PreInstantiate()
    {
        var source = corpsePrefab.GetComponent<PlayMakerFSM>(); var fsm = corpse.AddComponent<PlayMakerFSM>(); fsm.FsmName = "corpse";
        fsm.FsmStates = source.FsmStates.Select(state => new HutongGames.PlayMaker.FsmState { Name = state.Name, Actions = state.Actions.ToArray() }).ToArray();
        corpse.SetActive(false);
    }
    public string Broadcast => deathBroadcastEvent;
}
public class HeroController : UnityEngine.MonoBehaviour { public static HeroController instance = new UnityEngine.GameObject("Knight").AddComponent<HeroController>(); }
namespace Gauntlet.Mod { public class ArenaController { public Gauntlet.Core.Run Run = new Gauntlet.Core.Run(); public void CallNextWave() { Run.CallNextWave(); } } }

public class PlayerData
{
    public static PlayerData instance = new PlayerData();
    public int healthBlue, maxHealth;
    public bool equippedCharm_27;
}
