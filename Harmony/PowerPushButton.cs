using System.IO;

public class PowerPushButton : PowerTrigger
{

    // ####################################################################
    // ####################################################################

    public override PowerItemTypes PowerItemType => BlockButtonPush.PowerItemType;

    // ####################################################################
    // ####################################################################

    public PowerPushButton GetCurcuitRoot()
    {
        var root = this;
        while (root.Parent is PowerPushButton parent)
            root = parent;
        return root;
    }

    private bool IsInstantToggle()
    {
        var root = this; // GetCurcuitRoot();
        return root.triggerPowerDelay == TriggerPowerDelayTypes.Instant &&
            (root.triggerPowerDuration == TriggerPowerDurationTypes.Always ||
            root.triggerPowerDuration == TriggerPowerDurationTypes.Triggered);
    }

    // ####################################################################
    // ####################################################################

    public override bool IsActive
    {
        get
        {
            if (parentTriggered) return true;
            var root = GetCurcuitRoot();
            return root.IsInstantToggle() ?
                root.isTriggered : root.isActive;
        }
        // EO IsActive getter
    }
    // EO IsActive property

    // ####################################################################
    // ####################################################################

    public override void HandlePowerUpdate(bool parentIsOn)
    {
        if (TileEntity != null)
        {
            // Only difference to vanilla is that we pass `IsActive` instead of `isTriggered`
            ((TileEntityPoweredTrigger)TileEntity).Activate(isPowered & parentIsOn, IsActive);

            TileEntity.SetModified();
        }
        for (int index = 0; index < Children.Count; ++index)
        {
            if (Children[index] is PowerTrigger)
            {
                HandleParentTriggering(Children[index] as PowerTrigger);
                Children[index].HandlePowerUpdate(isPowered & parentIsOn);
            }
            else if (IsActive) Children[index].HandlePowerUpdate(isPowered & parentIsOn);
        }
        hasChangesLocal = true;
        HandleSingleUseDisable();
    }
    // EO HandlePowerUpdate

    // ####################################################################
    // ####################################################################

    public override bool IsTriggered
    {
        set
        {
            if (IsInstantToggle())
            {
                lastTriggered = isTriggered;
                isTriggered = value;
                if (isTriggered && !lastTriggered)
                    isActive = true;
                SendHasLocalChangesToRoot();
                if (!isTriggered && lastTriggered)
                {
                    HandleDisconnectChildren();
                    isActive = false;
                }
            }
            else
            {
                base.IsTriggered = value;
            }
        }
        // EO IsTriggered setter
    }
    // EO IsTriggered property

    // ####################################################################
    // ####################################################################

    public override void read(BinaryReader _br, byte _version)
    {
        base.read(_br, _version);
        if (!IsInstantToggle()) return;
        isTriggered = _br.ReadBoolean();
    }
    // EO read

    public override void write(BinaryWriter _bw)
    {
        base.write(_bw);
        if (!IsInstantToggle()) return;
        _bw.Write(isTriggered);
    }
    // EO write

    // ####################################################################
    // ####################################################################

    protected override void HandleSingleUseDisable()
    {
        if (IsInstantToggle()) return;
        base.HandleSingleUseDisable();
    }
    // EO HandleSingleUseDisable

    public override void CachedUpdateCall()
    {
        if (IsInstantToggle()) return;
        base.CachedUpdateCall();
    }
    // EO CachedUpdateCall

    // ####################################################################
    // ####################################################################

}
