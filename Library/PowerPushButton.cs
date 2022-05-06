#define UPDATE_IMMEDIATE
using System.Collections.Generic;
using UnityEngine;

public class PowerPushButton : PowerTrigger
{

    // ToDo: make this overwriteable via e.g. a file!?
    public static PowerItemTypes Type = (PowerItemTypes)83;

    // Just invent a power item type for us (hope nobody uses the same)
    public override PowerItemTypes PowerItemType => Type;

    // Re-use collection for better performance (GC needs less to do)
    static Queue<PowerPushButton> queue = new Queue<PowerPushButton>();

    // Expose publicly for debugging
    public bool IsTriggeredByParent => parentTriggered;
    public float PowerTime => powerTime;
    public float LastPowerTime => lastPowerTime;
    public float StartTime => delayStartTime;

    public bool LastTriggered => lastTriggered;

    public bool HasLocalChanges
    {
        get => hasChangesLocal;
        set => hasChangesLocal = value;
    }

    public override bool IsActive
    {
        get => isActive || parentTriggered;
    }

    protected override void HandleSoundDisable()
    {
        Audio.Manager.BroadcastPlay(Position, "trip_wire_trigger");
    }

#if UPDATE_IMMEDIATE

    protected override void HandleSingleUseDisable()
    {
        // Do nothing intentionally
        // Do not reset isTriggered
    }

    // Simple timer re-implementation
    // But call our function directly
    public override void CachedUpdateCall()
    {
        if (delayStartTime >= 0.0)
        {
            if (Time.time - lastPowerTime >= delayStartTime)
            {
                delayStartTime = -1f;
                SetupDurationTime();
                SetIsActive(true);
            }
        }
        else if (powerTime > 0.0 && !parentTriggered && Time.time - lastPowerTime >= powerTime)
        {
            powerTime = -1f;
            SetIsActive(false);
            HandleSoundDisable();
        }
    }

    // Main function to change the trigger
    // Should only be called on group root!
    public void SetIsActive(bool active)
    {
        // Update our flags
        // isActive = active;
        // isTriggered = active;
        // lastTriggered = active;
        // Process whole tree
        queue.Enqueue(this);
        // Also inform root about changes
        SendHasLocalChangesToRoot();
        // Start to process children
        while (queue.Count > 0)
        {
            // Process each button in the tree
            PowerPushButton btn = queue.Dequeue();
            // Simply update the flag
            btn.isActive = active;
            btn.isTriggered = active;
            btn.lastTriggered = active;
            btn.hasChangesLocal = false;
            // Check if we have a tile entity loaded
            // Non-recursive part of `HandlePowerUpdate`
            if (btn.TileEntity is TileEntityButtonPush te)
            {
                // Apply changes also to tile entity
                te.Activate(IsPowered, active);
                // Inform all clients
                te.SetModified();
            }
            // Enqueue all further children to process
            foreach (PowerItem child in btn.Children)
            {
                if (child is PowerPushButton item)
                {
                    item.parentTriggered = active;
                    queue.Enqueue(item);
                }
                // Disconnect if needed until next source
                else if (!active && !(child is PowerSource))
                {
                    if (child is PowerTrigger trigger)
                    {
                        trigger.SetTriggeredByParent(false);
                        trigger.HandleDisconnectChildren();
                    }
                    else
                    {
                        child.HandlePowerUpdate(false);
                        child.HandleDisconnect();
                    }
                }
            }
        }
    }

    // Disconnect is called when children are no longer powered
    // For our group this means its direct children are still powered
    // But anything further down the tree gets disconnected from power
    // NOTE: this overrules a harmony hook from electricity overhaul!
    public override void HandleDisconnectChildren()
    {
        SetIsActive(false);
        HandleSoundDisable();
    }

#else

    // Disconnect is called when children are no longer powered
    // For our group this means its direct children are still powered
    // But anything further down the tree gets disconnected from power
    // NOTE: this overrules a harmony hook from electricity overhaul!
    public override void HandleDisconnectChildren()
    {
        // Reset the values we need
        isActive = false;
        // Must not call `HandlePowerUpdate`
        // Overhaul has different behavior
        // HandlePowerUpdate(isPowered);
        // We already sent the changes
        hasChangesLocal = true;
        // Disconnect tree leafs
        queue.Enqueue(this);
        // Start to process children
        while (queue.Count > 0)
        {
            // Process each button in the tree
            PowerPushButton btn = queue.Dequeue();
            // Reset the values we need
            btn.isActive = false;
            btn.parentTriggered = false;
            // We already sent the changes
            btn.hasChangesLocal = true;
            // Do not disconnect our group from power!
            foreach (var child in btn.Children)
            {
                if (child is PowerPushButton item)
                {
                    queue.Enqueue(item);
                }
                else if (!(child is PowerSource))
                {
                    child.HandlePowerUpdate(false);
                    child.HandleDisconnect();
                }
            }
        }
    }
    
#endif

}
