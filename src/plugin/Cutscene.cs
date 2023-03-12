using System;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using static BeeWorld.Hooks.PlayerHooks;
using BeeWorld.Hooks;
using UnityEngine;

namespace BeeWorld
{
    public class Cutscene : UpdatableAndDeletable
    {
        public int timer;
        public float fadeOut;
        public float lastFadeOut;
        public bool shouldShutdown;

        public override void Update(bool eu)
        {
            base.Update(eu);
            timer++;

            lastFadeOut = fadeOut;
            if (fadeOut > 0f)
            {
                fadeOut = Mathf.Min(1f, fadeOut + 0.0125f);
                if (fadeOut == 1f)
                {
                    shouldShutdown = true;
                }
            }

        }

        public virtual Player.InputPackage GetInput()
        {
            return default;
        }

        public class CutsceneController : Player.PlayerController
        {
            public readonly Cutscene owner;
            public CutsceneController(Cutscene owner)
            {
                this.owner = owner;
            }

            public override Player.InputPackage GetInput()
            {
                return owner.GetInput();
            }
        }

        public class FlowerPickupCutscene : Cutscene
        {
            public Player player;
            public Flower flower;
            public AbstractPhysicalObject objectInStomach;
            public PlayerEx playerEx;

            public FlowerPickupCutscene(Player player, Flower flower)
            {
                this.player = player;
                this.flower = flower;
                PlayerData.TryGetValue(player, out playerEx);

                player.controller = new CutsceneController(this);

                if (player.grasps[0] != null && player.grasps[0].grabbed != flower)
                {
                    player.SwitchGrasps(0, 1);
                }
                if (player.grasps[1] != null)
                {
                    player.ReleaseGrasp(1);
                }

                if (player.objectInStomach != null)
                {
                    objectInStomach = player.objectInStomach;
                    player.objectInStomach = null;
                }

                if (playerEx != null)
                {
                    playerEx.preventGrabs = 210;
                }
            }

            public override void Update(bool eu)
            {
                base.Update(eu);

                if (timer == 1)
                {
                    foreach (var shortcut in room.shortcutsIndex)
                    {
                        room.lockedShortcuts.Add(shortcut);
                    }
                }

                if (timer == 200)
                {
                    player.controller = null;
                    player.objectInStomach = objectInStomach;
                    fadeOut = 0.01f;
                }

                if (shouldShutdown)
                {
                    if (SaveDataHooks.SaveData.TryGetValue(room.game.GetStorySession.saveState.miscWorldSaveData, out var saveData))
                    {
                        saveData.SetHasFlowerForRegion(room.world?.region?.name, true);

                        if (room.game.GetStorySession.saveState.deathPersistentSaveData.karmaCap < 9)
                        {
                            if (!ModManager.Expedition || !room.game.manager.rainWorld.ExpeditionMode)
                            {
                                room.game.GetStorySession.saveState.deathPersistentSaveData.karmaCap++;
                            }

                            room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.GhostScreen);
                        }
                        else
                        {
                            room.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.KarmaToMaxScreen);
                        }

                        room.game.GetStorySession.AppendTimeOnCycleEnd(true);

                        if (ModManager.Expedition && room.game.manager.rainWorld.ExpeditionMode)
                        {
                            Expedition.Expedition.coreFile.Save(runEnded: false);
                        }
                        else
                        {
                            room.game.GetStorySession.saveState.progression.SaveWorldStateAndProgression(false);
                        }
                    }

                    this.Destroy();
                }
            }

            public override Player.InputPackage GetInput()
            {
                if (slatedForDeletetion) return default;

                if (timer < 40)
                {
                    return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 1, false, false, false, false, false);
                }
                else if (timer < 200 && player.objectInStomach == null)
                {
                    return new Player.InputPackage(false, Options.ControlSetup.Preset.None, 0, 0, false, false, true, false, false);
                }
                return default;
            }
        }
    }
}