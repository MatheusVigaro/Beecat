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
using BeeWorld.Hooks;

namespace BeeWorld
{
    public class FlowerProperties : ItemProperties
    {
        public override void Throwable(Player player, ref bool throwable)
        {
            throwable = false;
        }

        public override void ScavCollectScore(Scavenger scav, ref int score)
        {
            score = 0;
        }

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            if (PlayerHooks.PlayerData.TryGetValue(player, out var playerData) && playerData.IsBee)
            {
                grabability = Player.ObjectGrabability.OneHand;
            } 
            else
            {
                grabability = Player.ObjectGrabability.CantGrab;
            }   
        }
    }
}