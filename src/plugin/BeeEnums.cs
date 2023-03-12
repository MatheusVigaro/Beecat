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

namespace BeeWorld
{
    public static class BeeEnums
    {
        public static void RegisterValues()
        {
            BeeBuzz = new SoundID("beebuzz", true);
            BeeFlower = new AbstractPhysicalObject.AbstractObjectType("BeeFlower", true);
        }

        public static void UnregisterValues()
        {
            Unregister(BeeBuzz);
        }

        private static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
        {
            if (extEnum != null)
            {
                extEnum.Unregister();
            }
        }

        public static SoundID BeeBuzz;

        public static AbstractPhysicalObject.AbstractObjectType BeeFlower;
    }
}