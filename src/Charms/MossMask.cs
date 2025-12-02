using ItemChanger.Items;
using Modding;

namespace DistantGreensCharms.Charms;

public class MossMask : ACharm
{
    public static readonly MossMask Instance = new();
    
    public override string Sprite  => "CharmIcons/MossMask.png"; //test
    public override string Name => "Moss-Mask"; 
    public override string Description => "Moss-Mask description"; //todo
    public override int DefaultCost => 3;
    public override string Scene => "GG_Workshop"; //todo
    public override float X => 17.65f; //todo
    public override float Y => 6.41f; //todo

    public override void Hook()
    {
        ModHooks.TakeHealthHook += CheckMaskActivation;
        ModHooks.SetPlayerBoolHook += ChargeCharm;
    }

    private bool charged = true;
    private int CheckMaskActivation(int damage)
    {
        if (!Equipped() || !charged || PlayerData.instance.health - damage > 0) return damage;
        if (PlayerData.instance.health - damage > 0) return damage;
        charged = false;
        return 0;
    }

    private bool ChargeCharm(string targetBool, bool value)
    {
        if(targetBool == "atBench" && value && Equipped()) charged = true;
        return value;
    }
}