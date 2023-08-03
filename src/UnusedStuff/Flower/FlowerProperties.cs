using BeeWorld.Extensions;
using Fisobs.Properties;

namespace BeeWorld;

public class FlowerProperties : ItemProperties
{
    public override void Throwable(Player player, ref bool throwable)
    {
        throwable = false;
    }

    public override void ScavCollectScore(Scavenger scav, ref int score)
    {
        score = 0;
    }

    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        if (player.IsBee())
        {
            grabability = Player.ObjectGrabability.OneHand;
        } 
        else
        {
            grabability = Player.ObjectGrabability.CantGrab;
        }   
    }
}