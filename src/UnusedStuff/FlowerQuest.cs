using UnityEngine;

namespace BeeWorld;

public class FlowerQuest
{
    public static FlowerDefinition GetFlower(string region)
    {
        switch (region)
        {
            case "SI":
                return new FlowerDefinition
                {
                    Region = region,
                    Room = "SI_BEEFLOWER",
                    Position = new Vector2(614, 193),
                    Color = Color.red,
                    Sprite = "SkyDandelion"
                };
            case "SB":
            case "OE":
            default:
                return null;
        }
    }

    public class FlowerDefinition
    {
        public string Region;
        public string Room;
        public Vector2 Position;
        public Color Color;
        public string Sprite;
    }
}