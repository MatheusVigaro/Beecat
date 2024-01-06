using BeeWorld.Extensions;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using HUD;
using MoreSlugcats;
using SlugBase.DataTypes;
using UnityEngine.Rendering;

namespace BeeWorld;

public class BupCritob : Critob
{
    public BupCritob() : base(BeeEnums.CreatureType.Bup)
    {
        Icon = new SimpleIcon("Kill_Slugcat", new Color(1, 0.8117647058823529f, 0.050980392156862744f));
        ShelterDanger = ShelterDanger.Safe;
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
        IL.HUD.FoodMeter.ctor += BupsFood;
    }

    private void BupsFood(ILContext il)
    {
        var cursor = new ILCursor(il);

        var loc = 0;
        cursor.GotoNext(MoveType.After,
            i => i.MatchLdloc(out loc),
            i => i.MatchCallOrCallvirt(out _),
            i => i.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
            i => i.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
            i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
            i => i.MatchCallOrCallvirt(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Equality")));

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc, loc);
        cursor.EmitDelegate((FoodMeter self, int i) => ((Player)self.hud.owner).abstractCreature.Room.creatures[i].creatureTemplate.type == BeeEnums.CreatureType.Bup);
        cursor.Emit(OpCodes.Or);
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
                        var dir = Custom.DirVec(self.bodyChunks[1].pos, Creaturenearby.mainBodyChunk.pos);
                        self.Bee().StingerAttack(new Vector2(60, 20) * dir , new Vector2(20, 20) * dir);
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
                    else 
                    {
                        if (self.bodyChunks[0].vel.y <= -10 && self.onBack == null)
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