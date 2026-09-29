using System.Runtime.CompilerServices;
using SlugBase.DataTypes;

namespace BeeWorld;

public static class BeeEnums
{
    public static SlugcatStats.Name Beecat = new("bee");
    public static SlugcatStats.Name SnowFlake = new("SnowflakeCat");
    public static class Sound
    {
        public static SoundID BeeBuzz;
    }

    public static class AbstractObject
    {
        public static AbstractPhysicalObject.AbstractObjectType BeeFlower;
    }

    public static class Datepearl
    {
        public static DataPearl.AbstractDataPearl.DataPearlType BeePearls;
    }

    public static class Conversations
    {
        public static Conversation.ID MeetLTTM;
        public static Conversation.ID MeetFPpearls;
        public static Conversation.ID MeetFP;
        public static Conversation.ID PearlRead;
        public static Conversation.ID Forced;
        public static Conversation.ID nonmsc;
        public static Conversation.ID nonfp;
        public static Conversation.ID extra;
        public static SSOracleBehavior.Action BeeThrow;
        public static SSOracleBehavior.Action Reading;
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
        Datepearl.BeePearls = new DataPearl.AbstractDataPearl.DataPearlType("Beepearl", true);
        AbstractObject.BeeFlower = new(nameof(AbstractObject.BeeFlower), true);

        Conversations.nonmsc = new Conversation.ID("nonmsc", true);
        if (ModManager.MSC)
        {
            Conversations.nonfp = new Conversation.ID("nonfp", true);
            Conversations.Forced = new Conversation.ID("Forced", true);
            Conversations.PearlRead = new Conversation.ID("PearlRead", true);
            Conversations.MeetLTTM = new Conversation.ID("MeetLTTM", true);
            Conversations.MeetFPpearls = new Conversation.ID("MeetFPpearls", true);
            Conversations.MeetFP = new Conversation.ID("MeetFP", true);
            Conversations.extra = new Conversation.ID("extra", true);
            Conversations.BeeThrow = new SSOracleBehavior.Action("BeeThrow", true);
            Conversations.Reading = new SSOracleBehavior.Action("Reading", true);
            
            CreatureType.Bup = new(nameof(CreatureType.Bup), true);
            SandboxUnlockID.Bup = new(nameof(SandboxUnlockID.Bup), true);
        }
    }

    public static void UnregisterValues()
    {
        Conversations.BeeThrow?.Unregister();
        Conversations.nonfp?.Unregister();
        Conversations.nonmsc?.Unregister();
        Conversations.Forced?.Unregister();
        Conversations.PearlRead?.Unregister();
        Conversations.MeetLTTM?.Unregister();
        Conversations.MeetFPpearls?.Unregister();
        Conversations.MeetFP?.Unregister();
        Datepearl.BeePearls?.Unregister();
        Sound.BeeBuzz?.Unregister();
        AbstractObject.BeeFlower?.Unregister();
        CreatureType.Bup?.Unregister();
        SandboxUnlockID.Bup?.Unregister();
    }
}