using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ItemChanger;
using UnityEngine;

namespace DistantGreensCharms.Helper;

public static class SpriteManager
{
    private static Dictionary<string, Sprite> Sprites = new();

    public static Sprite Get(string spriteName, float? ppu = null)
    {
        if (Sprites.TryGetValue(spriteName, out var sprite)) return sprite;
        string resourceName = $"DistantGreensCharms.Resources.{spriteName}.png";
        LoadTexture(resourceName, out Texture2D texture);
        sprite = (ppu is float) ? 
            Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), (float) ppu) :
            Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));
        Sprites.Add(spriteName, sprite);
        return sprite;
    }

    private static void LoadTexture(string name, out Texture2D texture)
    {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
        {
            if (stream == null)
            {
                throw new FileNotFoundException($"Embedded resource not found: {name}");
            }

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            ImageConversion.LoadImage(texture, buffer, true);
            texture.filterMode = FilterMode.Bilinear;
        }
    }

    public static ISprite CastToISprite(Sprite sprite)
    {
        return new ItemChangerSprite(sprite);
    }
}

internal class ItemChangerSprite : ISprite
{
    public ItemChangerSprite(Sprite sprite)
    {
        Value = sprite;
    }
    
    public ISprite Clone() => (ISprite)MemberwiseClone();
    public Sprite Value { get; }
}

