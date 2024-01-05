using BeeWorld.Extensions;

namespace BeeWorld.Hooks;

public static class PlayerGraphicsHooks
{
    public static void Apply()
    {
        On.PlayerGraphics.ctor += PlayerGraphics_ctor;
        On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
        On.PlayerGraphics.Update += PlayerGraphics_Update;
        On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
        On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
    }

    private static readonly Color FullStaminaColor = new Color(1, 0.8f, 0);
    private static readonly Color LowStaminaColor = new Color(0.9f, 0.05f, 0);
    
    private static void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        orig(self, sLeaser, rCam, newContatiner);

        if (!self.player.IsBee(out var bee))
        {
            return;
        }

        if (bee.initialWingSprite > 0 && sLeaser.sprites.Length > bee.initialWingSprite + 5)
        {
            newContatiner ??= rCam.ReturnFContainer("Midground");
            var hud2Container = rCam.ReturnFContainer("HUD2");

            //-- Wings go behind chest
            for (var i = 0; i < 2; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    var sprite = sLeaser.sprites[bee.WingSprite(i, j)];
                    newContatiner.AddChild(sprite);
                    sprite.MoveBehindOtherNode(sLeaser.sprites[0]);
                }
            }

            //-- Tail go behind hips
            sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);
            
            //-- Stinger go behind tail
            newContatiner.AddChild(sLeaser.sprites[bee.stingerSprite]);
            sLeaser.sprites[bee.stingerSprite].MoveBehindOtherNode(sLeaser.sprites[2]);

            //-- Antennae in front of face
            newContatiner.AddChild(sLeaser.sprites[bee.antennaeSprite]);
            sLeaser.sprites[bee.antennaeSprite].MoveBehindOtherNode(sLeaser.sprites[9]);
            
            //-- Floof behind face
            newContatiner.AddChild(sLeaser.sprites[bee.floofSprite]);
            sLeaser.sprites[bee.floofSprite].MoveBehindOtherNode(sLeaser.sprites[9]);

            //-- Stamina HUD
            hud2Container.AddChild(sLeaser.sprites[bee.staminaSprite]);
        }
    }

    private static void PlayerGraphics_ctor(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
    {
        orig(self, ow);

        if (!self.player.IsBee(out var bee))
        {
            return;
        }

        self.bodyPearl = new PlayerGraphics.CosmeticPearl(self, 0);

        bee.RecreateTailIfNeeded(self);

        bee.wingDeployment = new float[2, 2];
        bee.wingDeploymentSpeed = new float[2, 2];
        
        bee.SetupColors(self);
        bee.LoadTailAtlas();
    }

    private static void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        orig(self, sLeaser, rCam);

        if (!self.player.IsBee(out var bee))
        {
            return;
        }

        bee.initialWingSprite = sLeaser.sprites.Length;
        bee.antennaeSprite = bee.initialWingSprite + 4;
        bee.floofSprite = bee.antennaeSprite + 1;
        bee.staminaSprite = bee.floofSprite + 1;
        bee.stingerSprite = bee.staminaSprite + 1;

        Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 8);

        for (var i = 0; i < 2; i++)
        {
            for (var j = 0; j < 2; j++)
            {
                sLeaser.sprites[bee.WingSprite(i, j)] = new FSprite("BeeWing" + (j == 0 ? "A1" : "B2"));
                sLeaser.sprites[bee.WingSprite(i, j)].anchorX = 0f;
                sLeaser.sprites[bee.WingSprite(i, j)].scaleY = 1f;
            }
        }

        sLeaser.sprites[bee.antennaeSprite] = new FSprite("BeeAntennaeHeadA0");
        sLeaser.sprites[bee.floofSprite] = new FSprite("floof2");
        sLeaser.sprites[bee.staminaSprite] = new FSprite("Futile_White")
        {
            scaleX = -5,
            scaleY = 5,
            color = FullStaminaColor,
            alpha = 0,
            shader = Custom.rainWorld.Shaders["BeecatStaminaBar"]
        };
        sLeaser.sprites[bee.stingerSprite] = new FSprite("atlases/beecatstinger")
        {
            scale = 0.4f
        };

        if (sLeaser.sprites[2] is TriangleMesh tail && bee.TailAtlas.elements != null && bee.TailAtlas.elements.Count > 0)
        {
            tail.element = bee.TailAtlas.elements[0];
            for (var i = tail.vertices.Length - 1; i >= 0; i--)
            {
                var perc = i / 2 / (float)(tail.vertices.Length / 2);
                //tail.verticeColors[i] = Color.Lerp(fromColor, toColor, perc);
                Vector2 uv;
                if (i % 2 == 0)
                    uv = new Vector2(perc, 0f);
                else if (i < tail.vertices.Length - 1)
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

        if (!self.player.IsBee(out var bee))
        {
            return;
        }

        if (bee.isFlying)
        {
            bee.wingDeploymentGetTo = 1f;
        }
        else
        {
            bee.wingDeploymentGetTo = 0.9f;
        }

        bee.lastZRotation = bee.zRotation;
        bee.zRotation = Vector2.Lerp(bee.zRotation, Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos), 0.15f);
        bee.zRotation = bee.zRotation.normalized;

        for (var k = 0; k < 2; k++)
        {
            for (var l = 0; l < 2; l++)
            {
                if (self.player.Consious)
                {
                    if (Random.value < 0.033333335f)
                    {
                        //player.wingDeploymentSpeed[k, l] = Random.value * Random.value * 0.3f;
                        bee.wingDeploymentSpeed[k, l] = 0.6f;
                    }
                    else if (bee.wingDeployment[k, l] < bee.wingDeploymentGetTo)
                    {
                        bee.wingDeployment[k, l] = Mathf.Min(bee.wingDeployment[k, l] + bee.wingDeploymentSpeed[k, l], bee.wingDeploymentGetTo);
                    }
                    else if (bee.wingDeployment[k, l] > bee.wingDeploymentGetTo)
                    {
                        bee.wingDeployment[k, l] = Mathf.Max(bee.wingDeployment[k, l] - bee.wingDeploymentSpeed[k, l], bee.wingDeploymentGetTo);
                    }
                }
                else if (bee.wingDeployment[k, l] == 1f)
                {
                    bee.wingDeployment[k, l] = 0.9f;
                }
            }
        }

        bee.wingOffset += 1f / Random.Range(50, 60);
        bee.wingTimeAdd += 1f;
        if (bee.wingTimeAdd >= 3f)
        {
            bee.wingTimeAdd = 0f;
        }
        
        
        //-- Stamina HUD stuff
        bee.staminaLastPos = bee.staminaPos;
        bee.staminaLastFill = bee.staminaFill;
        bee.staminaLastFade = bee.staminaFade;

        bee.staminaPos = self.player.mainBodyChunk.pos;
        bee.staminaFill = bee.wingStamina / bee.WingStaminaMax;

        if (bee.wingStamina < bee.WingStaminaMax)
        {
            bee.staminaFadeCounter = 40;
        }
        else if (bee.staminaFadeCounter > 0)
        {
            bee.staminaFadeCounter--;
        }

        if (bee.staminaFadeCounter > 0)
        {
            bee.staminaFade = Mathf.Min(bee.staminaFade + 1f / 140, 0.4f);
        }
        else
        {
            bee.staminaFade = Mathf.Max(bee.staminaFade - 1f / 140, 0);
        }

        if (Custom.Dist(bee.staminaLastPos, bee.staminaPos) > 20)
        {
            bee.staminaLastPos = bee.staminaPos;
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
        var result = Vector2.zero;

        if (self.player.bodyMode == Player.BodyModeIndex.Stand)
        {
            result.x += self.player.flipDirection * (self.RenderAsPup ? 2f : 6f) * Mathf.Clamp(Mathf.Abs(self.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f) * 0.3f;
            result.y += Mathf.Cos((self.player.animationFrame + 0f) / 6f * 2f * 3.1415927f) * (self.RenderAsPup ? 1.5f : 2f) * 0.3f;
        }
        else if (self.player.bodyMode == Player.BodyModeIndex.Crawl)
        {
            var num4 = Mathf.Sin(self.player.animationFrame / 21f * 2f * 3.1415927f);
            var num5 = Mathf.Cos(self.player.animationFrame / 14f * 2f * 3.1415927f);
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

        if (!self.player.IsBee(out var bee))
        {
            return;
        }

        var animationOffset = GetAnimationOffset(self);

        var wingColor = bee.WingColor;
        var antennaeColor = bee.AntennaeColor;
        var fluffColor = bee.FluffColor;
        var stripeColor = bee.StripeColor;

        sLeaser.sprites[2].color = Color.white;

        //-- Antennae stuff

        var baseHeadName = bee.IsBup ? "HeadC" : "HeadA";
        var headSpriteName = sLeaser.sprites[3].element.name;
        if (!string.IsNullOrWhiteSpace(headSpriteName) && headSpriteName.Contains(baseHeadName))
        {
            var headSpriteNumber = headSpriteName.Substring(headSpriteName.LastIndexOf(baseHeadName, StringComparison.InvariantCultureIgnoreCase)+5);

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

            var antennaePos = new Vector2(sLeaser.sprites[3].x + antennaeOffsetX, sLeaser.sprites[3].y + antennaeOffsetY);

            sLeaser.sprites[bee.antennaeSprite].scaleX = sLeaser.sprites[3].scaleX * 1.3f;
            sLeaser.sprites[bee.antennaeSprite].scaleY = 1.3f;
            sLeaser.sprites[bee.antennaeSprite].rotation = sLeaser.sprites[3].rotation;
            sLeaser.sprites[bee.antennaeSprite].x = antennaePos.x;
            sLeaser.sprites[bee.antennaeSprite].y = antennaePos.y;
            sLeaser.sprites[bee.antennaeSprite].element = Futile.atlasManager.GetElementWithName("BeeAntennaeHeadA" + headSpriteNumber);
            sLeaser.sprites[bee.antennaeSprite].color = antennaeColor;
        }

        //-- Fluff stuff
        var headToBody = (new Vector2(sLeaser.sprites[1].x, sLeaser.sprites[1].y) - new Vector2(sLeaser.sprites[3].x, sLeaser.sprites[3].y)).normalized;
        //var floofPos = Vector2.Lerp(player.lastFloofPos, new Vector2(sLeaser.sprites[3].x + headToBody.x * 4f, sLeaser.sprites[3].y + headToBody.y * 4f), timeStacker * 2);
        var floofPos = new Vector2(sLeaser.sprites[3].x + headToBody.x * 7.5f, sLeaser.sprites[3].y + headToBody.y * 7.5f);

        //sLeaser.sprites[player.floofSprite].scaleX = sLeaser.sprites[3].scaleX;
        sLeaser.sprites[bee.floofSprite].scaleY = 0.75f;
        sLeaser.sprites[bee.floofSprite].rotation = sLeaser.sprites[3].rotation;
        sLeaser.sprites[bee.floofSprite].x = floofPos.x;
        sLeaser.sprites[bee.floofSprite].y = floofPos.y;
        sLeaser.sprites[bee.floofSprite].color = fluffColor;

        if (bee.IsBup)
        {
            sLeaser.sprites[bee.floofSprite].scaleY = 0.55f;
            sLeaser.sprites[bee.floofSprite].scaleX = 0.70f;
        }

        if (bee.wingStamina < bee.LowWingStamina && !self.player.dead)
        {
            sLeaser.sprites[9].element = Futile.atlasManager.GetElementWithName("FaceStunned");
        }

        //player.lastFloofPos = new Vector2(sLeaser.sprites[player.floofSprite].x, sLeaser.sprites[player.floofSprite].y);

        //-- Hand stuff

        for (var i = 5; i <= 8; i++)
        {
            var name = "Beecat" + sLeaser.sprites[i].element.name; 
            if (!name.StartsWith("Beecat") && Futile.atlasManager.DoesContainElementWithName(name))
            {
                sLeaser.sprites[i].element = Futile.atlasManager.GetElementWithName(name);
            }
        }
        
        //-- Stinger stuff
        var tailVertices = ((TriangleMesh)sLeaser.sprites[2]).vertices;
        var tailTip = new[]
        {
            tailVertices[tailVertices.Length - 3],
            tailVertices[tailVertices.Length - 2],
            tailVertices[tailVertices.Length - 1],
        };
        var stingerAngle = (((tailTip[0] + tailTip[1]) / 2) - tailTip[2]).normalized;
        var stingerPos = ((tailTip[0] + tailTip[1] + tailTip[2]) / 3) + stingerAngle * -3;
        sLeaser.sprites[bee.stingerSprite].SetPosition(stingerPos);
        sLeaser.sprites[bee.stingerSprite].rotation = Custom.VecToDeg(stingerAngle);
        sLeaser.sprites[bee.stingerSprite].color = stripeColor;
        sLeaser.sprites[bee.stingerSprite].isVisible = !bee.stingerUsed;

        //-- Stamina HUD stuff
        sLeaser.sprites[bee.staminaSprite].SetPosition(Vector2.Lerp(bee.staminaLastPos, bee.staminaPos, timeStacker) - camPos);
        sLeaser.sprites[bee.staminaSprite].alpha = Mathf.Lerp(bee.staminaLastFade, bee.staminaFade, timeStacker);
        sLeaser.sprites[bee.staminaSprite].isVisible = !bee.IsBup;

        var currentStaminaFill = Mathf.Lerp(bee.staminaLastFill, bee.staminaFill, timeStacker);
        var staminaMeterColor = Color.Lerp(LowStaminaColor, FullStaminaColor, currentStaminaFill);
        //-- Blue channel controls the meter's fullness
        staminaMeterColor.b = currentStaminaFill;
        sLeaser.sprites[bee.staminaSprite].color = staminaMeterColor;

        //-- Wings stuff

        Vector2 vector = Vector3.Slerp(bee.lastZRotation, bee.zRotation, timeStacker);
        var vector2 = Vector2.Lerp(Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker), Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker), bee.IsBup ? 0.1f : 0.5f);
        var normalized = (Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker) - Vector2.Lerp(self.player.bodyChunks[0].lastPos, self.player.bodyChunks[0].pos, timeStacker)).normalized;
        var a = Custom.PerpendicularVector(-normalized);
        var num = Custom.AimFromOneVectorToAnother(-normalized, normalized);

        for (var j = 0; j < 2; j++)
        {
            var num7 = j == 0 ? 5f : 11f;
            var num8 = j == 0 ? -20f : 24f;

            num7 -= 20;
            num8 -= 20;

            num7 += 3f * Mathf.Abs(vector.x);
            for (var k = 0; k < 2; k++)
            {
                var d = (j == 0 ? distanceWingsTop : distanceWingsBottom) * (0.2f + 0.8f * Mathf.Abs(vector.y)) * Mathf.Lerp(1f, 0.85f, Mathf.InverseLerp(0.5f, 0f, bee.wingDeployment[k, j]));
                var vector3 = vector2 + normalized * num7 + a * d * (k == 0 ? -1f : 1f) + a * vector.x * Mathf.Lerp(-3f, -5f, Mathf.InverseLerp(0.5f, 0f, bee.wingDeployment[k, j]));
                sLeaser.sprites[bee.WingSprite(k, j)].x = vector3.x - camPos.x;
                sLeaser.sprites[bee.WingSprite(k, j)].y = vector3.y - camPos.y;

                if (Mathf.Abs(num) < 105)
                {
                    bee.wingYAdjust = Mathf.Lerp(bee.wingYAdjust, m6, 0.05f);
                }
                else
                {
                    bee.wingYAdjust = Mathf.Lerp(bee.wingYAdjust, 0, 0.05f);
                }

                sLeaser.sprites[bee.WingSprite(k, j)].x += animationOffset.x;
                sLeaser.sprites[bee.WingSprite(k, j)].y += bee.wingYAdjust + animationOffset.y;

                sLeaser.sprites[bee.WingSprite(k, j)].alpha = 0.6f;
                sLeaser.sprites[bee.WingSprite(k, j)].color = wingColor;

                if (bee.wingDeployment[k, j] == 1f)
                {
                    float num10;
                    float a2;
                    float b;
                    if (j == 0)
                    {
                        num10 = Mathf.Pow(Custom.Decimal(bee.wingOffset + Mathf.InverseLerp(0f, 3f, bee.wingTimeAdd + timeStacker)), 0.75f);
                        a2 = m1;
                        b = m2;
                    }
                    else
                    {
                        num10 = Mathf.Pow(Custom.Decimal(bee.wingOffset + Mathf.InverseLerp(0f, 3f, bee.wingTimeAdd + timeStacker) + 0.8f), 1.3f);
                        a2 = m3;
                        b = m4;
                    }
                    num10 = Mathf.Pow(0.5f + 0.5f * Mathf.Sin(num10 * 3.1415927f * 2f), 0.7f);
                    num10 = Mathf.Lerp(a2, b, num10);

                    sLeaser.sprites[bee.WingSprite(k, j)].rotation = num - 180f + (num8 + num10) * (k == 0 ? 1f : -1f);
                    sLeaser.sprites[bee.WingSprite(k, j)].scaleX = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(Mathf.Abs(vector.y), 1f, Mathf.Abs(0.5f - num10) * 1.4f)), 1f) * (k == 0 ? 1f : -1f) * wingLength;
                }
                else
                {
                    sLeaser.sprites[bee.WingSprite(k, j)].scaleX = (k == 0 ? 1f : -1f) * wingLength;
                    sLeaser.sprites[bee.WingSprite(k, j)].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(self.player.bodyChunks[1].lastPos, self.player.bodyChunks[1].pos, timeStacker), vector3) - m5 * (k == 0 ? 1f : -1f);
                }
            }
        }
    }
}