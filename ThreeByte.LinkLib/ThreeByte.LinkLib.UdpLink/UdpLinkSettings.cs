namespace ThreeByte.LinkLib.UdpLink;

public record class UdpLinkSettings
{
    public string Address { get; }

    public int RemotePort { get; }

    public int LocalPort { get; }

    public UdpLinkSettings(string address, int remotePort)
        : this(address, remotePort, 0)
    {
    }

    public UdpLinkSettings(string address, int remotePort, int localPort)
    {
        Address = address;
        RemotePort = remotePort;
        LocalPort = localPort;
    }
}
