using BeeWorld;
using SlugBase.SaveData;

namespace Beeworld;

public static class FlowerQuests
{
    public static Quest huh;

    public static void Apply()
    {
        On.Player.Update += Player_Update;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.room != null && self.room.world.game.cameras[0].room != null && !BeeOptions.VanillaType.Value && self.room.world.game.session is StoryGameSession session && ModManager.MSC && self.room.game.StoryCharacter == BeeEnums.Beecat)
        {
            var Data = session.saveState.miscWorldSaveData.GetSlugBaseData();
            if (self.room.updateList.OfType<Quest>().Any()) return;
            switch (self.room.abstractRoom.name.ToLower())
            {
                case "hi_b02":
                    if (Data.TryGet("HI", out bool HI) || HI) break;
                    self.room.AddObject(new Quest(new Vector2(535, 1370), "HI", "Speed"));
                    break;
                case "si_beeflower":
                    if (Data.TryGet("SI", out bool SI) || SI) return;
                    self.room.AddObject(new Quest(new Vector2(344, 516), "SI", "StingDMG"));
                    break;
                case "ss_lab11":
                    if (Data.TryGet("SS", out bool SS) || SS) return;
                    self.room.AddObject(new Quest(new Vector2(264, 154), "SS", "VerticalFly"));
                    break;
                case "su_a40":
                    if (Data.TryGet("SU", out bool SU) || SU) return;
                    self.room.AddObject(new Quest(new Vector2(824, 655), "SU", "StingDMG"));
                    break;
                case "sb_gor02":
                    if (Data.TryGet("SB", out bool SB) || SB) return;
                    self.room.AddObject(new Quest(new Vector2(202, 494), "SB", "StingDMG"));
                    break;
                case "ds_a01":
                    if (Data.TryGet("DS", out bool DS) || DS) return;
                    self.room.AddObject(new Quest(new Vector2(263, 373), "DS", "Speed"));
                    break;
                case "sh_a10":
                    if (Data.TryGet("SH", out bool SH) || SH) return;
                    self.room.AddObject(new Quest(new Vector2(885, 615), "SH", "Speed"));
                    break;
                case "lm_edge02":
                    if (Data.TryGet("LM", out bool LM) || LM) return;
                    self.room.AddObject(new Quest(new Vector2(1643, 1221), "LM", "WingSpeed"));
                    break;
                case "lf_f02":
                    if (Data.TryGet("LF", out bool LF) || LF) return;
                    self.room.AddObject(new Quest(new Vector2(1945, 2034), "LF", "WingSpeed"));
                    break;
                case "cc_sump05":
                    if (Data.TryGet("CC", out bool CC) || CC) return;
                    self.room.AddObject(new Quest(new Vector2(3463, 1162), "CC", "WingSpeed"));
                    break;
                case "gw_tower15":
                    if (Data.TryGet("GW", out bool GW) || GW) return;
                    self.room.AddObject(new Quest(new Vector2(2366, 1075), "GW", "Speed"));
                    break;
                case "uw_j01":
                    if (Data.TryGet("UW", out bool UW) || UW) return;
                    self.room.AddObject(new Quest(new Vector2(683, 1153), "UW", "StingDMG"));
                    break;
                case "dm_u06":
                    if (Data.TryGet("DM", out bool DM) || DM) return;
                    self.room.AddObject(new Quest(new Vector2(3642, 254), "DM", "StingDMG"));
                    break;
                case "z3_a35":
                    if (Data.TryGet("Z3", out bool Z3) || Z3) return;
                    self.room.AddObject(new Quest(new Vector2(626, 398), "Z3", "StingDMG"));
                    break;
                case "z2_a37":
                    if (Data.TryGet("Z2", out bool Z2) || Z2) return;
                    self.room.AddObject(new Quest(new Vector2(2804, 514), "Z2", "WingSpeed"));
                    break;
                case "z4_a09":
                    if (Data.TryGet("Z4", out bool Z4) || Z4) return;
                    self.room.AddObject(new Quest(new Vector2(146, 175), "Z4", "WingSpeed"));
                    break;
                case "z5_b04":
                    if (Data.TryGet("Z5", out bool Z5) || Z5) return;
                    self.room.AddObject(new Quest(new Vector2(884, 192), "Z5", "Speed"));
                    break;
                default:
                    break;
            }
        }
    }
}
public class Quest : CosmeticSprite
{
    private bool collected, wawaF, startup;
    private float wawa, timer, speed, wa;
    private readonly string Region, power;

    public Quest(Vector2 pos, string region, string power)
    {
        this.pos = pos;
        lastPos = pos;
        this.Region = region;
        this.power = power;
    }
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (timer > 5)
        {
            Destroy();
        }
        Player PlayerPL = null;
        foreach (var otherPlayer in room.updateList.OfType<Player>())
        {
            if (Custom.DistLess(pos, otherPlayer.bodyChunks[0].pos, otherPlayer.bodyChunks[0].rad + 40))
            {
                PlayerPL = otherPlayer;
                break;
            }
        }
        speed += 0.1f;

        if (PlayerPL != null && !collected)
        {
            collected = true;
            if (room != null && room.world.game.session is StoryGameSession session)
            {
                var Data = session.saveState.miscWorldSaveData.GetSlugBaseData();
                if (power == "Speed")
                {
                    Data.TryGet("Speed", out int check);
                    Data.Set("Speed", check+1);
                    room.game.cameras[0].hud.textPrompt.AddMessage("Speed Increased!", 0, 100, true, false);
                }
                if (power == "WingSpeed")
                {
                    Data.TryGet("WingSpeed", out int check);
                    Data.Set("WingSpeed", check+1);
                    room.game.cameras[0].hud.textPrompt.AddMessage("WingSpeed Increased!", 0, 100, true, false);
                }
                if (power == "StingDMG")
                {
                    Data.TryGet("StingDMG", out float check);
                    Data.Set("StingDMG", check + 0.2f);
                    room.game.cameras[0].hud.textPrompt.AddMessage("Sting Damage Increased!", 0, 100, true, false);
                }
                if (power == "VerticalFly")
                {
                    Data.Set("VerticalFly", true);
                    room.game.cameras[0].hud.textPrompt.AddMessage("Vertical Fly Unlocked!", 0, 100, true, false);
                }
                Data.Set(Region, true);
            }
        }
        if (wawa > 0 && wawaF)
        {
            wawa -= 0.01f;
        }
        else wawaF = false;
        if (wawa < 1 && !wawaF)
        {
            wawa += 0.01f;
        }
        else wawaF = true;
    }
    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        base.InitiateSprites(sLeaser, rCam);
        sLeaser.sprites = new FSprite[3];
        
        sLeaser.sprites[0] = new FSprite("miscDangerSymbol", true);
        sLeaser.sprites[1] = new FSprite("Futile_White", true)
        {
            shader = rCam.room.game.rainWorld.Shaders["VectorCircle"]
        };
        sLeaser.sprites[2] = new FSprite("Futile_White", true)
        {
            shader = rCam.room.game.rainWorld.Shaders["VectorCircle"],
            isVisible = false
        };

        AddToContainer(sLeaser, rCam, null);
    }
    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        sLeaser.sprites[0].SetPosition(Vector2.Lerp(lastPos, pos, timeStacker) - camPos);
        sLeaser.sprites[1].SetPosition(sLeaser.sprites[0].GetPosition());
        sLeaser.sprites[2].SetPosition(sLeaser.sprites[0].GetPosition());
        sLeaser.sprites[1].color = sLeaser.sprites[0].color;
        float pwa = Mathf.SmoothStep(0, 1, wawa);
        sLeaser.sprites[1].scale = Mathf.Lerp(5.07f, 5.3f, pwa);
        sLeaser.sprites[1].alpha = Mathf.Lerp(0.07f, 0.1f, pwa);

        float frequency = 0.1f; 
        float phaseOffset = 0.0f; 
        float red = Mathf.Sin(frequency * speed + 0 * Mathf.PI / 3 + phaseOffset);
        float green = Mathf.Sin(frequency * speed + 2 * Mathf.PI / 3 + phaseOffset);
        float blue = Mathf.Sin(frequency * speed + 4 * Mathf.PI / 3 + phaseOffset);
        red = (red + 1) / 2;
        green = (green + 1) / 2;
        blue = (blue + 1) / 2;

        sLeaser.sprites[0].color = new Color(red, green, blue);



        if (collected)
        {
            timer += 0.04f;
            if (!startup)
            {
                startup = true;
                room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pos);
                sLeaser.sprites[2].isVisible = true;
                wa = sLeaser.sprites[1].alpha;
            }   
            float easedTimer = Mathf.SmoothStep(0, 1, timer); // Easing out
            sLeaser.sprites[1].alpha = Mathf.Lerp(wa, 0, easedTimer);
            sLeaser.sprites[0].alpha = Mathf.Lerp(1, 0, timer);
            sLeaser.sprites[2].scale = Mathf.Lerp(0, 40, easedTimer);
            sLeaser.sprites[2].alpha = Mathf.Lerp(1, 0, easedTimer);
            sLeaser.sprites[2].color = new Color(red, green, blue);
        }
    }
}