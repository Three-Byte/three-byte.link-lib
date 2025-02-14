namespace ThreeByte.LinkLib.SerialLink
{
    public class SerialFrame
    {
        public byte[] Header { get; set; } = null!;
        public byte[] Footer { get; set; } = null!;
    }
}
