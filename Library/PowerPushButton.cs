using System.Collections.Generic;

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

    // Disconnect is called when children are no longer powered
    // For our group this means its direct children are still powered
    // But anything further down the tree gets disconnected from power
    public override void HandleDisconnectChildren()
    {
        queue.Enqueue(this);
        // Start to process children
        while (queue.Count > 0)
        {
            // Process each button in the tree
            PowerPushButton btn = queue.Dequeue();
            btn.HandlePowerUpdate(false);
            // Do not disconnect our group!
            foreach (var child in btn.Children)
            {
                if (child is PowerPushButton item)
                {
                    queue.Enqueue(item);
                }
                else
                {
                    child.HandleDisconnect();
                }
            }

        }
    }

    // Main function to change the trigger
    // Should only be called on group root!
    public void SetIsActive(bool value)
    {
        // Update our flags
        isActive = value;
        isTriggered = value;
        // Process whole tree
        queue.Enqueue(this);
        // Start to process children
        while (queue.Count > 0)
        {
            // Process each button in the tree
            PowerPushButton btn = queue.Dequeue();
            // Simply update the flag
            btn.parentTriggered = value;
            btn.hasChangesLocal = true;
            // Check if we have a tile entity loaded
            if (btn.TileEntity is TileEntityButtonPush te)
            {
                // Apply changes also to tile entity
                te.Activate(IsPowered, value);
                // Inform all clients
                te.SetModified();
            }
            // Enqueue all further children to process
            foreach (PowerItem child in btn.Children)
            {
                if (child is PowerPushButton item)
                    queue.Enqueue(item);
                // Disconnect if needed
                else if (value == false)
                    child.HandleDisconnect();
            }
        }
        // Also inform root about changes
        SendHasLocalChangesToRoot();
    }

}
