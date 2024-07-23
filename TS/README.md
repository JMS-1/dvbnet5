# JMS.DVB.NET.TS

In this assembly anything around analyising data streams of a transport stream are provided - for some situations stream builders as well. In most cases only the **TSParser** class is used to retrieve and dispatch content from a transport stream - data and control as well.

The implementation does not depend on DVB devices at all. It may be used to analyse transport stream files in the same way as a DVB transport stream incoming live from a DVB device.

The dispatching feature allows to write data stream directly to disk files using a double buffering layer to support higher data rates. It is possible to send incoming data to an UDP receipient like VLC - even in addition to wrting to a file.
