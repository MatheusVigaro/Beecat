using Mono.Cecil.Cil;
using MonoMod.Cil;
using RWCustom;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace BeeWorld.Hooks
{
    public class PlayerGraphicsHooks
    {
        public static void Init()
        {
            On.PlayerGraphics.ctor += PlayerGraphics_ctor;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;

            IL.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites1;
        }

        #region ILHooks

        private static void PlayerGraphics_InitiateSprites1(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdstr("Futile_White"),
                                                     i => i.MatchLdloc(0)))
            {
                return;
            }

            cursor.MoveAfterLabels();

            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4_1);

        }
        #endregion

        #region PlayerGraphics

        private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsBee)
            {
                return;
            }

            if (player.initialWingSprite > 0 && sLeaser.sprites.Length > player.initialWingSprite + 5)
            {
                var foregroundContainer = rCam.ReturnFContainer("Foreground");
                var midgroundContainer = rCam.ReturnFContainer("Midground");

                //-- Wings go behind chest
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var sprite = sLeaser.sprites[player.WingSprite(i, j)];
                        foregroundContainer.RemoveChild(sprite);
                        midgroundContainer.AddChild(sprite);
                        sprite.MoveBehindOtherNode(sLeaser.sprites[0]);
                    }
                }

                //-- Tail go behind hips
                sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);

                foregroundContainer.RemoveChild(sLeaser.sprites[player.antennaeSprite]);
                midgroundContainer.AddChild(sLeaser.sprites[player.antennaeSprite]);
                foregroundContainer.RemoveChild(sLeaser.sprites[player.floofSprite]);
                midgroundContainer.AddChild(sLeaser.sprites[player.floofSprite]);
            }
        }

        private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsBee)
            {
                return;
            }

            player.RecreateTailIfNeeded(self);

            player.wingDeployment = new float[2, 2];
            player.wingDeploymentSpeed = new float[2, 2];
        }

        private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsBee)
            {
                return;
            }

            player.initialWingSprite = sLeaser.sprites.Length;
            player.antennaeSprite = player.initialWingSprite + 4;
            player.floofSprite = player.antennaeSprite + 1;

            Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 6);

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    sLeaser.sprites[player.WingSprite(i, j)] = new FSprite("BeeWing" + (j == 0 ? "A1" : "B2"), true);
                    sLeaser.sprites[player.WingSprite(i, j)].anchorX = 0f;
                    sLeaser.sprites[player.WingSprite(i, j)].scaleY = 1f;
                    //sLeaser.sprites[player.WingSprite(i, j)].shader = rCam.room.game.rainWorld.Shaders["CicadaWing"];
                }
            }

            sLeaser.sprites[player.antennaeSprite] = new FSprite("BeeAntennaeHeadA0", true);
            sLeaser.sprites[player.floofSprite] = new FSprite("floof2", true);

            if (sLeaser.sprites[2] is TriangleMesh tail && player.TailAtlas.elements != null && player.TailAtlas.elements.Count > 0)
            {
                //tail.element = Futile.atlasManager.GetElementWithName("beecattail");
                tail.element = player.TailAtlas.elements[0];
                for (int i = tail.verticeColors.Length - 1; i >= 0; i--)
                {
                    float perc = i / 2 / (float)(tail.verticeColors.Length / 2);
                    //tail.verticeColors[i] = Color.Lerp(fromColor, toColor, perc);
                    Vector2 uv;
                    if (i % 2 == 0)
                        uv = new Vector2(perc, 0f);
                    else if (i < tail.verticeColors.Length - 1)
                        uv = new Vector2(perc, 1f);
                    else
                        uv = new Vector2(1f, 0f);

                    // Map UV values to the element
                    uv.x = Mathf.Lerp(tail.element.uvBottomLeft.x, tail.element.uvTopRight.x, uv.x);
                    uv.y = Mathf.Lerp(tail.element.uvBottomLeft.y, tail.element.uvTopRight.y, uv.y);

                    tail.UVvertices[i] = uv;
                }
            }

            self.AddToContainer(sLeaser, rCam, null);
        }


        private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsBee)
            {
                return;
            }

            if (player.isFlying)
            {
                player.wingDeploymentGetTo = 1f;
            }
            else
            {
                player.wingDeploymentGetTo = 0.9f;
            }

            player.lastZRotation = player.zRotation;
            player.zRotation = Vector2.Lerp(player.zRotation, Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos), 0.15f);
            player.zRotation = player.zRotation.normalized;

            for (int k = 0; k < 2; k++)
            {
                for (int l = 0; l < 2; l++)
                {
                    if (self.player.Consious)
                    {
                        if (Random.value < 0.033333335f)
                        {
                            //player.wingDeploymentSpeed[k, l] = Random.value * Random.value * 0.3f;
                            player.wingDeploymentSpeed[k, l] = 0.6f;
                        }
                        else if (player.wingDeployment[k, l] < player.wingDeploymentGetTo)
                        {
                            player.wingDeployment[k, l] = Mathf.Min(player.wingDeployment[k, l] + player.wingDeploymentSpeed[k, l], player.wingDeploymentGetTo);
                        }
                        else if (player.wingDeployment[k, l] > player.wingDeploymentGetTo)
                        {
                            player.wingDeployment[k, l] = Mathf.Max(player.wingDeployment[k, l] - player.wingDeploymentSpeed[k, l], player.wingDeploymentGetTo);
                        }
                    }
                    else if (player.wingDeployment[k, l] == 1f)
                    {
                        player.wingDeployment[k, l] = 0.9f;
                    }
                }
            }

            player.wingOffset += 1f / Random.Range(50, 60);
            player.wingTimeAdd += 1f;
            if (player.wingTimeAdd >= 3f)
            {
                player.wingTimeAdd = 0f;
            }
        }


        static float wingLength = 0.75f;

        //-- Wing rotation
        static float m5 = -70f;

        //-- Vertical adjustment when crouching
        static float m6 = 8f;

        //-- Distance between wings
        static float distanceWingsTop = 5f;
        static float distanceWingsBottom = 3f;

        //-- top wing rotation min/max when flying
        static float m1 = 20f;
        static float m2 = 120f;

        //-- bottom wing rotation min/max when flying
        static float m3 = -45f;
        static float m4 = 75f;

        public static Vector2 GetAnimationOffset(PlayerGraphics self)
        {
            Vector2 result = Vector2.zero;

            if (self.player.bodyMode == Player.BodyModeIndex.Stand)
            {
                result.x += self.player.flipDirection * (self.RenderAsPup ? 2f : 6f) * Mathf.Clamp(Mathf.Abs(self.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f) * 0.3f;
                result.y += Mathf.Cos((self.player.animationFrame + 0f) / 6f * 2f * 3.1415927f) * (self.RenderAsPup ? 1.5f : 2f) * 0.3f;
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.Crawl)
            {
                float num4 = Mathf.Sin(self.player.animationFrame / 21f * 2f * 3.1415927f);
                float num5 = Mathf.Cos(self.player.animationFrame / 14f * 2f * 3.1415927f);
                result.x += num5 * self.player.flipDirection * 2f;
                result.y -= num4 * -1.5f - 3f;
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam)
            {
                if (self.player.animation == Player.AnimationIndex.ClimbOnBeam)
                {
                    result.x += self.player.flipDirection * 2.5f + self.player.flipDirection * 0.5f * Mathf.Sin(self.player.animationFrame / 20f * 3.1415927f * 2f);
                }
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.WallClimb)
            {
                result.y += 2f;
                result.x -= self.player.flipDirection * (self.owner.bodyChunks[1].ContactPoint.y < 0 ? 3f : 5f);
            }
            else if (self.player.bodyMode == Player.BodyModeIndex.Default)
            {
                if (self.player.animation == Player.AnimationIndex.LedgeGrab)
                {
                    result.x -= self.player.flipDirection * 5f;
                }
            }
            else if (self.player.animation == Player.AnimationIndex.CorridorTurn)
            {
                result += Custom.DegToVec(Random.value * 360f) * 3f * Random.value;
            }

            return result;
        }

        private static void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!PlayerHooks.PlayerData.TryGetValue(self.player, out var player) || !player.IsBee)
            {
                return;
            }

            var animationOffset = GetAnimationOffset(self);

            var wingColor = player.WingColor.Equals(Color.clear) ? Color.white : player.WingColor;
            var tailColor = player.TailColor.Equals(Color.clear) ? sLeaser.sprites[0].color : player.TailColor;
            var stripeColor = player.StripeColor.Equals(Color.clear) ? sLeaser.sprites[9].color : player.StripeColor;
            var antennaeColor = player.AntennaeColor.Equals(Color.clear) ? sLeaser.sprites[9].color : player.AntennaeColor;
            var fluffColor = player.FluffColor.Equals(Color.clear) ? Color.Lerp(sLeaser.sprites[9].color, Color.white, 0.2f) : player.FluffColor;

            sLeaser.sprites[2].color = Color.white;

            //-- Tail stuff (Replaced with a UV map, see InitiateSprites
            /*
            if (sLeaser.sprites[2] is TriangleMesh tailMesh)
            {
                var black = stripeColor;
                var yellow = tailColor;
                var useBlack = false;

                var lastIndex = 0;

                for (var i = 0; i < tailMesh.verticeColors.Length; i += 2)
                {
                    tailMesh.verticeColors[i] = useBlack ? black : yellow;
                    lastIndex = i;
                    if (i + 1 < tailMesh.verticeColors.Length)
                    {
                        tailMesh.verticeColors[i + 1] = useBlack ? black : yellow;
                        lastIndex++;
                    }
                    useBlack = !useBlack;
                }

                for (var i = lastIndex + 1; i < tailMesh.verticeColors.Length; i++)
                {
                    tailMesh.verticeColors[i] = useBlack ? black : yellow;
                }
            }*/


            //-- Antennae stuff
            
            var headSpriteName = sLeaser.sprites[3].element.name;
            if (!string.IsNullOrWhiteSpace(headSpriteName) && headSpriteName.StartsWith("HeadA"))
            {
                var headSpriteNumber = headSpriteName.Substring(5);

                var antennaeOffsetX = 0f;
                var antennaeOffsetY = 0f;
                switch (headSpriteNumber)
                {
                    case "0":
                    case "1":
                    case "2":
                    case "3":
                        antennaeOffsetY = 2f;
                        break;
                    case "5":
                    case "6":
                        antennaeOffsetX = -1.5f * Math.Sign(sLeaser.sprites[3].scaleX);
                        break;
                    case "7":
                        antennaeOffsetY = -3.5f;
                        break;
                }

                //var antennaePos = Vector2.Lerp(player.lastAntennaePos, new Vector2(sLeaser.sprites[3].x + antennaeOffsetX, sLeaser.sprites[3].y + antennaeOffsetY), timeStacker * 2);
                var antennaePos = new Vector2(sLeaser.sprites[3].x + antennaeOffsetX, sLeaser.sprites[3].y + antennaeOffsetY);

                sLeaser.sprites[player.antennaeSprite].scaleX = sLeaser.sprites[3].scaleX * 1.3f;
                sLeaser.sprites[player.antennaeSprite].scaleY = 1.3f;
                sLeaser.sprites[player.antennaeSprite].rotation = sLeaser.sprites[3].rotation;
                sLeaser.sprites[player.antennaeSprite].x = antennaePos.x;
                sLeaser.sprites[player.antennaeSprite].y = antennaePos.y;
                sLeaser.sprites[player.antennaeSprite].element = Futile.atlasManager.GetElementWithName("BeeAntennae" + headSpriteName);
                sLeaser.sprites[player.antennaeSprite].color = antennaeColor;

                player.lastAntennaePos = new Vector2(sLeaser.sprites[player.antennaeSprite].x, sLeaser.sprites[player.antennaeSprite].y);
            }

            //-- Fluff stuff
            var headToBody = (new Vector2(sLeaser.sprites[1].x, sLeaser.sprites[1].y) - new Vector2(sLeaser.sprites[3].x, sLeaser.sprites[3].y)).normalized;
            //var floofPos = Vector2.Lerp(player.lastFloofPos, new Vector2(sLeaser.sprites[3].x + headToBody.x * 4f, sLeaser.sprites[3].y + headToBody.y * 4f), timeStacker * 2);
            var floofPos = new Vector2(sLeaser.sprites[3].x + headToBody.x * 7.5f, sLeaser.sprites[3].y + headToBody.y * 7.5f);

            sLeaser.sprites[player.floofSprite].scaleX = sLeaser.sprites[3].scaleX;
            sLeaser.sprites[player.floofSprite].scaleY = 0.75f;
            sLeaser.sprites[player.floofSprite].rotation = sLeaser.sprites[3].rotation;
            sLeaser.sprites[player.floofSprite].x = floofPos.x;
            sLeaser.sprites[player.floofSprite].y = floofPos.y;
            sLeaser.sprites[player.floofSprite].color = fluffColor;

            if (player.wingStamina < player.MinimumFlightStamina * 3 && !self.player.dead)
            {
                sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
            }

            //player.lastFloofPos = new Vector2(sLeaser.sprites[player.floofSprite].x, sLeaser.sprites[player.floofSprite].y);

            //-- Hand stuff

            for (var i = 5; i <= 8; i++)
            {
                if (!sLeaser.sprites[i].element.name.StartsWith("Beecat"))
                {
                    sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName("Beecat" + sLeaser.sprites[i].element.name);
                }
            }

            //-- Wings stuff

            Vector2 vector = Vector3.Slerp(player.lastZRotation, player.zRotation, timeStacker);
            Vector2 vector2 = Vector2.Lerp(Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker), Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker), 0.5f);
            Vector2 normalized = (Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker) - Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker)).normalized;
            Vector2 a = Custom.PerpendicularVector(-normalized);
            float num = Custom.AimFromOneVectorToAnother(-normalized, normalized);

            for (int j = 0; j < 2; j++)
            {
                float num7 = j == 0 ? 5f : 11f;
                float num8 = j == 0 ? -20f : 24f;

                num7 -= 20;
                num8 -= 20;

                num7 += 3f * Mathf.Abs(vector.x);
                for (int k = 0; k < 2; k++)
                {
                    //float d = ((j == 0) ? 11f : 9f) * (0.2f + 0.8f * Mathf.Abs(vector.y)) * Mathf.Lerp(1f, 0.85f, Mathf.InverseLerp(0.5f, 0f, player.wingDeployment[k, j]));
                    float d = (j == 0 ? distanceWingsTop : distanceWingsBottom) * (0.2f + 0.8f * Mathf.Abs(vector.y)) * Mathf.Lerp(1f, 0.85f, Mathf.InverseLerp(0.5f, 0f, player.wingDeployment[k, j]));
                    Vector2 vector3 = vector2 + normalized * num7 + a * d * (k == 0 ? -1f : 1f) + a * vector.x * Mathf.Lerp(-3f, -5f, Mathf.InverseLerp(0.5f, 0f, player.wingDeployment[k, j]));
                    sLeaser.sprites[player.WingSprite(k, j)].x = vector3.x - camPos.x;
                    sLeaser.sprites[player.WingSprite(k, j)].y = vector3.y - camPos.y;

                    if (Mathf.Abs(num) < 105)
                    {
                        player.wingYAdjust = Mathf.Lerp(player.wingYAdjust, m6, 0.05f);
                    }
                    else
                    {
                        player.wingYAdjust = Mathf.Lerp(player.wingYAdjust, 0, 0.05f);
                    }

                    sLeaser.sprites[player.WingSprite(k, j)].x += animationOffset.x;
                    sLeaser.sprites[player.WingSprite(k, j)].y += player.wingYAdjust + animationOffset.y;

                    sLeaser.sprites[player.WingSprite(k, j)].alpha = 0.6f;
                    sLeaser.sprites[player.WingSprite(k, j)].color = wingColor;

                    if (player.wingDeployment[k, j] == 1f)
                    {
                        float num10;
                        float a2;
                        float b;
                        if (j == 0)
                        {
                            num10 = Mathf.Pow(Custom.Decimal(player.wingOffset + Mathf.InverseLerp(0f, 3f, player.wingTimeAdd + timeStacker)), 0.75f);
                            a2 = m1;
                            b = m2;
                        }
                        else
                        {
                            num10 = Mathf.Pow(Custom.Decimal(player.wingOffset + Mathf.InverseLerp(0f, 3f, player.wingTimeAdd + timeStacker) + 0.8f), 1.3f);
                            a2 = m3;
                            b = m4;
                        }
                        num10 = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(num10 * 3.1415927f * 2f), 0.7f);
                        num10 = Mathf.Lerp(a2, b, num10);

                        sLeaser.sprites[player.WingSprite(k, j)].rotation = num - 180f + (num8 + num10) * (k == 0 ? 1f : -1f);
                        sLeaser.sprites[player.WingSprite(k, j)].scaleX = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(Mathf.Abs(vector.y), 1f, Mathf.Abs(0.5f - num10) * 1.4f)), 1f) * (k == 0 ? 1f : -1f) * wingLength;
                    }
                    else
                    {
                        sLeaser.sprites[player.WingSprite(k, j)].scaleX = (k == 0 ? 1f : -1f) * wingLength;
                        sLeaser.sprites[player.WingSprite(k, j)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker), vector3) - m5 * (k == 0 ? 1f : -1f);
                    }
                }
            }
        }

        #endregion
    }
}