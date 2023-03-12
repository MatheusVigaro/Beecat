using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using SlugBase;
using SlugBase.DataTypes;
using SlugBase.Features;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace BeeWorld.Hooks
{
    public class WorldHooks
    {
        public static void Init()
        {
            On.World.SpawnGhost += World_SpawnGhost;
            On.GhostHunch.Update += GhostHunch_Update;
        }

        private static void GhostHunch_Update(On.GhostHunch.orig_Update orig, GhostHunch self, bool eu)
        {
            if (self.room?.game?.session is StoryGameSession storySession && storySession.saveStateNumber.value == "bee")
            {
                self.Destroy();
            }

            orig(self, eu);
        }

        private static void World_SpawnGhost(On.World.orig_SpawnGhost orig, World self)
        {
            if (self.game.session is StoryGameSession storySession && storySession.saveStateNumber.value == "bee")
            {
                return;
            }

            orig(self);
        }
    }
}