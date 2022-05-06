#define UPDATE_IMMEDIATE
using System.Collections.Generic;

public class TileEntityButtonPush : TileEntityPoweredTrigger
{

    // Really just an arbitrary number (I tend to use number above 241)
    // ToDo: make this overwriteable via e.g. a file!?
    public static TileEntityType Type => (TileEntityType)243;

    // Store state client side
    // Only used if not a server
    private bool isActive = false;

    // Overload custom tile entity type (used to persist entity)
    public override TileEntityType GetTileEntityType() => Type;

    // Basic constructor (nothing specialized here)
    public TileEntityButtonPush(Chunk _chunk) : base(_chunk) {}

    // Return either from PowerItem or client state
    public bool IsEnabled
    {
        get
        {
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                return isActive;
            if (PowerItem is PowerTrigger trigger)
                return trigger.IsActive;
            return false;
        }
    }


    // Return root of button group (server side only)
    public PowerPushButton GetRootCircuitItem()
    {
        // if (!ConnectionManager.Instance.IsServer)
        //     throw new Exception("Only call server side");
        PowerPushButton node = GetPowerItem() as PowerPushButton;
        while (node != null)
        {
            if (node.Parent is PowerPushButton btn)
            {
                node = btn;
                continue;
            }
            break;
        }
        return node;
    }

    public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
    {
        base.read(_br, _eStreamMode);
        if (_eStreamMode == StreamModeRead.FromServer)
        {
            ClientData.Property1 = _br.ReadByte();
            ClientData.Property2 = _br.ReadByte();
            isActive = _br.ReadBoolean();
            UpdateEmissionColor();
        }
    }

    public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
    {
        base.write(_bw, _eStreamMode);
        if (_eStreamMode == StreamModeWrite.ToClient) 
        {
            PowerPushButton item = GetRootCircuitItem();
            _bw.Write((byte) item.TriggerPowerDelay);
            _bw.Write((byte) item.TriggerPowerDuration);
            _bw.Write(item.IsActive);
        }
    }

    protected override PowerItem CreatePowerItem()
    {
        PowerItem item = base.CreatePowerItem();
        if (item is PowerTrigger trigger)
        {
            // Update to our own type
            // This will be persisted
            return new PowerPushButton()
            {
                TriggerType = trigger.TriggerType
            };
        }
        return item;
    }

    static Queue<PowerPushButton> queue = new Queue<PowerPushButton>();

    public void Toggle()
    {
        // if (!ConnectionManager.Instance.IsServer)
        //     throw new Exception("Only call server side");
        // Get the root power circuit item
        PowerPushButton root = GetRootCircuitItem();
        // Triggered power duration doesn't really exist for us (we use always or timed)
        // ToDo: maybe we should instead use triggered (makes more sense conceptually?)
        if (root.TriggerPowerDuration == PowerTrigger.TriggerPowerDurationTypes.Triggered)
            root.TriggerPowerDuration = PowerTrigger.TriggerPowerDurationTypes.Always;
        // Check if mode is timed (meaning we will just reset the timer if already on)
        if (root.TriggerPowerDuration == PowerTrigger.TriggerPowerDurationTypes.Always)
        {
#if UPDATE_IMMEDIATE
            // Immediately setup the state
            root.SetIsActive(!root.IsActive);
#else
            // Let PowerManager figure it out
            if (root.IsActive) root.ResetTrigger();
            else root.IsTriggered = true;
#endif
        }
        else
        {
            // Otherwise we just toggle the thing
            root.IsTriggered = true; // !root.IsTriggered;
        }
    }

    protected override void setModified()
    {
        base.setModified();
        UpdateEmissionColor();
    }

    public virtual void UpdateEmissionColor()
    {
        if (!(GameManager.Instance.World is WorldBase world)) return;
        if (!(world.GetChunkFromWorldPos(ToWorldPos()) is Chunk chunk)) return;
        UpdateEmissionColor(chunk.GetBlockEntity(ToWorldPos()));
    }

    public virtual void UpdateEmissionColor(BlockEntityData blockEntity)
    {
        BlockButtonPush.UpdateEmissionColor(IsPowered, IsEnabled, blockEntity);
    }

}
