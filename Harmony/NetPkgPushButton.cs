public class NetPkgPushButton : NetPackage
{

    // ####################################################################
    // ####################################################################

    public Vector3i blockPos;

    // ####################################################################
    // ####################################################################

    public NetPackage Setup(Vector3i position)
    {
        blockPos = position;
        return this;
    }

    // ####################################################################
    // ####################################################################

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        ElectricityPushButon.TriggerPushButton(blockPos);
    }

    // ####################################################################
    // ####################################################################

    public override void read(PooledBinaryReader _br)
    {
        blockPos = StreamUtils.ReadVector3i(_br);
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        StreamUtils.Write(_bw, blockPos);
    }

    // ####################################################################
    // ####################################################################

    public override int GetLength() => 30;

    // ####################################################################
    // ####################################################################

}
