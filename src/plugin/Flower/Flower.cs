using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;
using System.Xml;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using SlugBase.DataTypes;
using UnityEngine.XR;
using System.Diagnostics;

namespace BeeWorld
{
    public class Flower : PlayerCarryableItem, IDrawable
    {
        public Flower(AbstractPhysicalObject abstractPhysicalObject) : base(abstractPhysicalObject)
        {
            bodyChunks = new BodyChunk[1];
            bodyChunks[0] = new BodyChunk(this, 0, abstractPhysicalObject.Room.realizedRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile), 10f, 0.05f);
             
            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.93f;
            gravity = 0.6f;
            bounce = 0.2f;
            surfaceFriction = 0.7f;
            collisionLayer = 2;
            waterFriction = 0.95f;
            buoyancy = 0.9f;
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Items");
            }
            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].RemoveFromContainer();
            }
            newContatiner.AddChild(sLeaser.sprites[0]);
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            sLeaser.sprites[0].x = pos.x - camPos.x;
            sLeaser.sprites[0].y = pos.y - camPos.y;

            if (base.slatedForDeletetion || this.room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];
            sLeaser.sprites[0] = new FSprite("SkyDandelion", true);
            sLeaser.sprites[0].color = Color.red;
            sLeaser.sprites[0].scale = 2;
            this.AddToContainer(sLeaser, rCam, null);
        }
        bool cutsceneStarted;

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (!cutsceneStarted && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player player)
            {
                cutsceneStarted = true;
                room.AddObject(new Cutscene.FlowerPickupCutscene(player, this));
            }
        }
    }
}