using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gauntlet.Mod
{
    internal sealed class CardIconCache : IDisposable
    {
        private readonly Dictionary<Sprite, Texture2D> cache = new Dictionary<Sprite, Texture2D>();

        internal Texture2D Get(Sprite sprite)
        {
            if (sprite == null) return null;
            if (cache.TryGetValue(sprite, out var icon)) return icon;
            try { icon = Render(sprite); }
            catch (Exception ex) { Debug.LogWarning("[Gauntlet] Card artwork unavailable: " + sprite.name + ": " + ex.Message); }
            // Cache failures too, so the fallback emblem doesn't retry every GUI event.
            cache.Add(sprite, icon);
            return icon;
        }

        private static Texture2D Render(Sprite sprite)
        {
            // Unity 2020 returns Rect.zero for textureRect on tightly packed
            // sprites. Let its SpriteRenderer resolve the atlas, mesh, rotation
            // and alpha instead of treating a sprite as a rectangular crop.
            const int size = 128;
            var root = new GameObject("Gauntlet.IconPreview");
            var cameraObject = new GameObject("Gauntlet.IconCamera");
            var target = RenderTexture.GetTemporary(size, size, 16, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            Texture2D result = null;
            Material material = null;
            try
            {
                root.layer = 31;
                root.transform.position = new Vector3(-10000, -10000, 0);
                var renderer = root.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                material = new Material(Shader.Find("Sprites/Default"));
                renderer.sharedMaterial = material;
                var camera = cameraObject.AddComponent<Camera>();
                camera.enabled = false;
                camera.orthographic = true;
                camera.orthographicSize = Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.y) * 0.525f;
                camera.aspect = 1;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = 2;
                camera.cullingMask = 1 << 31;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                var center = renderer.bounds.center;
                camera.transform.position = new Vector3(center.x, center.y, center.z - 1);
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                result = new Texture2D(size, size, TextureFormat.RGBA32, false);
                result.name = "Gauntlet.Icon." + sprite.name;
                result.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                // Sprites/Default renders premultiplied RGB. IMGUI expects
                // straight alpha; undo that multiplication to keep soft edges.
                var pixels = result.GetPixels32();
                bool visible = false;
                for (int i = 0; i < pixels.Length; i++)
                {
                    var p = pixels[i];
                    if (p.a == 0) { pixels[i] = new Color32(0, 0, 0, 0); continue; }
                    if (p.r > 8 || p.g > 8 || p.b > 8) visible = true;
                    pixels[i] = new Color32((byte)Math.Min(255, p.r * 255 / p.a),
                        (byte)Math.Min(255, p.g * 255 / p.a), (byte)Math.Min(255, p.b * 255 / p.a), p.a);
                }
                if (!visible) throw new InvalidOperationException("Native sprite rendered blank.");
                result.SetPixels32(pixels);
                result.Apply(false, true);
                result.filterMode = FilterMode.Bilinear;
                result.wrapMode = TextureWrapMode.Clamp;
                return result;
            }
            catch
            {
                if (result != null) UnityEngine.Object.Destroy(result);
                throw;
            }
            finally
            {
                RenderTexture.active = previous;
                root.SetActive(false);
                cameraObject.SetActive(false);
                UnityEngine.Object.Destroy(root);
                UnityEngine.Object.Destroy(cameraObject);
                if (material != null) UnityEngine.Object.Destroy(material);
                RenderTexture.ReleaseTemporary(target);
            }
        }

        public void Dispose()
        {
            foreach (var icon in cache.Values) if (icon != null) UnityEngine.Object.Destroy(icon);
            cache.Clear();
        }
    }
}
