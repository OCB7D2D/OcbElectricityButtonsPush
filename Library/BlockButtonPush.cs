using System.Collections.Generic;
using UnityEngine;

public class BlockButtonPush : BlockPowered
{

    private BlockActivationCommand[] cmds = new BlockActivationCommand[3]
    {
        new BlockActivationCommand("light", "electric_switch", true),
        new BlockActivationCommand("options", "tool", true),
        new BlockActivationCommand("take", "hand", false)
    };

    public BlockButtonPush() => HasTileEntity = true;

    public override void Init() => base.Init();


    // Copied from other powered blocks
    public override void OnBlockAdded(
        WorldBase _world,
        Chunk _chunk,
        Vector3i _blockPos,
        BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        if (_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityButtonPush) return;
        TileEntityPowered tileEntity = CreateTileEntity(_chunk);
        tileEntity.localChunkPos = World.toBlock(_blockPos);
        tileEntity.InitializePowerData();
        _chunk.AddTileEntity(tileEntity);
    }


    // Currently only called from TileEntityButtonPush
    public static void UpdateEmissionColor(
        bool isPowered, bool isEnabled, 
        BlockEntityData blockEntity)
    {
        if (blockEntity != null &&
            blockEntity.transform != null &&
            blockEntity.transform.gameObject != null)
        {
            Color color = isEnabled ? Color.green : Color.red;
            float intensity = isPowered ? 2f : .5f;
            if (!isPowered) color = Color.yellow;
            // Code below is mostly copied from vanilla
            Renderer[] componentsInChildren = blockEntity.transform
                .gameObject.GetComponentsInChildren<Renderer>();
            if (componentsInChildren != null)
            {
                for (int index = 0; index < componentsInChildren.Length; ++index)
                {
                    if (
                        componentsInChildren[index].material !=
                        componentsInChildren[index].sharedMaterial
                    )
                    {
                        componentsInChildren[index].material =
                            new Material(componentsInChildren[index]
                                    .sharedMaterial);
                    }
                    // Only enable emission color on specific tags
                    // No idea how this is done in e.g. vanilla power switch
                    if (componentsInChildren[index].tag != "T_Deco") continue;
                    componentsInChildren[index].sharedMaterial = componentsInChildren[index].material;
                    componentsInChildren[index].material.SetColor("_EmissionColor", color * intensity);
                    componentsInChildren[index].material.SetColor("_Color", color);
                    componentsInChildren[index].material.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    public override void OnBlockEntityTransformAfterActivated(
        WorldBase _world,
        Vector3i _blockPos,
        int _cIdx,
        BlockValue _blockValue,
        BlockEntityData _ebcd)
    {
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
        if (_blockValue.ischild || !(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityButtonPush te)) return;
        te.UpdateEmissionColor(_ebcd);
    }

    public virtual TileEntityButtonPush CreateTileEntityButtonPush(Chunk chunk)
    {
        TileEntityButtonPush entityPoweredTrigger = new TileEntityButtonPush(chunk)
        {
            PowerItemType = PowerPushButton.Type,
            TriggerType = PowerTrigger.TriggerTypes.Motion
        };
        return entityPoweredTrigger;
    }


    public override TileEntityPowered CreateTileEntity(Chunk chunk)
    {
        return CreateTileEntityButtonPush(chunk);
    }

    public override void OnBlockValueChanged(
        WorldBase _world,
        Chunk _chunk,
        int _clrIdx,
        Vector3i _blockPos,
        BlockValue _oldBlockValue,
        BlockValue _newBlockValue)
    {
        base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
        if (_world.GetTileEntity(_clrIdx, _blockPos) is TileEntityButtonPush tileEntity)
            tileEntity.UpdateEmissionColor(null);
    }

    public override bool OnBlockActivated(
        int cmd,
        WorldBase _world,
        int _cIdx,
        Vector3i _blockPos,
        BlockValue _blockValue,
        EntityAlive _player)
    {
        if (_blockValue.ischild)
        {
            Vector3i parentPos = list[_blockValue.type].multiBlockPos.GetParentPos(_blockPos, _blockValue);
            BlockValue block = _world.GetBlock(parentPos);
            return OnBlockActivated(cmd, _world, _cIdx, parentPos, block, _player);
        }
        if (!(_world.GetTileEntity(_cIdx, _blockPos) is TileEntityButtonPush tileEntity)) return false;
        if (cmd == 0)
        {
            if (!ConnectionManager.Instance.IsServer)
            {
                // Let the server figure out what to update and what not
                // Ensures it works even if part of the group is not loaded
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                    NetPackageManager.GetPackage<NetPkgPushButton>()
                        .ToServer(_cIdx, _blockPos));
            }
            // We are actually the server, so we do have all PowerItems
            else
            {
                tileEntity.Toggle();
            }
        }
        else if (cmd == 1)
        {
            _player.AimingGun = false;
            _world.GetGameManager().TELockServer(_cIdx,
                tileEntity.ToWorldPos(), tileEntity.entityId,
                _player.entityId);
        }
        else if (cmd == 2)
        {
            TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
        }
        else {
            return false;
        }
        return true;
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(
        WorldBase _world,
        BlockValue _blockValue,
        int _clrIdx,
        Vector3i _blockPos,
        EntityAlive _entityFocusing)
    {
        var player = _world.GetGameManager().GetPersistentLocalPlayer();
        bool flag1 = _world.CanPlaceBlockAt(_blockPos, player);
        bool flag2 = _world.IsMyLandProtectedBlock(_blockPos, player);
        cmds[0].enabled = flag1;
        cmds[1].enabled = flag1;
        cmds[2].enabled = flag2 && TakeDelay > 0.0;
        return cmds;
    }

}