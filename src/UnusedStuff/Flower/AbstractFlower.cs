namespace BeeWorld;

public class AbstractFlower : AbstractPhysicalObject
{
    public AbstractFlower(World world, WorldCoordinate pos, EntityID ID) : base(world, BeeEnums.AbstractObject.BeeFlower, null, pos, ID)
    {
    }

    public override void Realize()
    {
        base.Realize();
        if (realizedObject == null)
        {
            realizedObject = new Flower(this);
        }

    }
}