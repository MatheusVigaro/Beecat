using Fisobs.Core;

namespace BeeWorld;

public class FlowerIcon : Icon
{
    public override int Data(AbstractPhysicalObject apo)
    {
        return 0;
    }

    public override Color SpriteColor(int data)
    {
        return Color.red;
    }

    public override string SpriteName(int data)
    {
        return "SkyDandelion";
    }
}