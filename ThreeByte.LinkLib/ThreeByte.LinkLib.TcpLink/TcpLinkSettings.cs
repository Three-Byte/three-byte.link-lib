namespace ThreeByte.LinkLib.TcpLink
{
    public class TcpLinkSettings
    {
        public string Address { get; }

        public int Port { get; }

        public TcpLinkSettings(string address, int port)
        {
            Address = address;
            Port = port;
        }
    }
}
