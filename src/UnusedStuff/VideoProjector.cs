/*using System;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

namespace BeeWorld
{
    public class VideoProjector
    {
        public Vector2? setPos;
        public float? setAlpha;

        public WeakReference<Room> roomRef;
        public WeakReference<VideoSprite> videoSpriteRef;

        public GameObject videoPlayerObject;
        public VideoPlayer videoPlayer;

        private object atlasLock;
        private FAtlas atlas;

        public bool ready;
        public int videoWidth;
        public int videoHeight;


        public VideoProjector()
        {
            atlasLock = new object();
            roomRef = new WeakReference<Room>(null);
            videoSpriteRef = new WeakReference<VideoSprite>(null);

            videoPlayerObject = new GameObject();

            videoPlayer = videoPlayerObject.AddComponent<VideoPlayer>();
            videoPlayer.url = "C:\\Users\\Vigaro\\Downloads\\beemovie.mp4";
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            videoPlayer.prepareCompleted += VideoPlayer_prepareCompleted;
            videoPlayer.frameReady += VideoPlayer_frameReady;
            videoPlayer.frameDropped += VideoPlayer_frameDropped;
            videoPlayer.errorReceived += VideoPlayer_errorReceived;
            videoPlayer.sendFrameReadyEvents = true;
            videoPlayer.skipOnDrop = true;
            Debug.LogWarning("Preparing video");
            videoPlayer.Prepare();
        }

        private void VideoPlayer_errorReceived(VideoPlayer source, string message)
        {
            Debug.LogWarning("Error on frame " + source.frame + ": " + message);
        }

        private void VideoPlayer_frameDropped(VideoPlayer source)
        {
            Debug.LogWarning("Frame Dropped! " + source.frame);
        }

        private void VideoPlayer_prepareCompleted(VideoPlayer source)
        {
            Debug.LogWarning("Video ready");
            //source.Pause();
            videoWidth = (int)source.width;
            videoHeight = (int)source.height;
            ready = true;
        }

        private void VideoPlayer_frameReady(VideoPlayer source, long frameIdx)
        {

            AsyncGPUReadback.Request(source.texture, 0, TextureFormat.RGBA32, request =>
            {
                if (request.hasError)
                {
                    Debug.LogError("Error when copying frame " + frameIdx);
                    return;
                }

                var texture = new Texture2D(videoWidth, videoHeight, TextureFormat.RGBA32, false);
                texture.LoadRawTextureData(request.GetData<uint>());
                texture.Apply();

                lock (atlasLock)
                {
                    if (atlas != null)
                    {
                        atlas.Unload();
                    }

                    atlas = Futile.atlasManager.LoadAtlasFromTexture("bee_movie_"+frameIdx.ToString(), texture, false);
                }
            });
        }

        public void Update(Player player, bool eu)
        {
            if (!ready) return;

            var newRoom = false;
            if (!roomRef.TryGetTarget(out var room))
            {
                if (player.room != null)
                {
                    room = player.room;
                    roomRef.SetTarget(room);
                    newRoom = true;
                }
            }
            else if (room != player.room)
            {
                room = player.room;
                roomRef.SetTarget(room);
                newRoom = true;
                if(videoSpriteRef.TryGetTarget(out var videoSprite)) {
                    videoSprite.Destroy();
                }
            }

            if (room != null)
            {
                VideoSprite videoSprite = null;
                if (newRoom)
                {
                    videoSprite = new VideoSprite(this);
                    room.AddObject(videoSprite);
                    videoSpriteRef.SetTarget(videoSprite);
                }
                else
                {
                    videoSpriteRef.TryGetTarget(out videoSprite);
                }

                if (videoSprite != null && player != null)
                {
                    videoSprite.setPos = player.bodyChunks[0].pos + new Vector2(0, 200);
                    videoSprite.playerCharacterReady = true;
                }
            }
        }

        public void Pause()
        {
            videoPlayer.Pause();
        }

        public void Resume()
        {
            videoPlayer.Play();
        }

        public void Seek(long frame)
        {
            videoPlayer.frame = frame;
        }

        public long CurrentFrame => videoPlayer.frame;

        public class VideoSprite : CosmeticSprite
        {
            public Vector2? setPos;
            public VideoProjector projector;
            public bool playerCharacterReady;

            public VideoSprite(VideoProjector projector)
            {
                this.projector = projector;
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                if (setPos != null)
                {
                    pos = setPos.Value;
                    setPos = null;
                }
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                lock (projector.atlasLock)
                {
                    if (projector.atlas != null && projector.atlas.elements.Count > 0)
                    {
                        sLeaser.sprites[0].element = projector.atlas.elements[0];
                    }
                }

                if (sLeaser.sprites[0] != null)
                {
                    if (pos.x > 0 && pos.y > 0)
                    {
                        sLeaser.sprites[0].x = pos.x - camPos.x;
                        sLeaser.sprites[0].y = pos.y - camPos.y;

                        if (playerCharacterReady && !sLeaser.sprites[0].isVisible)
                        {
                            sLeaser.sprites[0].isVisible = true;
                            sLeaser.sprites[0].alpha = 0.8f;
                        }
                    }
                }

                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                base.InitiateSprites(sLeaser, rCam);

                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite("Futile_White", true);
                sLeaser.sprites[0].isVisible = false;
                sLeaser.sprites[0].alpha = 0f;
                //sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Projection"];

                this.AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("HUD"));
            }
        }
    }
}*/