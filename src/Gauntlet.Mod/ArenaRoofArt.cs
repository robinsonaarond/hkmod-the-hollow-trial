using System;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class ArenaRoofArt : IDisposable
    {
        private GameObject root;
        internal void Build(GameObject platform, float left, float right, float underside)
        {
            Dispose();
            root = new GameObject("Gauntlet.CeilingArt");
            var box = platform.GetComponent<BoxCollider2D>();
            var scale = platform.transform.lossyScale;
            float width = box.size.x * Mathf.Abs(scale.x);
            float step = width - 0.12f; // Overlap the art at joins.
            int count = Mathf.CeilToInt((right - left + 4f) / step);
            for (int i = 0; i < count; i++)
            {
                var tile = UnityEngine.Object.Instantiate(platform);
                tile.SetActive(false); tile.name = "Ceiling stone " + (i + 1);
                tile.transform.SetParent(root.transform, true);
                // Turn the native Godhome stone ledge upside down so its solid
                // edge faces the arena. The separate verified box owns collision.
                tile.transform.rotation = Quaternion.Euler(0, 0, 180);
                float x = left - 2f + width / 2 + i * step;
                tile.transform.position = new Vector3(x + box.offset.x * scale.x,
                    underside + (box.offset.y + box.size.y / 2) * Mathf.Abs(scale.y), -0.2f);
                foreach (var collider in tile.GetComponentsInChildren<Collider2D>(true)) collider.enabled = false;
                foreach (var script in tile.GetComponentsInChildren<MonoBehaviour>(true))
                    if (!(script is tk2dBaseSprite) && !(script is tk2dSpriteAnimator)) script.enabled = false;
                tile.SetActive(true);
            }
        }
        public void Dispose()
        {
            if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); }
            root = null;
        }
    }
}
