using BeeWorld.Extensions;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using MoreSlugcats;
using SlugBase.DataTypes;
using UnityEngine.Rendering;

namespace BeeWorld;

public class BupCritob : Critob
{
    public BupCritob() : base(BeeEnums.CreatureType.Bup)
    {
        Icon = new SimpleIcon("Kill_Slugcat", new Color(1, 0.8117647058823529f, 0.050980392156862744f));
        LoadedPerformanceCost = 100f;
        SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
        RegisterUnlock(KillScore.Configurable(6), BeeEnums.SandboxUnlockID.Bup);
        BupHook hooks = new BupHook();
        hooks.ApplyMyHooks();
    }

    public override int ExpeditionScore() => 6;

    public override Color DevtoolsMapColor(AbstractCreature acrit) => new(1, 0.8117647058823529f, 0.050980392156862744f);

    public override string DevtoolsMapName(AbstractCreature acrit) => "Bup";

    public override IEnumerable<string> WorldFileAliases() => new[] { "BeePup", "Bup", };

    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction() => new[] { RoomAttractivenessPanel.Category.Lizards, RoomAttractivenessPanel.Category.Swimming, RoomAttractivenessPanel.Category.LikesInside, RoomAttractivenessPanel.Category.LikesWater };

    public override CreatureTemplate.Type ArenaFallback() => MoreSlugcatsEnums.CreatureTemplateType.SlugNPC;

    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit) => new SlugNPCAI(acrit, acrit.world);

    public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit) => new SlugNPCAbstractAI(acrit.world, acrit);

    public override Creature CreateRealizedCreature(AbstractCreature acrit) => new Player(acrit, acrit.world);

    public override CreatureState CreateState(AbstractCreature acrit) => new PlayerNPCState(acrit, 0);

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate cf = new CreatureFormula(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, Type, nameof(BeeEnums.CreatureType.Bup))
        {
            DefaultRelationship = new(CreatureTemplate.Relationship.Type.Eats, 1),
            HasAI = true,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.Slugcat),
        }.IntoTemplate();
        cf.meatPoints = 6;
        cf.visualRadius = 1500;
        cf.waterVision = 0.3f;
        cf.stowFoodInDen = true;
        cf.throughSurfaceVision = 0.5f;
        cf.movementBasedVision = 0.5f;
        cf.communityInfluence = 0.1f;
        cf.bodySize = 1;
        cf.usesCreatureHoles = false;
        cf.BlizzardAdapted = true;
        cf.BlizzardWanderer = true;
        cf.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        cf.lungCapacity = 1600;
        cf.jumpAction = "Swap Heads";
        cf.pickupAction = "Grab/Freeze";
        cf.shortcutSegments = 1;
        return cf;
    }

    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);
        s.Rivals(CreatureTemplate.Type.LizardTemplate, .1f);
        s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, .5f);
    }
}

public class BupHook
{
    public void ApplyMyHooks()
    {
        On.Player.Update += BupsAI;
    }
    private void BupsAI(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.isNPC)
        {
            if (self.Template.type.value == "Bup")
            {

                Creature Creaturenearby = null;
                if(self.room != null)
                {
                    foreach (var otherPlayers in self.room.updateList.OfType<Creature>())
                    {
                        if (otherPlayers is not Player && !otherPlayers.Template.smallCreature && Custom.DistLess(self.bodyChunks[0].pos, otherPlayers.bodyChunks[0].pos, otherPlayers.bodyChunks[0].rad + 20))
                        {
                            Creaturenearby = otherPlayers;
                            break;
                        }
                    }
                    if (Creaturenearby != null && Creaturenearby.graphicsModule != null && !Creaturenearby.dead && (self.Consious || (self.dangerGraspTime < 200 && !self.dead)) && !self.Bee().stingerUsed && self.Bee().stingerAttackCooldown <= 0)
                    {
                        self.Bee().StingerAttack(new Vector2(60 * self.flipDirection, 20), new Vector2(20 * self.flipDirection, 20));
                    }



                        

                    // -- fly
                    if (self.Bee().isFlying)
                    {
                        for (int i = 0; i <= 100; i += 10)
                        {
                            if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x, self.bodyChunks[0].pos.y - i)).Terrain == Room.Tile.TerrainType.Solid || self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x, self.bodyChunks[0].pos.y - i)).Terrain == Room.Tile.TerrainType.Floor)
                            {
                                self.Bee().StopFlight();
                                return;
                            }
                        }

                        Player nearbyBee = null;
                        foreach (var otherPlayer in self.room.updateList.OfType<Player>())
                        {
                            if (!otherPlayer.isNPC && Custom.DistLess(self.bodyChunks[0].pos, otherPlayer.bodyChunks[0].pos, otherPlayer.bodyChunks[0].rad + 100))
                            {
                                nearbyBee = otherPlayer;
                                break;
                            }
                        }
                        if (nearbyBee != null && nearbyBee.graphicsModule != null)
                        {
                            float direction = Mathf.Sign(nearbyBee.bodyChunks[0].pos.x - self.bodyChunks[0].pos.x);

                            // Adjust the velocity based on the direction
                            self.bodyChunks[0].vel.x = direction;
                            self.bodyChunks[1].vel.x = direction;
                        }
                        else
                        {
                            if (self.Bee().CDMET)
                            {
                                self.bodyChunks[0].vel.x += Mathf.Sign(self.Bee().polepos.x);
                            }
                            else
                            {
                                for (int i = 0; i <= 100; i += 10)
                                {
                                    if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x - i, self.bodyChunks[0].pos.y)).AnyBeam)
                                    {   
                                        self.Bee().polepos = new Vector2(-i, 0);
                                        self.Bee().CDMET = true;
                                        return; 
                                    }
                                }

                                // If the condition is not met in the negative direction
                                if (!self.Bee().CDMET)
                                {
                                    for (int i = 0; i <= 100; i += 10)
                                    {   
                                        if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x + i, self.bodyChunks[0].pos.y)).AnyBeam)
                                        {
                                            self.Bee().polepos = new Vector2(i, 0);
                                            self.Bee().CDMET = true;
                                            return; 
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else 
                    {
                        if (self.bodyChunks[0].vel.y <= -10)
                        {
                            self.Bee().isFlying = true;
                        }
                        if (self.input[0].jmp)
                        {
                            self.Bee().isFlying = !self.Bee().isFlying;
                        }
                    }

                }
            }
        }
    }
}