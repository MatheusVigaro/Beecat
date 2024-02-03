﻿using BeeWorld.Extensions;
using Menu;

namespace BeeWorld.Hooks;

public static class PlayerMiscHooks
{
    public static void Apply()
    {
        On.Player.UpdateMSC += Player_UpdateMSC;
        On.Player.CanBeSwallowed += Player_CanBeSwallowed;
        On.Player.Grabability += Player_Grabability;
        On.Player.DeathByBiteMultiplier += Player_DeathByBiteMultiplier;
        On.Menu.SlugcatSelectMenu.UpdateStartButtonText += NULL;
        On.Player.Update += Player_Update;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

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

    #region his code is mess
    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if(self.owner is Player PL)
        {
            if (PL.slugcatStats.name ==  BeeEnums.SnowFlake)
            {
                for (var i = 0; i < sLeaser.sprites.Length; i++)
                {
                    if (i != 9)
                    {
                        sLeaser.sprites[i].isVisible = false;
                    }
                }
            }
        }    
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.slugcatStats.name == BeeEnums.SnowFlake)
        {
            self.Die();
        }
    }
    private static void NULL(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, Menu.SlugcatSelectMenu self)
    {
        orig(self);
        if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == BeeEnums.SnowFlake)
        {
            self.startButton.fillTime = 99999999999999999;
            self.startButton.menuLabel.text = "? ? ?";
            if (self.slugcatPages[self.slugcatPageIndex] is SlugcatSelectMenu.SlugcatPageNewGame page)
            {
                int wa = Random.Range(0, 25);
                if (wa <= 3)
                {
                    page.difficultyLabel.text = "???";
                }
                else if(wa <= 5)
                {
                    page.difficultyLabel.text = "?? ?";
                }
                else if (wa <= 13)
                {
                    page.difficultyLabel.text = "ERROR";
                }
                else if (wa <= 10)
                {
                    page.difficultyLabel.text = "? ??";
                }
                else if (wa <= 13)
                {
                    page.difficultyLabel.text = "#E!P M3";
                }
                else if (wa <= 15)
                {
                    page.difficultyLabel.text = "NULL";
                }
                else if (wa <= 18)
                {
                    page.difficultyLabel.text = "Cryptic?!";
                }
                else if (wa <= 20)
                {
                    page.difficultyLabel.text = "? ? ?";
                }
                else if (wa <= 25)
                {
                    page.difficultyLabel.text = "$3CR37";
                }
            }
        }
    }
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