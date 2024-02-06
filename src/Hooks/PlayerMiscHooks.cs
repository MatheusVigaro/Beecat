using BeeWorld.Extensions;
using Menu;
using wa;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

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


        On.Menu.SlugcatSelectMenu.UpdateStartButtonText += NULL;
        On.Menu.SlugcatSelectMenu.CheckJollyCoopAvailable += SlugcatSelectMenu_CheckJollyCoopAvailable;
        On.Player.Update += Player_Update;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.SlugcatStats.HiddenOrUnplayableSlugcat += Codedphone;
        On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;

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

    private static void PebblesConversation_AddEvents(On.SSOracleBehavior.PebblesConversation.orig_AddEvents orig, SSOracleBehavior.PebblesConversation self)
    {
        if (!self.owner.player.IsBee(out var bee) && !ModManager.MSC)
        {
            orig(self);
            return;
        }
        self.events.Add(new Conversation.WaitEvent(self, 50));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Ah, another peculiar creature."), 0));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("..."), 0));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("Fascinating."), 0));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You appear to be part slugcat, part bee."), 5));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("A 'Beecat', perhaps."), 20));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("..."), 20));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("It seems you have the ability to fly around."), 10));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("..."), 20));
        self.events.Add(new Conversation.WaitEvent(self, 10));
        self.events.Add(new Conversation.TextEvent(self, 0, self.Translate("You are free to go now."), 5));

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
    private static bool Codedphone(On.SlugcatStats.orig_HiddenOrUnplayableSlugcat orig, SlugcatStats.Name self)
    {
        var result = orig(self);
        if(self == BeeEnums.Secret)
        {
            if(wa >= 0.01f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return result;
    }
    private static bool SlugcatSelectMenu_CheckJollyCoopAvailable(On.Menu.SlugcatSelectMenu.orig_CheckJollyCoopAvailable orig, SlugcatSelectMenu self, SlugcatStats.Name slugcat)
    {
        var result = orig(self, slugcat);
        if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == BeeEnums.Secret)
        {
            return false;
        }
        return result;
    }

    private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if(self.owner is Player PL)
        {
            if (PL.slugcatStats.name == BeeEnums.Secret)
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
        if (self.slugcatStats.name == BeeEnums.Secret)
        {
            self.Die();
        }
    }
    private static void NULL(On.Menu.SlugcatSelectMenu.orig_UpdateStartButtonText orig, Menu.SlugcatSelectMenu self)
    {
        orig(self);
        if (self.slugcatPages[self.slugcatPageIndex].slugcatNumber == BeeEnums.Secret)
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
