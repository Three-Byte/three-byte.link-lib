# Network Communication Library

## Overview
The Network Communication Library provides a simple and efficient way to handle network communication using TCP, UDP, and Serial protocols. This library is designed to be easy to use and integrate into your projects, offering robust and reliable communication capabilities.

## Features
- **TCP Communication**: Establish and manage TCP connections for reliable data transfer.
- **UDP Communication**: Send and receive data using the UDP protocol for low-latency communication.
- **Serial Communication**: Interface with serial devices for data exchange.
- **Asynchronous Support**: Fully supports asynchronous operations for non-blocking communication.
- **Cross-Platform**: Compatible with .NET Standard 2.1, making it usable across different platforms.



## Installation
To install the library, use the following command in the NuGet Package Manager Console:
```powershell
Install-Package ThreeByte.LinkLib.TcpLink
Install-Package ThreeByte.LinkLib.UdpLink
Install-Package ThreeByte.LinkLib.SerialLink
```

## Usage

### TCP Communication
```csharp
using ThreeByte.LinkLib.TcpLink;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new TcpServer("0.0.0.0", 8080);
        server.Start();
        while (true)
        {
            var clientSocket = await server.AcceptConnectionAsync();
            var data = await server.ReceiveDataAsync(clientSocket);
            Console.WriteLine("Received: " + data);
            await server.SendDataAsync(clientSocket, "Hello, Client!");
            server.CloseConnection(clientSocket);
        }
        server.Stop();
    }
}
```

### UDP Communication
```csharp
using ThreeByte.LinkLib.UdpLink;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new UdpClient("127.0.0.1", 8080);
        await client.SendDataAsync("Hello, Server!");
        var response = await client.ReceiveDataAsync();
        Console.WriteLine("Server response: " + response);
    }
}
```

### Serial Communication
```csharp
using ThreeByte.LinkLib.SerialLink;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var serial = new SerialComm("/dev/ttyUSB0", 9600);
        await serial.OpenAsync();
        await serial.SendDataAsync("Hello, Device!");
        var response = await serial.ReceiveDataAsync();
        Console.WriteLine("Device response: " + response);
        serial.Close();
    }
}
```

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## License
...

## Contact
For any questions or issues, please open an issue on GitHub or contact us at ....
```

Feel free to customize this example to better fit your specific library and its features! If you have any other questions or need further assistance, let me know.