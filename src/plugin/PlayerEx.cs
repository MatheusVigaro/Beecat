using BeeWorld;
using SlugBase.Features;
using SlugBase;
using System.Linq;
using System;
using UnityEngine;

namespace BeeWorld
{
    public class PlayerEx
    {
        public readonly int WingStaminaMaxBase;
        public readonly float WingStaminaRecoveryBase;
        public readonly float WingSpeed;
        public readonly bool BeeCompanion;
        public readonly bool IsBee;

        public readonly SlugcatStats.Name Name;
        public SlugBaseCharacter Character;

        public bool CanFly => WingStaminaMax > 0 && WingSpeed > 0;
        public float MinimumFlightStamina => WingStaminaMax * 0.1f;

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

        public Vector2 lastFloofPos;
        public Vector2 lastAntennaePos;

        public WeakReference<Player> playerRef;
        public float[,] wingDeployment;
        public float[,] wingDeploymentSpeed;
        public float wingDeploymentGetTo;
        public float wingOffset;
        public float wingTimeAdd;
        public Vector2 zRotation;
        public Vector2 lastZRotation;
        public float wingYAdjust;

        public DynamicSoundLoop flyingBuzzSound;

        public Color WingColor;
        public Color TailColor;
        public Color StripeColor;
        public Color AntennaeColor;
        public Color FluffColor;

        public FAtlas TailAtlas;

        public bool UnlockedExtraStamina = false;
        public bool UnlockedVerticalFlight = false;

        public int WingStaminaMax => UnlockedExtraStamina ? (int)(WingStaminaMaxBase * 1.6f) : WingStaminaMaxBase;
        public float WingStaminaRecovery => UnlockedExtraStamina ? WingStaminaRecoveryBase * 1.2f : WingStaminaRecoveryBase;

        public PlayerEx(Player player)
        {
            IsBee =
                Plugin.WingStamina.TryGet(player, out WingStaminaMaxBase) &&
                Plugin.WingStaminaRecovery.TryGet(player, out WingStaminaRecoveryBase) &&
                Plugin.WingSpeed.TryGet(player, out WingSpeed) &&
                Plugin.BeeCompanion.TryGet(player, out BeeCompanion);

            playerRef = new WeakReference<Player>(player);

            if (!IsBee)
            {
                return;
            }

            if (ExtEnumBase.TryParse(typeof(SlugcatStats.Name), "bee", true, out var extEnum))
            {
                Name = extEnum as SlugcatStats.Name;
            }

            SetupColors(player);
            LoadTailAtlas();

            lastTail = -1;

            flyingBuzzSound = new ChunkDynamicSoundLoop(player.bodyChunks[0]);
            flyingBuzzSound.sound = BeeEnums.BeeBuzz;
            flyingBuzzSound.Pitch = 1f;
            flyingBuzzSound.Volume = 1f;

            wingStamina = WingStaminaMax;
            timeSinceLastFlight = 200;
        }

        ~PlayerEx() {
            try
            {
                TailAtlas.Unload();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void LoadTailAtlas()
        {
            var tailTexture = new Texture2D(Plugin.TailTexture.width, Plugin.TailTexture.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(Plugin.TailTexture, tailTexture);

            MapTextureColor(tailTexture, Color.white, TailColor);
            MapTextureColor(tailTexture, Color.red, StripeColor);

            if (playerRef.TryGetTarget(out var player)) {
                TailAtlas = Futile.atlasManager.LoadAtlasFromTexture("beecattailtexture_" + player.playerState.playerNumber, tailTexture, false);
            }
        }

        private void SetupColors(Player player)
        {
            //-- Loading default colors
            if (SlugBaseCharacter.TryGet(Name, out Character))
            {
                if (Character.Features.TryGet(PlayerFeatures.CustomColors, out var customColors))
                {
                    if (customColors.Length > 2)
                    {
                        WingColor = customColors[2].GetColor(player.playerState.playerNumber);
                    }
                    if (customColors.Length > 3)
                    {
                        TailColor = customColors[3].GetColor(player.playerState.playerNumber);
                    }
                    if (customColors.Length > 4)
                    {
                        StripeColor = customColors[4].GetColor(player.playerState.playerNumber);
                    }
                    if (customColors.Length > 5)
                    {
                        AntennaeColor = customColors[5].GetColor(player.playerState.playerNumber);
                    }
                    if (customColors.Length > 6)
                    {
                        FluffColor = customColors[6].GetColor(player.playerState.playerNumber);
                    }

                    if (WingColor.Equals(Color.clear))
                    {
                        WingColor = Color.white;
                    }
                    if (TailColor.Equals(Color.clear))
                    {
                        if (customColors.Length > 0)
                        {
                            TailColor = customColors[0].GetColor(player.playerState.playerNumber);
                        }
                        else
                        {
                            ColorUtility.TryParseHtmlString("#ffcf0d", out TailColor);
                        }
                    }
                    if (StripeColor.Equals(Color.clear))
                    {
                        if (customColors.Length > 1)
                        {
                            StripeColor = customColors[1].GetColor(player.playerState.playerNumber);
                        }
                        else
                        {
                            ColorUtility.TryParseHtmlString("#010101", out StripeColor);
                        }
                    }
                    if (AntennaeColor.Equals(Color.clear))
                    {
                        if (customColors.Length > 2)
                        {
                            AntennaeColor = customColors[1].GetColor(player.playerState.playerNumber);
                        }
                        else
                        {
                            ColorUtility.TryParseHtmlString("#010101", out AntennaeColor);
                        }
                    }
                    if (FluffColor.Equals(Color.clear))
                    {
                        if (customColors.Length > 2)
                        {
                            FluffColor = customColors[1].GetColor(player.playerState.playerNumber);
                        }
                        else
                        {
                            ColorUtility.TryParseHtmlString("#161c24", out FluffColor);
                        }
                    }
                }
            }

            //-- Loading custom colors if enabled
            if (PlayerGraphics.customColors != null && !player.IsJollyPlayer)
            {
                if (PlayerGraphics.customColors.Count > 2)
                {
                    WingColor = PlayerGraphics.CustomColorSafety(2);
                }
                if (PlayerGraphics.customColors.Count > 3)
                {
                    TailColor = PlayerGraphics.CustomColorSafety(3);
                }
                if (PlayerGraphics.customColors.Count > 4)
                {
                    StripeColor = PlayerGraphics.CustomColorSafety(4);
                }
                if (PlayerGraphics.customColors.Count > 5)
                {
                    AntennaeColor = PlayerGraphics.CustomColorSafety(5);
                }
                if (PlayerGraphics.customColors.Count > 6)
                {
                    FluffColor = PlayerGraphics.CustomColorSafety(6);
                }
            }
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

            return wingStamina > 0 && preventFlight == 0 && player.canJump <= 0 && player.bodyMode != Player.BodyModeIndex.Crawl && player.bodyMode != Player.BodyModeIndex.CorridorClimb && player.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && player.animation != Player.AnimationIndex.HangFromBeam && player.animation != Player.AnimationIndex.ClimbOnBeam && player.bodyMode != Player.BodyModeIndex.WallClimb && player.bodyMode != Player.BodyModeIndex.Swimming && player.Consious && !player.Stunned && player.animation != Player.AnimationIndex.AntlerClimb && player.animation != Player.AnimationIndex.VineGrab && player.animation != Player.AnimationIndex.ZeroGPoleGrab;
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

        public static void MapTextureColor(Texture2D texture, Color from, Color to)
        {
            for (var x = 0; x < texture.width; x++)
            {
                for (var y = 0; y < texture.height; y++)
                {
                    if (texture.GetPixel(x, y) == from)
                    {
                        texture.SetPixel(x, y, to);
                    }
                }
            }
            

            texture.Apply(false);
        }
    }
}