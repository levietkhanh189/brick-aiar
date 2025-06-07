using UnityEngine;
using System;

public static class TextureToBase64
{
    public static string ConvertToBase64(Texture2D texture)
    {
        Texture2D readableTexture = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture rt = RenderTexture.GetTemporary(texture.width, texture.height);

        Graphics.Blit(texture, rt);
        RenderTexture.active = rt;
        readableTexture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        byte[] imageData = readableTexture.EncodeToPNG();
        return Convert.ToBase64String(imageData);
    }

}
