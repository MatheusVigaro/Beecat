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
    public class PlayerHooks
    {
        public static ConditionalWeakTable<Player, PlayerEx> PlayerData = new();

        public static void Init()
        {
            On.Player.ctor += Player_ctor;
            On.Player.UpdateMSC += Player_UpdateMSC;
            On.Player.SlugSlamConditions += Player_SlugSlamConditions;
            On.Player.CanBeSwallowed += Player_CanBeSwallowed;
            On.SlugcatStats.SlugcatCanMaul += SlugcatStats_SlugcatCanMaul;
            On.Player.Grabability += Player_Grabability;

            IL.Player.ThrowObject += Player_ThrowObject;
        }

        private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
        {
            if (PlayerData.TryGetValue(self, out var player) && player.preventGrabs > 0)
            {
                return Player.ObjectGrabability.CantGrab;
            }

            return orig(self, obj);
        }

        #region ILHooks

        private static void Player_ThrowObject(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdsfld<Player.AnimationIndex>("Flip")))
            {
                return;
            }

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdloc(1)))
            {
                return;
            }

            cursor.MoveAfterLabels();

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<Player, bool>>(player =>
            {
                return PlayerData.TryGetValue(player, out var playerEx) && playerEx.IsBee && playerEx.isFlying;
            });
            cursor.Emit(OpCodes.Or);
        }

        #endregion

        #region Player

        private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
        {
            return orig(self, testObj) || (testObj is Flower && PlayerData.TryGetValue(self, out var player) && player.IsBee);
        }

        private static bool SlugcatStats_SlugcatCanMaul(On.SlugcatStats.orig_SlugcatCanMaul orig, SlugcatStats.Name slugcatNum)
        {
            return slugcatNum.value.ToLower() == "bee" || orig(slugcatNum);
        }


        private static bool Player_SlugSlamConditions(On.Player.orig_SlugSlamConditions orig, Player self, PhysicalObject otherObject)
        {
            if (!PlayerData.TryGetValue(self, out var player) || !player.IsBee)
            {
                return orig(self, otherObject);
            }

            if ((otherObject as Creature).abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
            {
                return false;
            }
            if (self.gourmandAttackNegateTime > 0)
            {
                return false;
            }
            if (self.gravity == 0f)
            {
                return false;
            }
            if (self.cantBeGrabbedCounter > 0)
            {
                return false;
            }
            if (self.forceSleepCounter > 0)
            {
                return false;
            }
            if (self.timeSinceInCorridorMode < 5)
            {
                return false;
            }
            if (self.submerged)
            {
                return false;
            }
            if (self.enteringShortCut != null || self.animation != Player.AnimationIndex.BellySlide && self.canJump >= 5)
            {
                return false;
            }
            if (self.animation == Player.AnimationIndex.CorridorTurn || self.animation == Player.AnimationIndex.CrawlTurn || self.animation == Player.AnimationIndex.ZeroGSwim || self.animation == Player.AnimationIndex.ZeroGPoleGrab || self.animation == Player.AnimationIndex.GetUpOnBeam || self.animation == Player.AnimationIndex.ClimbOnBeam || self.animation == Player.AnimationIndex.AntlerClimb || self.animation == Player.AnimationIndex.BeamTip)
            {
                return false;
            }
            Vector2 vel = self.bodyChunks[0].vel;
            if (self.bodyChunks[1].vel.magnitude < vel.magnitude)
            {
                vel = self.bodyChunks[1].vel;
            }
            if (self.animation != Player.AnimationIndex.BellySlide && vel.y >= -10f && vel.magnitude <= 25f)
            {
                return false;
            }
            Creature creature = otherObject as Creature;
            foreach (Creature.Grasp item in self.grabbedBy)
            {
                if (item.pacifying || item.grabber == creature)
                {
                    return false;
                }
            }
            return (!ModManager.CoopAvailable || !(otherObject is Player) || Custom.rainWorld.options.friendlyFire) && self.FoodInStomach >= 8;
        }

        private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
        {
            orig(self);

            if (!PlayerData.TryGetValue(self, out var player) || !player.IsBee)
            {
                return;
            }

            const float normalGravity = 0.9f;
            const float normalAirFriction = 0.999f;
            const float flightGravity = 0.12f;
            const float flightAirFriction = 0.7f;
            const float flightKickinDuration = 6f;

            if (player.CanFly)
            {
                if (self.animation == Player.AnimationIndex.HangFromBeam)
                {
                    player.preventFlight = 15;
                }
                else if (player.preventFlight > 0)
                {
                    player.preventFlight--;
                }

                if (player.isFlying)
                {
                    player.flyingBuzzSound.Volume = Mathf.Lerp(0f, 1f, player.currentFlightDuration / flightKickinDuration);

                    player.currentFlightDuration++;

                    self.AerobicIncrease(0.08f);

                    self.gravity = Mathf.Lerp(normalGravity, flightGravity, player.currentFlightDuration / flightKickinDuration);
                    self.airFriction = Mathf.Lerp(normalAirFriction, flightAirFriction, player.currentFlightDuration / flightKickinDuration);


                    if (self.input[0].x > 0)
                    {
                        self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x + player.WingSpeed;
                        self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x - 1f;
                    }
                    else if (self.input[0].x < 0)
                    {
                        self.bodyChunks[0].vel.x = self.bodyChunks[0].vel.x - player.WingSpeed;
                        self.bodyChunks[1].vel.x = self.bodyChunks[1].vel.x + 1f;
                    }

                    if (self.room.gravity <= 0.5)
                    {
                        if (self.input[0].y > 0)
                        {
                            self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.WingSpeed;
                            self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 1f;
                        }
                        else if (self.input[0].y < 0)
                        {
                            self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - player.WingSpeed;
                            self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 1f;
                        }
                    }
                    else if (player.UnlockedVerticalFlight)
                    {
                        if (self.input[0].y > 0)
                        {
                            self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y + player.WingSpeed * 0.5f;
                            self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y - 0.3f;
                        }
                        else if (self.input[0].y < 0)
                        {
                            self.bodyChunks[0].vel.y = self.bodyChunks[0].vel.y - player.WingSpeed;
                            self.bodyChunks[1].vel.y = self.bodyChunks[1].vel.y + 0.3f;
                        }
                    }

                    player.wingStaminaRecoveryCooldown = 40;
                    player.wingStamina--;

                    if (!self.input[0].jmp || !player.CanSustainFlight())
                    {
                        player.StopFlight();
                    }
                }
                else
                {
                    player.flyingBuzzSound.Volume = Mathf.Lerp(1f, 0f, player.timeSinceLastFlight / flightKickinDuration);

                    player.timeSinceLastFlight++;

                    player.flyingBuzzSound.Volume = 0f;

                    if (player.wingStaminaRecoveryCooldown > 0)
                    {
                        player.wingStaminaRecoveryCooldown--;
                    }
                    else
                    {
                        player.wingStamina = Mathf.Min(player.wingStamina + player.WingStaminaRecovery, player.WingStaminaMax);
                    }

                    if (self.wantToJump > 0 && player.wingStamina > player.MinimumFlightStamina && player.CanSustainFlight())
                    {
                        player.InitiateFlight();
                    }

                    self.airFriction = normalAirFriction;
                    self.gravity = normalGravity;
                }
            }

            if (player.preventGrabs > 0)
            {
                player.preventGrabs--;
            }

            player.flyingBuzzSound.Update();
            player.RecreateTailIfNeeded(self.graphicsModule as PlayerGraphics);
        }

        private static void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            PlayerData.Add(self, new PlayerEx(self));
        }
        #endregion
    }
}
