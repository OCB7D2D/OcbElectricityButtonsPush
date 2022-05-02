public class NetPkgPushButton : NetPackage
{

    private int ClrIdx = 0;
    private Vector3i Position = Vector3i.zero;

    public NetPkgPushButton ToServer(int clrIdx, Vector3i position)
    {
        ClrIdx = clrIdx;
        Position = position;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        ClrIdx = _br.ReadInt32();
        Position = new Vector3i(
            _br.ReadInt32(),
            _br.ReadInt32(),
            _br.ReadInt32());
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(ClrIdx);
        _bw.Write(Position.x);
        _bw.Write(Position.y);
        _bw.Write(Position.z);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        // Simply call toggle on the tile entity (server side) to do all the hard work
        if (_world.GetTileEntity(ClrIdx, Position) is TileEntityButtonPush te) te.Toggle();
    }

    public override int GetLength() => 28;
}
