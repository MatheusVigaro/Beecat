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
using Fisobs.Properties;

namespace BeeWorld
{
    public class AbstractFlower : AbstractPhysicalObject
    {
        public AbstractFlower(World world, WorldCoordinate pos, EntityID ID) : base(world, BeeEnums.BeeFlower, null, pos, ID)
        {
        }

        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new Flower(this);
            }

        }
    }
}