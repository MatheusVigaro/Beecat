using BeeWorld.Extensions;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using HUD;
using MonoMod.RuntimeDetour;
using MoreSlugcats;
using Smoke;
using VoidSea;

namespace BeeWorld;

public class BupCritob : Critob
{
    public BupCritob() : base(BeeEnums.CreatureType.Bup)
    {
        Icon = new SimpleIcon("atlases/bups", new Color(1, 1, 1));
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
        cf.baseStunResistance = -25;
        cf.waterVision = 0.3f;
        cf.stowFoodInDen = true;   
        cf.throughSurfaceVision = 0.5f;
        cf.movementBasedVision = 0.5f;
        cf.communityInfluence = 0.1f;
        cf.bodySize = 1;
        cf.usesCreatureHoles = false;
        cf.usesNPCTransportation = true;
        cf.BlizzardAdapted = true;
        cf.BlizzardWanderer = true;
        cf.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
        cf.lungCapacity = 1600;
        cf.jumpAction = "Swap Heads";
        cf.pickupAction = "Grab/Freeze";
        cf.shortcutSegments = 2;
        return cf;
    }

    public override void EstablishRelationships()
    {
        var s = new Relationships(Type);
        s.Eats(CreatureTemplate.Type.Fly, .5f);
        s.Eats(CreatureTemplate.Type.EggBug, 1f);
        s.Fears(CreatureTemplate.Type.Vulture, 1f);
        s.Fears(CreatureTemplate.Type.BigEel, 1f);
        s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
        s.Fears(CreatureTemplate.Type.TentaclePlant, 1f);
        s.Fears(CreatureTemplate.Type.MirosBird, 1f);
        s.Fears(CreatureTemplate.Type.Centipede, 0.5f);
        s.Fears(CreatureTemplate.Type.Centiwing, 0.4f);
        s.Fears(CreatureTemplate.Type.LizardTemplate, 0.6f);
        s.Eats(CreatureTemplate.Type.SmallCentipede, 0.6f);
        s.Eats(CreatureTemplate.Type.SmallNeedleWorm, 0.4f);
        s.Fears(CreatureTemplate.Type.BigSpider, 0.5f);
        s.Fears(CreatureTemplate.Type.SpitterSpider, 0.8f);
        s.Fears(CreatureTemplate.Type.DropBug, 0.5f);
        s.Fears(CreatureTemplate.Type.RedCentipede, 1f);
        s.IsInPack(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 0.5f);
        s.Eats(CreatureTemplate.Type.VultureGrub, 0.4f);
        s.EatenBy(CreatureTemplate.Type.LizardTemplate, 0.5f);
        s.FearedBy(CreatureTemplate.Type.Fly, 0.5f);
        s.FearedBy(CreatureTemplate.Type.LanternMouse, 0.3f);
        s.EatenBy(CreatureTemplate.Type.Vulture, 0.3f);
        s.HasDynamicRelationship(CreatureTemplate.Type.CicadaA, 1f);
        s.HasDynamicRelationship(CreatureTemplate.Type.CicadaB, 1f);
        s.HasDynamicRelationship(CreatureTemplate.Type.JetFish, 1f);
        s.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);
        s.EatenBy(CreatureTemplate.Type.MirosBird, 0.6f);
        s.HasDynamicRelationship(CreatureTemplate.Type.Scavenger, 1f);
        s.EatenBy(CreatureTemplate.Type.BigSpider, 0.6f);
        s.Fears(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 1f);
        s.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.6f);
        s.Eats(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.5f);
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
        IL.SaveState.SessionEnded += SaveState_SessionEnded;
        On.Player.CanMaulCreature += Player_CanMaulCreature;
    }   

    private static bool Player_CanMaulCreature(On.Player.orig_CanMaulCreature orig, Player self, Creature crit)
    {
        var result = orig(self, crit);
        if (crit is Player PL)
        {
            if (PL.Template.type.value == "Bup")
            {
                return false;
            }
        }
        return result;
    }

    private static void SaveState_SessionEnded(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        //Check for Bups, so they don't count as friends
        var loc = -1;
        cursor.GotoNext(MoveType.After,
            i => i.MatchLdloc(out loc),
            i => i.MatchCallOrCallvirt(out _),
            i => i.MatchLdfld<AbstractCreature>(nameof(AbstractCreature.creatureTemplate)),
            i => i.MatchLdfld<CreatureTemplate>(nameof(CreatureTemplate.type)),
            i => i.MatchLdsfld<MoreSlugcatsEnums.CreatureTemplateType>(nameof(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)),
            i => i.MatchCallOrCallvirt(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Equality")));
        cursor.Emit(OpCodes.Ldarg_1);//Push RainWorldGame onto stack
        cursor.Emit(OpCodes.Ldloc, loc); //Push loop index value onto stack
        cursor.EmitDelegate<Func<bool, RainWorldGame, int, bool>>((matchFound, game, index) => //Send all three onto stack
        {
            //Return an existing match, or a BeePup match
            return matchFound || game.FirstAlivePlayer != null && game.world.GetAbstractRoom(game.FirstAlivePlayer.pos).creatures[index].creatureTemplate.type == BeeEnums.CreatureType.Bup;
        });
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
        cursor.EmitDelegate((CreatureTemplate old, World self) => self.game.session is StoryGameSession session && session.saveStateNumber == BeeEnums.Beecat || (BeeOptions.BupsSpawnAll.Value && Random.value >= BeeOptions.BupsSpawnRate.Value) ? StaticWorld.GetCreatureTemplate(BeeEnums.CreatureType.Bup) : old);
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
        if (self.saveStateNumber == BeeEnums.Beecat)
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
        }

        orig(self, game, survived, newMalnourished);
    }

    private static bool Player_SlugSlamConditions(On.Player.orig_SlugSlamConditions orig, Player self, PhysicalObject otherObject)
    {
        return orig(self, otherObject) && !(otherObject is Creature crit && (crit.abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC || crit.abstractCreature.creatureTemplate.type == BeeEnums.CreatureType.Bup));
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
                // -- snowbups
                if (self.Bee().SnowBup)
                {
                    if (self.Bee().effecttime > 0)
                    {
                        if (new[] { Player.AnimationIndex.HangFromBeam, Player.AnimationIndex.GetUpOnBeam, Player.AnimationIndex.StandOnBeam, Player.AnimationIndex.ClimbOnBeam, Player.AnimationIndex.GetUpToBeamTip, Player.AnimationIndex.HangUnderVerticalBeam, Player.AnimationIndex.BeamTip, Player.AnimationIndex.ZeroGPoleGrab }.Contains(self.animation))
                        {
                            self.Bee().effecttime--;
                        }
                        else
                        {
                            var smoke = new FireSmoke(self.room);
                            self.Bee().effecttime--;
                            smoke.EmitSmoke(self.mainBodyChunk.pos, -self.bodyChunks[0].vel * Custom.RNV(), new(113, 135, 171), 10);
                            smoke.EmitSmoke(self.mainBodyChunk.pos, -self.bodyChunks[0].vel * Custom.RNV(), new(161, 201, 209), 10);
                            smoke.EmitSmoke(self.mainBodyChunk.pos, -self.bodyChunks[0].vel * Custom.RNV(), Color.white, 10);

                        }
                    }
                        
                    if (self.canJump >= 4 && self.Bee().landed)
                    {
                        self.Bee().landed = false;
                    }
                    if (self.input[0].jmp && !self.input[1].jmp && self.canJump == 0 && !self.Bee().landed)
                    {
                        self.Bee().landed = true;
                        self.Bee().effecttime = 20;
                        self.Bee().isFlying = true;
                        self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.firstChunk.pos);
                        self.mushroomEffect = 1f;

                        var hypothermiaFactor = Mathf.Lerp(1, 0.5f, self.Hypothermia);

                        if (self.input[0].x != 0)
                        {
                            self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y * 0.25f, 0f) + 11f * hypothermiaFactor;
                            self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y * 0.25f, 0f) + 10f * hypothermiaFactor;
                            self.bodyChunks[0].vel.x = 10f * self.input[0].x * hypothermiaFactor;
                            self.bodyChunks[1].vel.x = 8f * self.input[0].x * hypothermiaFactor;
                            self.jumpBoost = 8;
                        }
                        else
                        {
                            self.bodyChunks[0].vel.y = 14f * hypothermiaFactor;
                            self.bodyChunks[1].vel.y = 13f * hypothermiaFactor;
                            self.jumpBoost = 10;
                        }
                        self.animation = Player.AnimationIndex.Flip;
                        self.Blink(5);
                    }
                    if (ai.abstractAI.isTamed)
                    {

                    }
                }

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
                if (Creaturenearby != null && !Creaturenearby.dead && (self.Consious || (self.dangerGraspTime < 200 && !self.dead)) && !self.Bee().stingerUsed && self.Bee().stingerAttackCooldown <= 0 && self.onBack == null)
                {
                    ai.abstractAI.Moved(); //RUN AWAY AFTER STING
                    ai.nap = true; // nvm go sleep
                    var dir = Custom.DirVec(self.bodyChunks[1].pos, Creaturenearby.mainBodyChunk.pos);
                    self.Bee().StingerAttack(new Vector2(60, 20) * dir , new Vector2(20, 20) * dir);
                }

                // -- please do not look, spoilers for secret mod. || It is used for debugging follow test
                Player Snowflake = null;
                foreach (var otherPlayers in self.room.updateList.OfType<Player>())
                {
                    if (self.Bee().SnowBup && otherPlayers.slugcatStats.name == BeeEnums.SnowFlake && Custom.DistLess(self.bodyChunks[0].pos, otherPlayers.bodyChunks[0].pos, otherPlayers.bodyChunks[0].rad + 500))
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

                    if (self.input[0].y > 0 )
                    {
                        self.bodyChunks[0].vel.y += BeeOptions.BupsFly.Value;
                        self.bodyChunks[1].vel.y += BeeOptions.BupsFly.Value;
                        self.Bee().wingStamina--;
                    }


                    // -- finding poles left right by 1 tiles  
                    if (self.Bee().CDMET)
                    {
                        float wa = self.bodyChunks[0].pos.x - self.Bee().polepos.x;
                        self.bodyChunks[0].vel.x += (wa <= 0) ? 1 : -1;
                        ai.abstractAI.SetDestination(self.abstractCreature.pos);    
                    }
                    else
                    {
                        for (int i = 0; i <= self.Bee().wingStamina; i += 10)
                        {
                            if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x - i, self.bodyChunks[0].pos.y)).AnyBeam)
                            {
                                self.Bee().polepos = new Vector2(self.bodyChunks[0].pos.x - i - 10, 0);
                                self.Bee().CDMET = true;
                                return;
                            }
                            if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x + i + 5, self.bodyChunks[0].pos.y)).AnyBeam)
                            {
                                self.Bee().polepos = new Vector2(self.bodyChunks[0].pos.x+i + 17, 0);
                                self.Bee().CDMET = true;
                                return;
                            }
                        }
                    }
                    if (!self.Bee().CDMET)
                    {
                        for (int i = 0; i <= self.Bee().wingStamina + 50; i += 10)
                        {
                            if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x - i, self.bodyChunks[0].pos.y - i)).Terrain == Room.Tile.TerrainType.Solid && self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x - i, self.bodyChunks[0].pos.y - i + 20)).Terrain == Room.Tile.TerrainType.Air)
                            {
                                self.Bee().polepos = new Vector2(self.bodyChunks[0].pos.x - i - 10, 0);
                                self.Bee().CDMET = true;
                                return;
                            }
                            if (self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x + i, self.bodyChunks[0].pos.y - i)).Terrain == Room.Tile.TerrainType.Solid && self.room.GetTile(new Vector2(self.bodyChunks[0].pos.x + i, self.bodyChunks[0].pos.y - i + 20)).Terrain == Room.Tile.TerrainType.Air)
                            {
                                self.Bee().polepos = new Vector2(self.bodyChunks[0].pos.x + i + 17, 0);
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
                }
            }
        }
    }
}