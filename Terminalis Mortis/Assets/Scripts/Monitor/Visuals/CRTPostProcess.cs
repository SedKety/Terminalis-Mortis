using UnityEngine;

public class CRTPostProcess : MonoBehaviour
{
    public Material crtMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, crtMaterial);
    }
}