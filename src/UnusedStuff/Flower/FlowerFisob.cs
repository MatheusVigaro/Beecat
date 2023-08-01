using Fisobs.Items;
using Fisobs.Core;
using Fisobs.Sandbox;
using Fisobs.Properties;

namespace BeeWorld;

public class FlowerFisob : Fisob
{
    public FlowerFisob() : base(BeeEnums.AbstractObject.BeeFlower)
    {
        Icon = new FlowerIcon();
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData entitySaveData, SandboxUnlock unlock)
    {
        return new AbstractFlower(world, entitySaveData.Pos, entitySaveData.ID);
    }


    private static readonly FlowerProperties properties = new();

    public override ItemProperties Properties(PhysicalObject forObject)
    {
        // If you need to use the forObject parameter, pass it to your ItemProperties class's constructor.
        // The Mosquitoes example demonstrates this.
        return properties;
    }
}