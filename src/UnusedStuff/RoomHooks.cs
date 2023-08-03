/*
using MoreSlugcats;
using System.Linq;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace BeeWorld.Hooks;

public class RoomHooks
{
    public static void Init()
    {
        On.Room.NowViewed += Room_NowViewed;
    }

    static public ConditionalWeakTable<AbstractRoom, AbstractFlower> FlowerInRoom = new();

    private static void Room_NowViewed(On.Room.orig_NowViewed orig, Room self)
    {
        orig(self);

        var region = self.world?.region?.name;
        if (string.IsNullOrEmpty(region))
        {
            return;
        }

        if (!self.game.IsStorySession || !SaveDataHooks.SaveData.TryGetValue(self.game.GetStorySession.saveState.miscWorldSaveData, out var saveData))
        {
            return;
        }

        var flower = FlowerQuest.GetFlower(region);
        if (flower == null)
        {
            return;
        }

        if (saveData.GetHasFlowerForRegion(region))
        {
            return;
        }

        if (self.abstractRoom.name == flower.Room)
        {
            if (!FlowerInRoom.TryGetValue(self.abstractRoom, out var flowerInRoom))
            {
                var abstractFlower = new AbstractFlower(self.world, self.GetWorldCoordinate(flower.Position), self.game.GetNewID());
                self.abstractRoom.AddEntity(abstractFlower);
                abstractFlower.RealizeInRoom();

                FlowerInRoom.Add(self.abstractRoom, flowerInRoom);
            }
        }

        var targetRoom = self.world.abstractRooms.FirstOrDefault(x => x.name == flower.Room);

        if (targetRoom == null)
        {
            return;
        }

        var path = RoomPathFinder.FindShortestPath(self.abstractRoom, targetRoom);

        if (path != null && path.Count <= 5)
        {
            var num_keyframes = (Random.value < 0.5f) ? 3 : 4;
            var amount = 100 * (6 - path.Count); //0~500

            Debug.LogWarning("Adding " + amount + " FairyParticles");
            for (var i = 0; i < amount; i++)
            {
                FairyParticle fairyParticle = new FairyParticle((float)Random.Range(0, 360), num_keyframes, 60f, 180f, 40f, 100f, 5f, 30f);
                self.AddObject(fairyParticle);

                //-- May be changed depending on the sprite
                fairyParticle.spriteName = "SkyDandelion";
                fairyParticle.scale_multiplier = 0.1f;

                fairyParticle.minHSL = new Vector3(0, 1f, 0.4f);
                fairyParticle.maxHSL = new Vector3(0.05f, 1f, 0.65f);
                fairyParticle.scale_min = 4f;
                fairyParticle.scale_max = 8f;
                fairyParticle.direction_min = 0f;
                fairyParticle.direction_max = 360f;
                fairyParticle.dir_deviation_min = 5f;
                fairyParticle.dir_deviation_max = 30f;
                fairyParticle.interp_dir_method = FairyParticle.LerpMethod.SIN_IO;
                fairyParticle.alpha_trans_ratio = 0.75f;
                fairyParticle.num_keyframes = num_keyframes;
                fairyParticle.interp_speed_method = FairyParticle.LerpMethod.SIN_IO;
                fairyParticle.interp_dist_min = 40f;
                fairyParticle.interp_dist_max = 100f;
                fairyParticle.interp_duration_min = 60f;
                fairyParticle.interp_duration_max = 180f;
                fairyParticle.interp_trans_ratio = 0.5f;
                fairyParticle.pulse_min = 1f;
                fairyParticle.pulse_max = 1f;
                fairyParticle.pulse_rate = 0;
                fairyParticle.abs_pulse = false;
                fairyParticle.glowRadius = 0;
                fairyParticle.glowIntensity = 0.5f;
                fairyParticle.rotation_rate = 1f;

                fairyParticle.ResetNoPositionChange();
            }
        }
    }
}*/