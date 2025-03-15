# Network Communication Library

## Overview
The Network Communication Library provides a simple and efficient way to handle network communication using TCP, UDP, and Serial protocols. This library is designed to be easy to use and integrate into your projects, offering robust and reliable communication capabilities. /

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

## How to Build and Publish NuGet Packages using pipelines

To build and publish NuGet packages, follow these steps:
1. Create a new GitHub branch and switch to it
2. Update the source code in any of the following `ThreeByte.LinkLib` folders:
- ThreeByte.LinkLib.SerialLink
- ThreeByte.LinkLib.TcpLink
- ThreeByte.LinkLib.UdpLink

**Note:** The pipeline will only be triggered when changes are made in these folders

3. Commit and push your changes to the GitHub repository.
4. Create a pull request to the main branch and attach one of the following labels based on the type of change you made:
- major
- minor
- patch

**Note:** The version of the new NuGet package is determined by the attached label. We follow [Semantic Versioning](https://semver.org/)

5. Merge your pull request
6. Wait approximately 5 minutes — your NuGet package will then be available on [nuget.org](https://www.nuget.org/profiles/olaaf)

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request with your changes.

## License
...

## Contact
For any questions or issues, please contact us at `support@mail`.
```
Feel free to customize this example to better fit your specific library and its features!
If you have any other questions or need further assistance, let me know.
```
