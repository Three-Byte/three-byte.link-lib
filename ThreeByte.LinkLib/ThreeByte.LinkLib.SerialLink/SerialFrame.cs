namespace ThreeByte.LinkLib.SerialLink;

public record class SerialFrame(
    byte[] Header,
    byte[] Footer);
