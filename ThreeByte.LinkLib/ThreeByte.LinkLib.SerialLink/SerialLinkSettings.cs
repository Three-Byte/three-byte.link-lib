using System.IO.Ports;

namespace ThreeByte.LinkLib.SerialLink;

public class SerialLinkSettings
{
    public string ComPort { get; }
    
    public int BaudRate { get; } = 9600;
    
    public int DataBits { get; } = 8;
    
    public Parity Parity { get; } = Parity.None;

    public SerialLinkSettings(string comPort, int baudRate, int dataBits, Parity parity)
    {
        ComPort = comPort;
        BaudRate = baudRate;
        DataBits = dataBits;
        Parity = parity;
    }
}
