using MonoMod.RuntimeDetour;

namespace BeeWorld;

public static class BupHooks
{
    public static void Apply()
    {
        _ = new Hook(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Equality"), CursedHookDoNotCopyItWillSurelyBreakTheGame);
        On.Player.Stun += Player_Stun;
    }

    private static void Player_Stun(On.Player.orig_Stun orig, Player self, int st)
    {
        if (self.isNPC && self.Template.type.value == "Bup") //todo Figuring out how to check if its in shelter or not
        {
            return;
        }
        orig(self, st);
    }

    private static bool CursedHookDoNotCopyItWillSurelyBreakTheGame(Func<ExtEnum<CreatureTemplate.Type>, ExtEnum<CreatureTemplate.Type>, bool> orig, ExtEnum<CreatureTemplate.Type> a, ExtEnum<CreatureTemplate.Type> b)
    {
        return orig(a, b) || (ModManager.MSC && b?.value == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC?.value && a?.value == BeeEnums.CreatureType.Bup.value);
    }
}