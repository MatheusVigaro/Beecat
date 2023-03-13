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
using BeeWorld.Hooks;
using Fisobs.Core;
using System.IO;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace BeeWorld
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public partial class Plugin : BaseUnityPlugin
    {
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorld_OnOnModsInit;
        }

        public static readonly PlayerFeature<int> WingStamina = PlayerInt("beeworld/wingstamina");
        public static readonly PlayerFeature<float> WingStaminaRecovery = PlayerFloat("beeworld/wingstaminarecovery");
        public static readonly PlayerFeature<float> WingSpeed = PlayerFloat("beeworld/wingsspeed");
        public static readonly PlayerFeature<bool> BeeCompanion = PlayerBool("beeworld/beecompanion");

        public static readonly PlayerFeature<PlayerColor> WingColor = PlayerCustomColor("Wings");
        public static readonly PlayerFeature<PlayerColor> TailColor = PlayerCustomColor("Tail");
        public static readonly PlayerFeature<PlayerColor> StripeColor = PlayerCustomColor("Tail Stripes");

        public static Texture2D TailTexture;

        private bool IsInit;

        private void RainWorld_OnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;
                IsInit = true;

                Futile.atlasManager.LoadAtlas("atlases/beewings");
                Futile.atlasManager.LoadAtlas("atlases/beeantennaehead");
                Futile.atlasManager.LoadAtlas("atlases/floof");
                Futile.atlasManager.LoadAtlas("atlases/floof2");
                Futile.atlasManager.LoadAtlas("atlases/beecathands");
                Futile.atlasManager.LoadAtlas("atlases/beecattail");

                TailTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
                var tailTextureFile = AssetManager.ResolveFilePath("textures/beecattail.png");
                if (File.Exists(tailTextureFile))
                {
                    var rawData = File.ReadAllBytes(tailTextureFile);
                    TailTexture.LoadImage(rawData);
                }

                BeeEnums.RegisterValues();

                PlayerHooks.Init();
                PlayerGraphicsHooks.Init();
                RoomHooks.Init();
                SaveDataHooks.Init();
                WorldHooks.Init();

                Content.Register(new FlowerFisob());

                Debug.Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        /*
        static VideoProjector projector;

        private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (projector != null && !self.IsJollyPlayer)
            {
                projector.Update(self, eu);
            }
            else if (projector == null && Input.GetKey(KeyCode.Y))
            {
                projector = new VideoProjector();
            }
        }*/
    }
}