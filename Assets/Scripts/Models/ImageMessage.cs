using Mirror;

public struct ImageMessage : NetworkMessage
{
    public uint netId;
    public byte[] bytes;
    public bool hasLoaded { get { return bytes != null && bytes.Length != 0; } }
}
