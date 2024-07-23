# JMS.DVB.NET.Recording

The heart and implementation of the VCR.NET Recording service based on DVB.NET. Actually for DVB device access separate card server instances are used so that this assembly itsself has not dependency on physical devices. In addition to some own algorithms the implementations makes heavy use of the features provided in the DVB.NET ecosystem.

Some of the features included in this assembly are:

- web server runtime environment including a HTTP/REST API with OpenAPI specifications
- support for multiple DVB devices
- planning of single and repeating recordings
- integration of periodical source and program guide update tasks
- streaming of recordings to a UDP receiver - including mutlicasting
- use DVB devices for zapping without recording
- logging support
- highly configurable
- simple FTP server to allow for demuxing recordings - even while still active
