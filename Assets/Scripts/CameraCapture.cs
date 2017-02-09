using UnityEngine;

public class CameraCapture : MonoBehaviour
{
    [HideInInspector] public Texture2D RenderResult;
    private RenderTexture renderTexture;

    private void Start()
    {
        RenderResult = new Texture2D(Screen.width, Screen.height);
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        GetComponent<Camera>().targetTexture = renderTexture;
    }

    void OnPostRender()
    {
        Camera owner = GetComponent<Camera>();
        RenderTexture target = owner.targetTexture;

        if (target == null)
            return;

        RenderResult = new Texture2D(target.width, target.height, TextureFormat.ARGB32, true);
        Rect rect = new Rect(0, 0, target.width, target.height);
        RenderResult.ReadPixels(rect, 0, 0, true);
    }
}