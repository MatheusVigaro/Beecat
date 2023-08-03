using System.Linq;
using BeeWorld.Extensions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace BeeWorld.Hooks;

public static class PlayerCombatHooks
{
    public static void Apply()
    {
        On.Player.UpdateMSC += Player_UpdateMSC;
        On.SlugcatStats.SlugcatCanMaul += SlugcatStats_SlugcatCanMaul;

        IL.Player.ThrowObject += Player_ThrowObject;
        IL.Player.SlugSlamConditions += Player_SlugSlamConditions;
    }

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

    private static bool SlugcatStats_SlugcatCanMaul(On.SlugcatStats.orig_SlugcatCanMaul orig, SlugcatStats.Name slugcatNum)
    {
        return slugcatNum == BeeEnums.Beecat || orig(slugcatNum);
    }

    private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
    {
        orig(self);

        if (!self.IsBee(out var bee))
        {
            return;
        }

        if (bee.stingerAttackCounter > -15)
        {
            bee.stingerAttackCounter--;
        }

        if (bee.stingerAttackCounter == 0 && bee.stingerTargetChunk != null)
        {
            if (bee.stingerTargetChunk?.owner is Creature target && !target.dead)
            {
                self.room.PlaySound(SoundID.Spear_Stick_In_Creature, bee.stingerTargetChunk);
                target.Violence(self.bodyChunks[1], (bee.stingerTargetChunk.pos - self.bodyChunks[1].pos).normalized, bee.stingerTargetChunk, null, Creature.DamageType.Stab, 0.2f, 800);
                bee.stingerUsed = !BeeOptions.UnlimitedStingers.Value;
            }
        }

        if (bee.stingerAttackCooldown > 0)
        {
            bee.stingerAttackCooldown--;
        }

        if (self.Input().StingerAttackPressed && (self.Consious || (self.dangerGraspTime < 200 && !self.dead)) && !bee.stingerUsed && bee.stingerAttackCooldown <= 0)
        {
            bee.StingerAttack(new Vector2(60 * self.flipDirection, 20), new Vector2(20 * self.flipDirection, 20));
        }
        
        if (bee.stingerAttackCounter > 0 && bee.StingerTargetPos != default)
        {
            self.Tail().LastOrDefault()!.pos = bee.CurrentStingerAttackPos;
        } 
    }
}