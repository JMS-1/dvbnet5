# JMS.DVB.NET.CardServerCore

The **CardServer** concept is an abstraction above the DVB.NET interfaces to access DVB hardware. There is a protocol based on DVB.NET data structures but this API is intended to be used from any application which not neccessarily uses anything from DVB.NET - for example an application written in Java, JavaScript, Python and so on. Using the API XML requests and responses are sent to the card server which then uses DVB.NET to access the DVB hardware. Typically the card server will be started as a separate application (_OutOfProcessCardServer_) but if the controlling application is .NET as well it can although be created in the same process (_InMemoryCardServer_).

The API set will include a higher level of abstraction as DVB.NET itself and includes a couple of additional features beyond the pure device access:

- Access a DVB.NET device profile.
- Start a recording on this profile.
- Add additonal recordings on the same transponder.
- Stop each individual recording.
- Stream each recording to an UDP receiver - VLC may be used, multicasting is possible.
- Update the program guide - including the UK FreeSAT compressed guide.
- Do a source scan.
- Use the profile for zapping without any recording - so streaming only.

All features are implemented using standard DVB.NET functionality but combined using a single DVB.NET free API.
