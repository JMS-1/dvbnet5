# JMS.DVB.NET.DVBNETLegacy

Originally DVB.NET only supported DVB devices under Microsoft Windows. The first implmentation was a proprietary API to a very special card. Later on DVB.NET was extended to use the Windows BDA standard API which became the standard DVB.NET device interface. The original implementation then was called **legacy**.

Starting with DVB.NET version 5 no Windows DVB device is supported - neither PDA nor proprietary. In fact there is no longer any physical DVB device access implemented into DVB.NET any longer. Currently the only implementation uses a special DVB.NET TCP based protocol to community with external processes controlling the physical DVB hardware - this control process may run on the same computer as the application using DVB.NET or a remote system.

The abstract DVB.NET device implementations in this library provides the only possibility to implement concrete DVB.NET devices on top. So it actually is the link between a DVB.NET application and any DVB hardware.
