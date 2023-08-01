using SlugBase.DataTypes;

namespace BeeWorld;

public static class BeeEnums
{
    public static SlugcatStats.Name Beecat = new("bee");
    
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
        public static PlayerColor Body;
        public static PlayerColor Eyes;
        public static PlayerColor Wings;
        public static PlayerColor Tail;
        public static PlayerColor TailStripes;
        public static PlayerColor Antennae;
        public static PlayerColor NeckFluff;
    }

    public static void RegisterValues()
    {
        Sound.BeeBuzz = new SoundID("beebuzz", true);
        AbstractObject.BeeFlower = new AbstractPhysicalObject.AbstractObjectType("BeeFlower", true);

        Color.Body = new PlayerColor("Body");
        Color.Eyes = new PlayerColor("Eyes");
        Color.Wings = new PlayerColor("Wings");
        Color.Tail = new PlayerColor("Tail");
        Color.TailStripes = new PlayerColor("Tail Stripes");
        Color.Antennae = new PlayerColor("Antennae");
        Color.NeckFluff = new PlayerColor("Neck Fluff");
    }

    public static void UnregisterValues()
    {
        Sound.BeeBuzz.Unregister();
        AbstractObject.BeeFlower.Unregister();
    }
}