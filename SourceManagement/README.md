# JMS.DVB.NET.SourceManagement

Defined the data models DVB.NET uses to describe DVB receiption. There are abstractions like locations (e.g. a satellite), groups (e.g. a single transponder) and sources (i.e. a program feed). In addition for each supported variant (satellite, cable or terrestrial) there are dedicated implmentation classes.

Furthermore the assembly includes models to describe stream selection of a single program feed, e.g. which audio channels should be used. Typically these descriptions do not rely on specific stream identifiers bust on a higher level abstraction - e.g. to include an AC3 dolby digital audio stream in a recording. The DVB.NET runtime will use these settings to identify the actual stream identifiers.

All data models are designed to be XML serializable.
