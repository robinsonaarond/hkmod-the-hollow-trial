using System;
using System.Reflection;
using UnityEngine;

namespace Gauntlet.Mod
{
    public sealed class ArrivalMarker : MonoBehaviour
    {
        private static readonly FieldInfo PuffField = typeof(EnemyDeathEffects).GetField("deathPuffMedPrefab", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static GameObject Resolve(GameObject enemy)
        {
            var death = enemy != null ? enemy.GetComponent<EnemyDeathEffects>() : null;
            var prefab = death != null ? PuffField?.GetValue(death) as GameObject : null;
            if (prefab == null || prefab.GetComponentInChildren<ParticleSystem>(true) == null)
                throw new InvalidOperationException("The native orange arrival puff is unavailable.");
            return prefab;
        }
        internal void Configure(Vector3 point, GameObject prefab)
        {
            transform.position = new Vector3(point.x, point.y, point.z - 0.25f);
            // Keep the copy inactive until its native pooling script is disabled.
            // This is an owned visual effect, not a pooled combat/death object.
            gameObject.SetActive(false);
            var cloud = Instantiate(prefab, transform, false);
            cloud.name = "Orange arrival cloud";
            cloud.transform.localPosition = Vector3.zero;
            cloud.transform.localScale = Vector3.one * 0.6f;
            foreach (var script in cloud.GetComponentsInChildren<MonoBehaviour>(true))
            { script.enabled = false; Destroy(script); }
            var particles = cloud.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var particle in particles)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = particle.main;
                main.loop = false;
                main.playOnAwake = false;
                main.stopAction = ParticleSystemStopAction.None;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
            cloud.SetActive(true);
            gameObject.SetActive(true);
            foreach (var particle in particles) particle.Play(false);
        }
        // Leave the native cloud alive as the enemy emerges after 200 ms.
        // Unsafe/cancelled arrivals destroy it immediately through ClearWarning.
        internal void Complete() => Destroy(gameObject, 0.7f);
    }
}
