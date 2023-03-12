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
using Fisobs.Core;

namespace BeeWorld
{
    public class FlowerIcon : Icon
    {
        public override int Data(AbstractPhysicalObject apo)
        {
            return 0;
        }

        public override Color SpriteColor(int data)
        {
            return Color.red;
        }

        public override string SpriteName(int data)
        {
            return "SkyDandelion";
        }
    }
}