using System.Runtime.CompilerServices;
using SlugBase.DataTypes;

namespace BeeWorld;

public static class BeeEnums
{
    public static SlugcatStats.Name Beecat = new("bee");
    public static SlugcatStats.Name Secret = new("SnowFlake");
    public static SlugcatStats.Name SnowFlake = new("SnowFlakeCat");
    public static class Sound
    {
        public static SoundID BeeBuzz;
    }

    public static class AbstractObject
    {
        public static AbstractPhysicalObject.AbstractObjectType BeeFlower;
    }

    public static class Color
    {
        public static PlayerColor Body = new(nameof(Body));
        public static PlayerColor Eyes = new(nameof(Eyes));
        public static PlayerColor Wings = new(nameof(Wings));
        public static PlayerColor Tail = new(nameof(Tail));
        public static PlayerColor TailStripes = new("Tail Stripes");
        public static PlayerColor Antennae = new(nameof(Antennae));
        public static PlayerColor NeckFluff = new("Neck Fluff");
    }

    public static class CreatureType
    {
        public static CreatureTemplate.Type Bup;
    }

    public static class SandboxUnlockID
    {
        public static MultiplayerUnlocks.SandboxUnlockID Bup;
    }

    public static void RegisterValues()
    {
        RuntimeHelpers.RunClassConstructor(typeof(Color).TypeHandle);        
        
        Sound.BeeBuzz = new SoundID("beebuzz", true);
        AbstractObject.BeeFlower = new(nameof(AbstractObject.BeeFlower), true);

        if (ModManager.MSC)
        {
            CreatureType.Bup = new(nameof(CreatureType.Bup), true);
            SandboxUnlockID.Bup = new(nameof(SandboxUnlockID.Bup), true);
        }
    }

    public static void UnregisterValues()
    {
        Sound.BeeBuzz?.Unregister();
        AbstractObject.BeeFlower?.Unregister();
        CreatureType.Bup?.Unregister();
        SandboxUnlockID.Bup.Unregister();
    }
}