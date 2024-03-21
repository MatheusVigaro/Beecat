namespace SnowyWorld;
public static class FlowerQuest
{
    private static bool GWBF;

    public static void Apply()
    {
        On.Player.Update += Player_Update;
    }

    private static void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (!GWBF && self.room.world.game.cameras[0].room != null && self.room.world.game.cameras[0].room.abstractRoom.name == "beeflowergw" || self.room.world.game.cameras[0].room.abstractRoom.name == "BeeFlowerGW")
        {
            GWBF = true;
            self.room.AddObject(new Quest(new Vector2(439f, 522), "[Walk Speed Increased]"));
        }
        /*if (self.input[0].pckp && !self.input[1].pckp)
        {
            self.room.AddObject(new Quest(new Vector2(self.firstChunk.pos.x+ 50, self.firstChunk.pos.y), "[Walk Speed Increased]"));
        }*/
    }

    
}
public class Quest : CosmeticSprite
{
    private bool collected, wawaF, startup;
    private float wawa, timer, speed, wa;
    private readonly string Message;

    public Quest(Vector2 pos, string Message)
    {
        this.pos = pos;
        lastPos = pos;
        this.Message = Message;
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
        //Debug.Log("IM ON");

        if (PlayerPL != null && !collected)
        {
            collected = true;
            room.game.cameras[0].hud.textPrompt.AddMessage(Message, 0, 100, true, false);
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