using RWCustom;
using UnityEngine;

namespace BeeWorld;

public static class Utils
{
    public static void MapTextureColor(Texture2D texture, int alpha, Color32 to, bool apply = true)
    {
        var colors = texture.GetPixels32();

        for (var i = 0; i < colors.Length; i++)
        {
            if (colors[i].a == alpha)
            {
                colors[i] = to;
            }
        }
        
        texture.SetPixels32(colors);

        if (apply)
        {
            texture.Apply(false);
        }
    }

    public static BodyChunk ClosestAttackableChunk(Room room, Vector2 pos, float range)
    {
        BodyChunk closestChunk = null;
        var closestDist = float.MaxValue;

        foreach (var obj in room.updateList)
        {
            if (obj is Creature creature and not Player and not Fly && !creature.dead)
            {
                foreach (var chunk in creature.bodyChunks)
                {
                    var dist = Custom.Dist(chunk.pos, pos); 
                    if (dist < closestDist && dist < range)
                    {
                        closestChunk = chunk;
                        closestDist = dist;
                    }
                }
            }
        }

        return closestChunk;
    }
}