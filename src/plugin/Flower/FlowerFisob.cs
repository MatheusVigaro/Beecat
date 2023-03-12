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
using Fisobs.Items;
using Fisobs.Core;
using Fisobs.Sandbox;
using Fisobs.Properties;
using System.Runtime.CompilerServices;

namespace BeeWorld
{
    public class FlowerFisob : Fisob
    {
        public FlowerFisob() : base(BeeEnums.BeeFlower)
        {
            Icon = new FlowerIcon();
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
        {
            return new AbstractFlower(world, entitySaveData.Pos, entitySaveData.ID);
        }


        private static readonly FlowerProperties properties = new();

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
            // The Mosquitoes example demonstrates this.
            return properties;
        }
    }
}