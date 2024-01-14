using BeeWorld.Extensions;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using HUD;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using VoidSea;

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
        BupHook.Apply();
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

public static class BupHook
{
    public static void Apply()
    {
        On.Player.Update += BupsAI;
        IL.HUD.FoodMeter.ctor += BupsFood;
        IL.VoidSea.VoidSeaScene.Update += VoidSeaScene_Update;
        IL.GhostCreatureSedater.Update += GhostCreatureSedater_Update;
        On.OracleBehavior.CheckSlugpupsInRoom += OracleBehavior_CheckSlugpupsInRoom;
        IL.OracleBehavior.CheckStrayCreatureInRoom += OracleBehavior_CheckStrayCreatureInRoom;
        On.Player.SlugSlamConditions += Player_SlugSlamConditions;
        On.SaveState.SessionEnded += SaveState_SessionEnded;
        IL.ShelterDoor.Update += ShelterDoor_Update;
        IL.World.SpawnPupNPCs += World_SpawnPupNPCs;
        _ = new Hook(typeof(StoryGameSession).GetProperty(nameof(StoryGameSession.slugPupMaxCount))!.GetGetMethod(), StoryGameSession_slugPupMaxCount_get);
    }

    private static int StoryGameSession_slugPupMaxCount_get(Func<StoryGameSession, int> orig, StoryGameSession self)
    {
        return Math.Max(orig(self), self.saveStateNumber == BeeEnums.Beecat ? 2 : 0);
    }

    private static void World_SpawnPupNPCs(ILContext il)
    {
        var cursor = new ILCursor(il);

        var loc = -1;
        cursor.GotoNext(MoveType.After,
            i => i.MatchLdloc(out loc),
            i => i.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
            i => i.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
            i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
            i => i.MatchCallOrCallvirt(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Equality")));

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldloc, loc);
        cursor.EmitDelegate((AbstractCreature crit) => crit.creatureTemplate.type == BeeEnums.CreatureType.Bup);
        cursor.Emit(OpCodes.Or);

        cursor.GotoNext(MoveType.After, i => i.MatchCallOrCallvirt(typeof(StaticWorld).GetMethod(nameof(StaticWorld.GetCreatureTemplate), [typeof(CreatureTemplate.Type)])));

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((CreatureTemplate old, World self) => self.game.session is StoryGameSession session && session.saveStateNumber == BeeEnums.Beecat ? StaticWorld.GetCreatureTemplate(BeeEnums.CreatureType.Bup) : old);
    }

    private static void ShelterDoor_Update(ILContext il)
    {
        var cursor = new ILCursor(il);

        var loc = -1;
        cursor.GotoNext(MoveType.After,
            i => i.MatchLdloc(out loc),
            i => i.MatchCallOrCallvirt(out _),
            i => i.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
            i => i.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
            i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
            i => i.MatchCallOrCallvirt(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Inequality")));

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc, loc);
        cursor.EmitDelegate((ShelterDoor self, int i) => self.room.abstractRoom.creatures[i].creatureTemplate.type != BeeEnums.CreatureType.Bup);
        cursor.Emit(OpCodes.And);
    }

    private static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
    {
		for (var j = 0; j < game.GetStorySession.playerSessionRecords.Length; j++)
		{
			if (game.GetStorySession.playerSessionRecords[j] == null || (ModManager.CoopAvailable && game.world.GetAbstractRoom(game.Players[j].pos) == null))
			{
				continue;
			}
			game.GetStorySession.playerSessionRecords[j].pupCountInDen = 0;
			var flag = false;
			game.GetStorySession.playerSessionRecords[j].wentToSleepInRegion = game.world.region.name;
			foreach (var crit in game.world.GetAbstractRoom(game.Players[j].pos).creatures)
            {
                if (!crit.state.alive || crit.state.socialMemory == null || crit.realizedCreature == null || crit.abstractAI?.RealAI?.friendTracker?.friend == null || crit.abstractAI.RealAI.friendTracker.friend != game.Players[j].realizedCreature || !(crit.state.socialMemory.GetLike(game.Players[j].ID) > 0f))
                {
                    continue;
                }
                if (ModManager.MSC && crit.creatureTemplate.type == BeeEnums.CreatureType.Bup && crit.state is PlayerNPCState state)
                {
                    if (state.foodInStomach - (state.Malnourished ? SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).x : SlugcatStats.SlugcatFoodMeter(MoreSlugcatsEnums.SlugcatStatsName.Slugpup).y) >= 0)
                    {
                        game.GetStorySession.playerSessionRecords[j].pupCountInDen++;
                    }
                }
                else if (!flag)
                {
                    flag = true;
                    game.GetStorySession.playerSessionRecords[j].friendInDen = crit;
                    var orInitiateRelationship = crit.state.socialMemory.GetOrInitiateRelationship(game.Players[j].ID);
                    orInitiateRelationship.like = Mathf.Lerp(orInitiateRelationship.like, 1f, 0.5f);
                }
            }
		}
        
        orig(self, game, survived, newMalnourished);
    }

    private static bool Player_SlugSlamConditions(On.Player.orig_SlugSlamConditions orig, Player self, PhysicalObject otherObject)
    {
        return orig(self, otherObject) && !(otherObject is Creature crit && crit.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC);
    }

    private static void OracleBehavior_CheckStrayCreatureInRoom(ILContext il)
    {
        var cursor = new ILCursor(il);

        var loc = -1;
        cursor.GotoNext(MoveType.After,
            i => i.MatchLdloc(out loc),
            i => i.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
            i => i.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
            i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
            i => i.MatchCallOrCallvirt(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Inequality")));

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldloc, loc);
        cursor.EmitDelegate((AbstractCreature crit) => crit.creatureTemplate.type != BeeEnums.CreatureType.Bup);
        cursor.Emit(OpCodes.And);
    }


    private static bool OracleBehavior_CheckSlugpupsInRoom(On.OracleBehavior.orig_CheckSlugpupsInRoom orig, OracleBehavior self)
    {
        return orig(self) || self.oracle.room.abstractRoom.creatures.Any(creature => creature.creatureTemplate.type == BeeEnums.CreatureType.Bup && creature.state.alive);
    }

    private static void GhostCreatureSedater_Update(ILContext il)
    {
        var cursor = new ILCursor(il);

        var loc = -1;
        cursor.GotoNext(MoveType.After,
            i => i.MatchLdloc(out loc),
            i => i.MatchCallOrCallvirt(out _),
            i => i.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
            i => i.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
            i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
            i => i.MatchCallOrCallvirt(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Inequality")));

        cursor.MoveAfterLabels();
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldloc, loc);
        cursor.EmitDelegate((GhostCreatureSedater self, int i) => self.room.abstractRoom.creatures[i].creatureTemplate.type != BeeEnums.CreatureType.Bup);
        cursor.Emit(OpCodes.And);
    }

    private static void VoidSeaScene_Update(ILContext il)
    {
        var cursor = new ILCursor(il);

        var loc = -1;
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
        cursor.EmitDelegate((VoidSeaScene self, int i) => self.room.abstractRoom.creatures[i].creatureTemplate.type == BeeEnums.CreatureType.Bup);
        cursor.Emit(OpCodes.Or);
    }

    private static void BupsFood(ILContext il)
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

    private static void BupsAI(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.isNPC && self.room != null && self.AI != null)
        {
            // >non-nullable type
            // >looks inside
            // >NullReferenceExepction
            if (self.Template.type.value == "Bup")
            {
                var ai = self.AI;

                // -- normal bups
                Creature Creaturenearby = null;
                foreach (var otherPlayers in self.room.updateList.OfType<Creature>())
                {
                    if (otherPlayers is not Player && !otherPlayers.Template.smallCreature && Custom.DistLess(self.bodyChunks[0].pos, otherPlayers.bodyChunks[0].pos, otherPlayers.bodyChunks[0].rad + 20))
                    {
                        Creaturenearby = otherPlayers;
                        break;
                    }
                }
                if (Creaturenearby != null && !Creaturenearby.dead && (self.Consious || (self.dangerGraspTime < 200 && !self.dead)) && !self.Bee().stingerUsed && self.Bee().stingerAttackCooldown <= 0)
                {
                    ai.abstractAI.Moved(); //RUN AWAY AFTER STING
                    ai.nap = true;
                    var dir = Custom.DirVec(self.bodyChunks[1].pos, Creaturenearby.mainBodyChunk.pos);
                    self.Bee().StingerAttack(new Vector2(60, 20) * dir , new Vector2(20, 20) * dir);
                }

                // -- please do not look, spoilers for secret mod. || It is used for debugging follow test
                Player Snowflake = null;
                foreach (var otherPlayers in self.room.updateList.OfType<Player>())
                {
                    if (self.Bee().DRAGGER_COUNT && otherPlayers.slugcatStats.name.value == "SnowflakeCat" && Custom.DistLess(self.bodyChunks[0].pos, otherPlayers.bodyChunks[0].pos, otherPlayers.bodyChunks[0].rad + 500))
                    {
                        Snowflake = otherPlayers;
                        break;
                    }
                }
                if (Snowflake != null)
                {
                    ai.abstractAI.SetDestination(Snowflake.abstractCreature.pos);
                }

                // -- fly
                if (self.Bee().isFlying)
                {
                    if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.y, self.bodyChunks[0].pos.x)).AnyBeam) //temporary, not sure if theyll fall off beam before they can climb up
                    {
                        self.Bee().StopFlight();
                        return; 
                    }
                    for (int i = 0; i <= 100; i += 10)
                    {
                        if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x, self.bodyChunks[0].pos.y - i)).Terrain == Room.Tile.TerrainType.Solid || self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x, self.bodyChunks[0].pos.y - i)).Terrain == Room.Tile.TerrainType.Floor)
                        {
                            self.Bee().StopFlight();
                            return;
                        }
                        else if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x + (self.flipDirection * 20), self.bodyChunks[0].pos.y - i + 70)).Terrain == Room.Tile.TerrainType.Solid &&
                             self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x + (self.flipDirection * 20), self.bodyChunks[0].pos.y - i + 90)).Terrain == Room.Tile.TerrainType.Air)
                        {
                            self.Bee().StopFlight();
                            return;
                        }

                    }


                    // -- finding poles left right by 1 tiles  
                    if (self.Bee().CDMET)
                    {
                        self.bodyChunks[0].vel.x += Mathf.Sign(self.Bee().polepos.x);
                    }
                    else
                    {
                        for (int i = 0; i <= self.Bee().wingStamina; i += 10)
                        {
                            if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x - i, self.bodyChunks[0].pos.y)).AnyBeam)
                            {
                                self.Bee().polepos = new Vector2(-i, 0);
                                self.Bee().CDMET = true;
                                return;
                            }
                        }
                        for (int i = 0; i <= self.Bee().wingStamina; i += 10)
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