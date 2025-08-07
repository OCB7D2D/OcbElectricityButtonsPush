using System.IO;
using UnityEngine;

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
        var root = GetCurcuitRoot();
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

    /*
    public override void HandlePowerUpdate(bool parentIsOn)
    {
        Log.Out("============= Handle Power Update");
        Log.Out("============= Handle Power Update");
        Log.Out("============= Handle Power Update");
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
                if (Children[index] is PowerTrigger trigger)
                    HandleParentTriggering(trigger);
                Children[index].HandlePowerUpdate(isPowered & parentIsOn);
            }
            else if (IsActive) Children[index].HandlePowerUpdate(isPowered & parentIsOn);
        }
        hasChangesLocal = true;
        HandleSingleUseDisable();
    }
    // EO HandlePowerUpdate
    */

    // ####################################################################
    // ####################################################################

    public void EnableVisualState() => GetCurcuitRoot().UpdateVisualState(true);
    public void DisableVisualState() => GetCurcuitRoot().UpdateVisualState(false);

    public void UpdateVisualState(bool enable)
    {
        var world = GameManager.Instance.World;
        BlockValue bv = world.GetBlock(Position);
        if (enable)
        {
            if ((bv.meta & 0b10) != 0b10)
            {
                bv.meta |= 0b10;
                world.SetBlockRPC(Position, bv);
            }
        }
        else
        {
            if ((bv.meta & 0b10) == 0b10)
            {
                bv.meta &= (byte)(bv.meta & ~(0b10));
                world.SetBlockRPC(Position, bv);
            }
        }
        if (bv.Block is BlockButtonPush block)
            foreach (var child in this.Children)
                if (child is PowerPushButton trigger)
                    trigger.UpdateVisualState(enable);
    }

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
                {
                    EnableVisualState();
                    isActive = true;
                }
                SendHasLocalChangesToRoot();
                if (!isTriggered && lastTriggered)
                {
                    DisableVisualState();
                    HandleDisconnectChildren();
                    isActive = false;
                }
            }
            else
            {
                if (isTriggered != value)
                {
                    Log.Out("Changed trigger to {0}", value);
                }
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

    public override void HandleSingleUseDisable()
    {
        if (IsInstantToggle()) return;
        base.HandleSingleUseDisable();
    }
    // EO HandleSingleUseDisable

    public override void CachedUpdateCall()
    {
        if (IsInstantToggle()) return;
        // Some code copied (do the compare twice)
        // This way we still call the original method
        // Might be useful if some one else patched it
        if (delayStartTime >= 0.0)
        {
            if (Time.time - lastPowerTime >= delayStartTime)
            {
                EnableVisualState();
            }
        }
        else if (powerTime > 0.0)
        {
            if (!parentTriggered && Time.time - lastPowerTime >= powerTime)
        {
                DisableVisualState();
            }
        } 
        base.CachedUpdateCall();
    }
    // EO CachedUpdateCall

    // ####################################################################
    // ####################################################################

    public override void CheckForActiveChange()
    {
        // This would allow to also trigger "always" mode
        // Doesn't make much sense, always means always!?
        // if (powerTime == -1.0) return;
        if (powerTime > 0.0) return;
        if (!lastTriggered) return;
        if (isTriggered) return;
        DisableVisualState();
        isActive = false;
        HandleDisconnectChildren();
        SendHasLocalChangesToRoot();
        powerTime = -1f;
    }
    // EO CheckForActiveChange

    // ####################################################################
    // ####################################################################

}
