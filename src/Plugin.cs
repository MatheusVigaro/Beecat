using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using BeeWorld.Hooks;
using System.IO;
using RWCustom;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace BeeWorld;

[BepInPlugin(MOD_ID, MOD_NAME, VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "beeworld";
    public const string MOD_NAME = "Beecat";
    public const string VERSION = "1.2.0";
    public const string AUTHORS = "Vigaro";
        
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorld_OnOnModsInit;
        On.RainWorld.OnModsDisabled += RainWorld_OnModsDisabled;
    }

    private void RainWorld_OnModsDisabled(On.RainWorld.orig_OnModsDisabled orig, RainWorld self, ModManager.Mod[] newlyDisabledMods)
    {
        orig(self, newlyDisabledMods);
        BeeEnums.UnregisterValues();
    }

    public static readonly PlayerFeature<int> WingStamina = PlayerInt("beeworld/wingstamina");
    public static readonly PlayerFeature<float> WingStaminaRecovery = PlayerFloat("beeworld/wingstaminarecovery");
    public static readonly PlayerFeature<float> WingSpeed = PlayerFloat("beeworld/wingsspeed");
    public static readonly PlayerFeature<bool> BeeCompanion = PlayerBool("beeworld/beecompanion");

    public static Texture2D TailTexture;

    private bool IsInit;

    private void RainWorld_OnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            BeeEnums.RegisterValues();
            BeeOptions.RegisterOI();
            
            if (IsInit) return;
            IsInit = true;

            Futile.atlasManager.LoadAtlas("atlases/beewings");
            Futile.atlasManager.LoadAtlas("atlases/beeantennaehead");
            Futile.atlasManager.LoadAtlas("atlases/floof");
            Futile.atlasManager.LoadAtlas("atlases/floof2");
            Futile.atlasManager.LoadAtlas("atlases/beecathands");
            Futile.atlasManager.LoadImage("atlases/beecatstinger");

            TailTexture = new Texture2D(150, 75, TextureFormat.ARGB32, false);
            var tailTextureFile = AssetManager.ResolveFilePath("textures/beecattail.png");
            if (File.Exists(tailTextureFile))
            {
                var rawData = File.ReadAllBytes(tailTextureFile);
                TailTexture.LoadImage(rawData);
            }
            
            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/beecatshaders"));
            Custom.rainWorld.Shaders["BeecatStaminaBar"] = FShader.CreateShader("BeecatStaminaBar", bundle.LoadAsset<Shader>("Assets/BeecatStaminaBar.shader"));

            PlayerMiscHooks.Apply();
            PlayerFlightHooks.Apply();
            PlayerCombatHooks.Apply();
            PlayerGraphicsHooks.Apply();
            //RoomHooks.Init();
            //SaveDataHooks.Init();
            //WorldHooks.Init();

            //Content.Register(new FlowerFisob());

            Debug.Log($"Plugin {MOD_ID} is loaded!");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}