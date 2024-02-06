public class TileEntityButtonPush : TileEntityPoweredTrigger
{

    // ####################################################################
    // ####################################################################

    public TileEntityButtonPush(Chunk _chunk) : base(_chunk) {}

    // ####################################################################
    // ####################################################################

    public override TileEntityType GetTileEntityType()
    {
        // really just an arbitrary number
        // I tend to use number above 241
        return BlockButtonPush.TileEntityType;
    }
    // EO GetTileEntityType

    // ####################################################################
    // ####################################################################

    protected override PowerItem CreatePowerItem() {
        return new PowerPushButton
        {
            TriggerType = PowerTrigger.TriggerTypes.Motion,
            TriggerPowerDuration = PowerTrigger.TriggerPowerDurationTypes.Triggered,
            TriggerPowerDelay = PowerTrigger.TriggerPowerDelayTypes.Instant
        };
    }
    // EO CreatePowerItem

    // ####################################################################
    // ####################################################################

    public TileEntityButtonPush GetCurcuitRoot()
    {
        if (ConnectionManager.Instance.IsServer)
        {
            var pwr = GetPowerItem() as PowerPushButton;
            var te = pwr.GetCurcuitRoot().TileEntity;
            return te as TileEntityButtonPush;
        }
        else
        {
            var root = this;
            var world = GameManager.Instance.World;
            while (world.GetTileEntity(0, root.GetParent())
                 is TileEntityButtonPush parent) root = parent;
            return root;
        }
    }
    // EO GetCurcuitRoot

    // ####################################################################
    // ####################################################################

}
