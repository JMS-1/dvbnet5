# VCR.NET Recording Service 5

Actually this project is kind of a successor of the [VCR.NET / DVB.NET Project](https://github.com/JMS-1/DVB.NET---VCR.NET) _just_ ported to .NET Core. The orginial project was created to use under Microsoft Windows only and after the port the target operating system is primary linux. Most access to Windows DVB devices had to be removed and only a special access using a dedicated TCP API remains to access Linux DVB devices.

As before the project splits into the [VCR.NET Recording Service](Recording/README.md) which heavily depends on the implementation of the DVB.NET library:

- [Configuration access](Common/README.md)
- [DVB program description](SourceManagement/README.md)
- [Low level DVB device access abstraction](HardwareAbstraction/README.md)
- [Transort stream analysers](TS/README.md)
- [Scheduling algorithms](Algorithms/README.md)
- [Program guide support](EPG/README.md)
- [DVB control table helper](SITables/README.md)
- [DVB device implementation base classes](DVBNETLegacy/README.md)
- [Linux DVB device access proxy](Ubuntu/README.md)
- [Higher level DVB device access abstractions](CardServerCore/README.md)
- [Out of process access to DVB devices](CardServer/README.md)

Additionally to a HTTP/REST API a [Web Application](WebClient/README.md) is provided. The application allows access to all features included in the HTTP/REST API.

The VCR.NET recording service is designed to be [executed](WebServer/README.md) as a [docker container](Deploy/README.md).
