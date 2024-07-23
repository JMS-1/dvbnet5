# JMS.DVB.NET.HardwareAbstraction

This assembly is the core of the DVB device access using DVB.NET. DVB devices are implementation of abstract **Hardware** class. Some of the features provided by the interface are:

- Tuning to a transponder (frequency) on an origin (e.g. satellite)
- Management of data streams
- Multiple consumers per incoming data stream
- Access to control data streams like NIT, PAT, SDT or EIT
- Decryption
- Support for source and program guide update
- Device wakeup support after coming up from standby or hibernate
- Additional configuration through XML

There are derived abstract classes for alle supported DVB variants - satellite, cable or terrestrial.
