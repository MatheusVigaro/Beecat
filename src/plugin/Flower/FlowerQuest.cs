using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;
using System.Xml;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using SlugBase.DataTypes;

namespace BeeWorld
{
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
}