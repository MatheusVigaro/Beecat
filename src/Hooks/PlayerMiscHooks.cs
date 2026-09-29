using BeeWorld.Extensions;
using Menu;
using wa;

namespace BeeWorld.Hooks;

public static class PlayerMiscHooks
{
    public static void Apply()
    {
        On.Player.UpdateMSC += Player_UpdateMSC;
        On.Player.CanBeSwallowed += Player_CanBeSwallowed;
        On.Player.Grabability += Player_Grabability;
        On.Player.DeathByBiteMultiplier += Player_DeathByBiteMultiplier;
        On.Player.checkInput += Player_checkInput;
        
        #region moon fix, not proud of this one
        On.PlayerGraphics.CosmeticPearl.Update += (orig, self) =>
        {
            if (!self.pGraphics.player.IsBee()) orig(self);
        };

        On.PlayerGraphics.CosmeticPearl.AddToContainer += (orig, self, leaser, cam, contatiner) =>
        {
            if (!self.pGraphics.player.IsBee()) orig(self, leaser, cam, contatiner);
        };

        On.PlayerGraphics.CosmeticPearl.InitiateSprites += (orig, self, leaser, cam) =>
        {
            if (!self.pGraphics.player.IsBee()) orig(self, leaser, cam);
        };

        On.PlayerGraphics.CosmeticPearl.DrawSprites += (orig, self, leaser, cam, stacker, pos) =>
        {
            if (!self.pGraphics.player.IsBee()) orig(self, leaser, cam, stacker, pos);
        };

        On.PlayerGraphics.CosmeticPearl.ApplyPalette += (orig, self, leaser, cam, palette) =>
        {
            if (!self.pGraphics.player.IsBee()) orig(self, leaser, cam, palette);
        };
        #endregion
    }

    private static void Player_checkInput(On.Player.orig_checkInput orig, Player self)
    {
        orig(self);
        
        if (!self.IsBee(out var bee)) return;

        if (bee.nom >= 0) bee.nom--;
        if (bee.bleh >= 0) bee.bleh--;
        if (self.grasps.Any(x => x?.grabbed is not HoneyCombT))
        {
            if (self.FreeHand() != -1 && bee.bleh <= 0 && self.CurrentFood >= 3)
            {
                if (bee.nom >= 10)
                {
                    self.Blink(15);
                }
                if (bee.nom >= 50)
                {
                    var wa = new AbstractHoneyComb(self.room.world, self.abstractCreature.pos, self.room.game.GetNewID());
                    self.room.abstractRoom.AddEntity(wa);
                    wa.RealizeInRoom();
                    self.SlugcatGrab(wa.realizedObject, self.FreeHand());
                    bee.nom = 0;
                    bee.bleh = 500;
                    self.SubtractFood(3);
                }
                if (self.input[0].pckp && self.input[0].jmp)
                {
                    bee.nom += 2;
                }
            }
        }
    }

    public static float wa = Random.value;
    #region his code is mess
    #endregion

    private static float Player_DeathByBiteMultiplier(On.Player.orig_DeathByBiteMultiplier orig, Player self)
    {
        var result = orig(self);
        if (!self.IsBee()) return result;

        return 0.3f;
    }

    private static Player.ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (self.IsBee(out var bee) && bee.preventGrabs > 0)
        {
            return Player.ObjectGrabability.CantGrab;
        }

        return orig(self, obj);
    }

    private static bool Player_CanBeSwallowed(On.Player.orig_CanBeSwallowed orig, Player self, PhysicalObject testObj)
    {
        return orig(self, testObj) || (testObj is Flower && self.IsBee());
    }

    private static void Player_UpdateMSC(On.Player.orig_UpdateMSC orig, Player self)
    {
        orig(self);

        if (!self.IsBee(out var bee))
        {
            return;
        }

        if (bee.preventGrabs > 0)
        {
            bee.preventGrabs--;
        }

        bee.flyingBuzzSound.Update();
        bee.RecreateTailIfNeeded(self.PlayerGraphics());
    }
}
