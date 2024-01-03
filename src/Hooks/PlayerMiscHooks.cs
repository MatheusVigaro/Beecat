using BeeWorld.Extensions;

namespace BeeWorld.Hooks;

public static class PlayerMiscHooks
{
    public static void Apply()
    {
        On.Player.UpdateMSC += Player_UpdateMSC;
        On.Player.CanBeSwallowed += Player_CanBeSwallowed;
        On.Player.Grabability += Player_Grabability;
        On.Player.DeathByBiteMultiplier += Player_DeathByBiteMultiplier;

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