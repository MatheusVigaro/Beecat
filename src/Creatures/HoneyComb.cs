using Fisobs.Items;
using Fisobs.Core;
using Fisobs.Sandbox;
using Fisobs.Properties;
using BeeWorld;
using BeeWorld.Extensions;

namespace wa;

public class HoneyCombFisob : Fisob
{
    public static readonly AbstractPhysicalObject.AbstractObjectType HoneyComb = new("WA", true);

    public static readonly MultiplayerUnlocks.SandboxUnlockID HComb = new("Hcomb", true);
    public HoneyCombFisob() : base(HoneyComb)
    {
        Icon = new SimpleIcon("SkyDandelion", Color.cyan);

        SandboxPerformanceCost = new(linear: 0.2f, 0f);

        RegisterUnlock(HComb, parent: MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
        HoneyFood hooks = new HoneyFood();
        hooks.ApplyMyHooks();
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        string[] p = entitySaveData.CustomData.Split(';');
        if (p.Length < 8)
        {
            p = new string[8];
        }

        var result = new AbstractHoneyComb(world, entitySaveData.Pos, entitySaveData.ID)
        {
            hue = float.TryParse(p[0], out var h) ? h : 0,
            saturation = float.TryParse(p[1], out var s) ? s : 1,
            scaleX = float.TryParse(p[2], out var x) ? x : 1,
            scaleY = float.TryParse(p[3], out var y) ? y : 1,
        };

        if (unlock is SandboxUnlock u)
        {
            result.hue = u.Data / 1000f;

            if (u.Data == 0)
            {
                result.scaleX += 0.2f;
                result.scaleY += 0.2f;
            }
        }
        return result;
    }
    private static readonly HunProperties properties = new();

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
        // The Mosquitoes example demonstrates this.
        return properties;
    }
}

public class HunProperties : ItemProperties
{
    public override void Throwable(Player player, ref bool throwable)
    {
        throwable = true;
    }

    public override void ScavCollectScore(Scavenger scav, ref int score)
    {
        score = 5;
    }

    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.OneHand;
    }
}

public class AbstractHoneyComb : AbstractPhysicalObject
{
    public static AbstractObjectType HoneyFlake; //shhhh

    public float hue;
    public float saturation;
    public float scaleX;
    public float scaleY;
    public AbstractHoneyComb(World world, WorldCoordinate pos, EntityID ID) : base(world, HoneyFlake, null, pos, ID)
    {
        scaleX = 1;
        scaleY = 1;
        saturation = 0.5f;
        type = HoneyCombFisob.HoneyComb;
        hue = 1f;
    }
    public override string ToString()
    {
        return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY}");
    }
    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null)
        {
            realizedObject = new HoneyCombT(this);
        }

    }
}

public class HoneyCombT : PlayerCarryableItem, IDrawable, IPlayerEdible
{
    public HoneyCombT(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
    {
        var positions = new List<Vector2>();
        bodyChunks = new BodyChunk[positions.Count];
        bodyChunks = new BodyChunk[1];
        bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
        bodyChunkConnections = new BodyChunkConnection[0];

        airFriction = 0.999f;
        gravity = 0.9f;
        bounce = 0.2f;
        surfaceFriction = 0.7f;
        collisionLayer = 2;
        waterFriction = 0.95f;
        buoyancy = 0.9f;
    }
    public int bites = 3;

    public int BitesLeft => bites;

    public int FoodPoints => 2;

    public bool Edible => true;

    public bool AutomaticPickUp => true;

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Items");
        }
        for (var i = 0; i < sLeaser.sprites.Length; i++)
        {
            sLeaser.sprites[i].RemoveFromContainer();
        }
        newContatiner.AddChild(sLeaser.sprites[0]);
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
    }

    public void BitByPlayer(Creature.Grasp grasp, bool eu)
    {
        bites--;
        room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Dangle_Fruit : SoundID.Slugcat_Bite_Dangle_Fruit, base.firstChunk.pos);
        base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
        if (bites < 1)
        {
            (grasp.grabber as Player).ObjectEaten(this);
            grasp.Release();
            Destroy();
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);
        sLeaser.sprites[0].x = pos.x - camPos.x;
        sLeaser.sprites[0].y = pos.y - camPos.y;
        Vector2 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
        sLeaser.sprites[0].rotation = Custom.VecToDeg(v);
        sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("HC" + bites);
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
        }
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("HC1", true)
        {
            scale = 1.5f // how tf slugcat eat honeycomb after beecat spit out honeycomb few minutes ago-
        };
        AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Items"));
    }

    public void ThrowByPlayer()
    {
    }

    public Vector2 lastRotation;
    public Vector2? setRotation;
    public Vector2 rotation;
    public override void Update(bool eu)
    {
        base.Update(eu);
        if (room.game.devToolsActive && Input.GetKey("b"))
        {
            base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Futile.mousePosition) * 3f;
        }
        lastRotation = rotation;
        if (grabbedBy.Count > 0)
        {
            rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
            rotation.y = Mathf.Abs(rotation.y);
        }
        if (setRotation != null)
        {
            rotation = setRotation.Value;
            setRotation = null;
        }
        if (base.firstChunk.ContactPoint.y < 0)
        {
            rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x).normalized;
            BodyChunk firstChunk = base.firstChunk;
            firstChunk.vel.x *= 0.8f;
        }
    }
}
public class HoneyFood
{
    public void ApplyMyHooks()
    {
        On.SlugcatStats.NourishmentOfObjectEaten += SlugcatStats_NourishmentOfObjectEaten;
        On.Player.ObjectEaten += Player_ObjectEaten;
        On.Player.Update += Player_Update;
        On.Player.Jump += Player_Jump;
    }

    private void Player_Jump(On.Player.orig_Jump orig, Player self)
    {
        orig(self);
        var wa = self.Bee();
        if (wa.Adrenaline > 1)
        {
            self.jumpBoost += 2;
        }
    }

    private void Player_Update(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        var wa = self.Bee();
        if (wa.Adrenaline > 1)
        {
            wa.Adrenaline -= 0.01f;
            float sped = Mathf.Clamp(wa.Adrenaline/20, wa.CurrentSpeed, 50);
            self.dynamicRunSpeed[1] = sped;
            if (!self.IsBee(out var bee)) return;
            if (bee.isFlying)
            {
                bee.wingStamina = Mathf.Clamp(bee.wingStamina++, 0, bee.WingStaminaMax);
            }
            
        }

    }

    private void Player_ObjectEaten(On.Player.orig_ObjectEaten orig, Player self, IPlayerEdible edible)
    {
        orig(self, edible);
        if (edible is HoneyCombT)
        {
            var wa = self.Bee();
            wa.Adrenaline = 20;
            if (wa.Adrenaline > 1)
            {
                wa.CurrentSpeed = self.dynamicRunSpeed[1];
            }
            if (!self.IsBee(out var bee)) return;
            bee.wingStamina = bee.WingStaminaMax;
        }
    }

    private int SlugcatStats_NourishmentOfObjectEaten(On.SlugcatStats.orig_NourishmentOfObjectEaten orig, SlugcatStats.Name slugcatIndex, IPlayerEdible eatenobject)
    {
        var result = orig(slugcatIndex, eatenobject);
        if(eatenobject is HoneyCombT && slugcatIndex == BeeEnums.SnowFlake)
        {
            return 20;
        }   
        return result;
    }
}