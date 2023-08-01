﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BeeWorld.Hooks;

public static class PlayerHooks
{
    public static void Apply()
    {
        On.Player.UpdateMSC += Player_UpdateMSC;
        On.Player.CanBeSwallowed += Player_CanBeSwallowed;
        On.SlugcatStats.SlugcatCanMaul += SlugcatStats_SlugcatCanMaul;
        On.Player.Grabability += Player_Grabability;

        IL.Player.ThrowObject += Player_ThrowObject;
        IL.Player.SlugSlamConditions += Player_SlugSlamConditions;
    }

    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (self.IsBee(out var bee) && bee.preventGrabs > 0)
        {
            return Player.ObjectGrabability.CantGrab;
        }

        return orig(self, obj);
    }

    #region ILHooks

    private static void Player_ThrowObject(ILContext il)
    {
        var cursor = new ILCursor(il);

        cursor.GotoNext(MoveType.Before, i => i.MatchLdsfld<Player.AnimationIndex>("Flip"));
        cursor.GotoNext(MoveType.Before, i => i.MatchLdloc(1));

        cursor.MoveAfterLabels();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Player player) => player.IsBee(out var bee) && bee.isFlying);
        cursor.Emit(OpCodes.Or);
    }
    
    private static void Player_SlugSlamConditions(ILContext il)
    {
        var cursor = new ILCursor(il);
        cursor.GotoNext(MoveType.Before, i => i.MatchBrfalse(out _));

        cursor.MoveAfterLabels();

        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Player self) => !(self.IsBee() && self.FoodInStomach >= 8));
        cursor.Emit(OpCodes.And);
    }

    #endregion

    #region Player

    private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
    {
        return orig(self, testObj) || (testObj is Flower && self.IsBee());
    }

    private static bool SlugcatStats_SlugcatCanMaul(On.SlugcatStats.orig_SlugcatCanMaul orig, SlugcatStats.Name slugcatNum)
    {
        return slugcatNum.value.ToLower() == "bee" || orig(slugcatNum);
    }

    private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
    {
        orig(self);

        if (!self.IsBee(out var bee))
        {
            return;
        }

        const float normalGravity = 0.9f;
        const float normalAirFriction = 0.999f;
        const float flightGravity = 0.12f;
        const float flightAirFriction = 0.7f;
        const float flightKickinDuration = 6f;

        if (bee.CanFly)
        {
            if (self.animation == Player.AnimationIndex.HangFromBeam)
            {
                bee.preventFlight = 15;
            }
            else if (bee.preventFlight > 0)
            {
                bee.preventFlight--;
            }

            if (bee.isFlying)
            {
                bee.flyingBuzzSound.Volume = Mathf.Lerp(0f, 1f, bee.currentFlightDuration / flightKickinDuration);

                bee.currentFlightDuration++;

                self.AerobicIncrease(0.08f);

                self.gravity = Mathf.Lerp(normalGravity, flightGravity, bee.currentFlightDuration / flightKickinDuration);
                self.airFriction = Mathf.Lerp(normalAirFriction, flightAirFriction, bee.currentFlightDuration / flightKickinDuration);


                if (self.input[0].x > 0)
                {
                    self.bodyChunks[0].vel.x += bee.WingSpeed;
                    self.bodyChunks[1].vel.x -= 1f;
                }
                else if (self.input[0].x < 0)
                {
                    self.bodyChunks[0].vel.x -= bee.WingSpeed;
                    self.bodyChunks[1].vel.x += 1f;
                }

                if (self.room.gravity <= 0.5)
                {
                    if (self.input[0].y > 0)
                    {
                        self.bodyChunks[0].vel.y += bee.WingSpeed;
                        self.bodyChunks[1].vel.y -= 1f;
                    }
                    else if (self.input[0].y < 0)
                    {
                        self.bodyChunks[0].vel.y -= bee.WingSpeed;
                        self.bodyChunks[1].vel.y += 1f;
                    }
                }
                else if (bee.UnlockedVerticalFlight)
                {
                    if (self.input[0].y > 0)
                    {
                        self.bodyChunks[0].vel.y += bee.WingSpeed * 0.5f;
                        self.bodyChunks[1].vel.y -= 0.3f;
                    }
                    else if (self.input[0].y < 0)
                    {
                        self.bodyChunks[0].vel.y -= bee.WingSpeed;
                        self.bodyChunks[1].vel.y += 0.3f;
                    }
                }

                bee.wingStaminaRecoveryCooldown = 40;
                bee.wingStamina--;

                if (!self.input[0].jmp || !bee.CanSustainFlight())
                {
                    bee.StopFlight();
                }
            }
            else
            {
                bee.flyingBuzzSound.Volume = Mathf.Lerp(1f, 0f, bee.timeSinceLastFlight / flightKickinDuration);

                bee.timeSinceLastFlight++;

                bee.flyingBuzzSound.Volume = 0f;

                if (bee.wingStaminaRecoveryCooldown > 0)
                {
                    bee.wingStaminaRecoveryCooldown--;
                }
                else
                {
                    bee.wingStamina = Mathf.Min(bee.wingStamina + bee.WingStaminaRecovery, bee.WingStaminaMax);
                }

                if (self.wantToJump > 0 && bee.wingStamina > bee.MinimumFlightStamina && bee.CanSustainFlight())
                {
                    bee.InitiateFlight();
                }

                self.airFriction = normalAirFriction;
                self.gravity = normalGravity;
            }
        }

        if (bee.preventGrabs > 0)
        {
            bee.preventGrabs--;
        }

        bee.flyingBuzzSound.Update();
        bee.RecreateTailIfNeeded(self.graphicsModule as PlayerGraphics);
    }
    #endregion
}