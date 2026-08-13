using System;
using UnityEngine;

public class MoveMarkerVisual : MonoBehaviour {
    [SerializeField] private MeshRenderer markerRenderer;

    [SerializeField] [Min(0f)] [Tooltip("0 - not automatically hide")]
    private float lifetime = 0f;

    [SerializeField] [Min(0f)] private float autoFadeDuration = 0.5f;

    private Color baseColor = Color.darkGreen;

    private float timeLeft = -1f;
    private float fadeDuration = -1f;

    private MaterialPropertyBlock materialPropertyBlock;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private void Reset() {
        markerRenderer = GetComponentInChildren<MeshRenderer>(true);
    }

    private void Update() {
        if (timeLeft <= 0f) {
            return;
        }

        timeLeft -= Time.deltaTime;

        float alpha = 1f;

        if (fadeDuration > 0f) {
            alpha = Mathf.Clamp01(timeLeft / fadeDuration);
        } else if (lifetime > 0f && timeLeft < autoFadeDuration) {
            alpha = Mathf.Clamp01(timeLeft / autoFadeDuration);
        }

        ApplyColor(alpha);

        if (timeLeft <= 0f) {
            Destroy(gameObject);
        }
    }

    public void Setup(Vector3 position, Quaternion rotation, Color color) {
        transform.SetPositionAndRotation(position, rotation);

        baseColor = color;
        timeLeft = lifetime > 0f ? lifetime : -1f;
        fadeDuration = -1f;

        gameObject.SetActive(true);
        ApplyColor(1f);
    }

    public void FadeOutAndDestroy(float duration = 0.25f) {
        fadeDuration = Mathf.Max(0.01f, duration);
        timeLeft = fadeDuration;
    }

    private void ApplyColor(float alphaMultiplier) {
        if (markerRenderer == null) {
            return;
        }

        if (materialPropertyBlock == null) {
            materialPropertyBlock = new MaterialPropertyBlock();
        }
        
        markerRenderer.GetPropertyBlock(materialPropertyBlock);

        Color color = baseColor;
        color.a *= alphaMultiplier;
        
        materialPropertyBlock.SetColor(ColorId, color);
        materialPropertyBlock.SetColor(BaseColorId, color);
        
        markerRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}