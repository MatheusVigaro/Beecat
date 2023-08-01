﻿using System.Linq;
using System;
using System.Runtime.CompilerServices;
using RWCustom;
using SlugBase.DataTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BeeWorld;

public class BeePlayerData
{
    public const int WingStaminaMaxBase = 200;
    public const float WingStaminaRecoveryBase = 1;
    public const float WingSpeedBase = 2;
    public readonly bool IsBee;

    public bool CanFly => WingStaminaMax > 0 && WingSpeed > 0;
    public float MinimumFlightStamina => WingStaminaMax * 0.1f;
    public double LowWingStamina => MinimumFlightStamina * 3;

    public int WingStaminaMax => UnlockedExtraStamina ? (int)(WingStaminaMaxBase * 1.6f) : WingStaminaMaxBase;
    public float WingStaminaRecovery => UnlockedExtraStamina ? WingStaminaRecoveryBase * 1.2f : WingStaminaRecoveryBase;
    public float WingSpeed => UnlockedFasterWings ? WingSpeedBase * 1.3f : WingSpeedBase;

    public bool UnlockedExtraStamina = false;
    public bool UnlockedVerticalFlight = false;
    public bool UnlockedFasterWings = false;

    public int preventGrabs;
    public bool isFlying;
    public float wingStamina;
    public int wingStaminaRecoveryCooldown;
    public int currentFlightDuration;
    public int timeSinceLastFlight;
    public int preventFlight;
    public int lastTail;

    public int initialWingSprite;
    public int antennaeSprite;
    public int floofSprite;
    public int staminaSprite;

    public Vector2 staminaLastPos;
    public Vector2 staminaPos;
    public float staminaLastFill;
    public float staminaFill;
    public float staminaLastFade;
    public float staminaFade;
    public int staminaFadeCounter;

    public float[,] wingDeployment;
    public float[,] wingDeploymentSpeed;
    public float wingDeploymentGetTo;
    public float wingOffset;
    public float wingTimeAdd;
    public Vector2 zRotation;
    public Vector2 lastZRotation;
    public float wingYAdjust;

    public WeakReference<Player> playerRef;

    public DynamicSoundLoop flyingBuzzSound;

    public Color BodyColor;
    public Color EyesColor;
    public Color WingColor;
    public Color TailColor;
    public Color StripeColor;
    public Color AntennaeColor;
    public Color FluffColor;

    public FAtlas TailAtlas;

    public BeePlayerData(Player player)
    {
        IsBee = player.slugcatStats.name == BeeEnums.Beecat;

        playerRef = new WeakReference<Player>(player);

        if (!IsBee)
        {
            return;
        }

        SetupSounds(player);

        lastTail = -1;
        wingStamina = WingStaminaMax;
        timeSinceLastFlight = 200;
    }

    ~BeePlayerData() {
        try
        {
            TailAtlas.Unload();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void LoadTailAtlas()
    {
        var tailTexture = new Texture2D(Plugin.TailTexture.width, Plugin.TailTexture.height, TextureFormat.ARGB32, false);
        Graphics.CopyTexture(Plugin.TailTexture, tailTexture);

        Utils.MapTextureColor(tailTexture, 255, StripeColor, false);
        Utils.MapTextureColor(tailTexture, 0, TailColor);

        if (playerRef.TryGetTarget(out var player)) {
            TailAtlas = Futile.atlasManager.LoadAtlasFromTexture("beecattailtexture_" + player.playerState.playerNumber + Time.time + Random.value, tailTexture, false);
        }
    }

    private void SetupSounds(Player player)
    {
        flyingBuzzSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
        flyingBuzzSound.sound = BeeEnums.Sound.BeeBuzz;
        flyingBuzzSound.Pitch = 1f;
        flyingBuzzSound.Volume = 1f;
    }

    public void SetupColors(PlayerGraphics pg)
    {
        BodyColor = pg.GetColor(BeeEnums.Color.Body) ?? Custom.hexToColor("ffcf0d");
        EyesColor = pg.GetColor(BeeEnums.Color.Eyes) ?? Custom.hexToColor("010101");
        WingColor = pg.GetColor(BeeEnums.Color.Wings) ?? Color.white;
        TailColor = pg.GetColor(BeeEnums.Color.Tail) ?? Custom.hexToColor("ffcf0d");
        StripeColor = pg.GetColor(BeeEnums.Color.TailStripes) ?? Custom.hexToColor("010101");
        AntennaeColor = pg.GetColor(BeeEnums.Color.Antennae) ?? Custom.hexToColor("010101");
        FluffColor = pg.GetColor(BeeEnums.Color.NeckFluff) ?? Custom.hexToColor("161c24");
    }

    public void StopFlight()
    {
        currentFlightDuration = 0;
        timeSinceLastFlight = 0;
        isFlying = false;
    }

    public void InitiateFlight()
    {
        if (!playerRef.TryGetTarget(out var player))
        {
            return;
        }

        player.bodyMode = Player.BodyModeIndex.Default;
        player.animation = Player.AnimationIndex.None;
        player.wantToJump = 0;
        currentFlightDuration = 0;
        timeSinceLastFlight = 0;
        isFlying = true;
    }

    public bool CanSustainFlight()
    {
        if (!playerRef.TryGetTarget(out var player))
        {
            return false;
        }

        return wingStamina > 0 &&
               preventFlight <= 0 &&
               player.canJump <= 0 &&
               player.canWallJump == 0 && //-- Equals zero is correct, is negative when jumping to the left
               player.Consious &&
               player.bodyMode != Player.BodyModeIndex.Crawl &&
               player.bodyMode != Player.BodyModeIndex.CorridorClimb &&
               player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut &&
               player.animation != Player.AnimationIndex.HangFromBeam &&
               player.animation != Player.AnimationIndex.ClimbOnBeam &&
               player.bodyMode != Player.BodyModeIndex.WallClimb &&
               player.bodyMode != Player.BodyModeIndex.Swimming &&
               player.animation != Player.AnimationIndex.AntlerClimb &&
               player.animation != Player.AnimationIndex.VineGrab &&
               player.animation != Player.AnimationIndex.ZeroGPoleGrab;
    }

    public int WingSprite(int side, int wing)
    {
        return initialWingSprite + side + wing + wing;
    }

    public void RecreateTailIfNeeded(PlayerGraphics self)
    {
        var currentFood = self.player.CurrentFood;
        var oldTail = self.tail;

        if (currentFood < 6)
        {
            if (lastTail != 2)
            {
                lastTail = 2;
                self.tail = new TailSegment[5];
                self.tail[0] = new TailSegment(self, 8f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 6f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 4f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 2f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
                self.tail[4] = new TailSegment(self, 1f, 7f, self.tail[3], 0.85f, 1f, 0.5f, true);
            }
        }
        else if (currentFood < 11)
        {
            if (lastTail != 3)
            {
                lastTail = 3;
                self.tail = new TailSegment[6];
                self.tail[0] = new TailSegment(self, 10f, 4f, null, 0.85f, 1f, 1f, true);
                self.tail[1] = new TailSegment(self, 8f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
                self.tail[2] = new TailSegment(self, 6f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
                self.tail[3] = new TailSegment(self, 4f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
                self.tail[4] = new TailSegment(self, 2f, 7f, self.tail[3], 0.85f, 1f, 0.5f, true);
                self.tail[5] = new TailSegment(self, 1f, 7f, self.tail[4], 0.85f, 1f, 0.5f, true);
            }
        }
        else if (lastTail != 4)
        {
            lastTail = 4;
            self.tail = new TailSegment[7];
            self.tail[0] = new TailSegment(self, 11f, 4f, null, 0.85f, 1f, 1f, true);
            self.tail[1] = new TailSegment(self, 9f, 7f, self.tail[0], 0.85f, 1f, 0.5f, true);
            self.tail[2] = new TailSegment(self, 7f, 7f, self.tail[1], 0.85f, 1f, 0.5f, true);
            self.tail[3] = new TailSegment(self, 6f, 7f, self.tail[2], 0.85f, 1f, 0.5f, true);
            self.tail[4] = new TailSegment(self, 4f, 7f, self.tail[3], 0.85f, 1f, 0.5f, true);
            self.tail[5] = new TailSegment(self, 3f, 7f, self.tail[4], 0.85f, 1f, 0.5f, true);
            self.tail[6] = new TailSegment(self, 2f, 7f, self.tail[5], 0.85f, 1f, 0.5f, true);
        }

        if (oldTail != self.tail)
        {
            for (var i = 0; i < self.tail.Length && i < oldTail.Length; i++)
            {
                self.tail[i].pos = oldTail[i].pos;
                self.tail[i].lastPos = oldTail[i].lastPos;
                self.tail[i].vel = oldTail[i].vel;
                self.tail[i].terrainContact = oldTail[i].terrainContact;
                self.tail[i].stretched = oldTail[i].stretched;
            }

            var bp = self.bodyParts.ToList();
            bp.RemoveAll(x => x is TailSegment);
            bp.AddRange(self.tail);

            self.bodyParts = bp.ToArray();
        }
    }
}

public static class PlayerExtension
{
    private static readonly ConditionalWeakTable<Player, BeePlayerData> _cwt = new();
    
    public static BeePlayerData Bee(this Player player) => _cwt.GetValue(player, _ => new BeePlayerData(player));

    public static Color? GetColor(this PlayerGraphics pg, PlayerColor color) => color.GetColor(pg);

    public static Color? GetColor(this Player player, PlayerColor color) => (player.graphicsModule as PlayerGraphics)?.GetColor(color);

    public static bool IsBee(this Player player) => player.Bee().IsBee;

    public static bool IsBee(this Player player, out BeePlayerData bee)
    {
        bee = player.Bee();
        return bee.IsBee;
    }
}