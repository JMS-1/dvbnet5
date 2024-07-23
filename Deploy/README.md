# Deployment

The VCR.NET recording service is provided in a pre-built for as a [docker container](https://downloads.psimarron.net/vcrnet5.docker) - based on the _mcr.microsoft.com/dotnet/aspnet_ image. To run the service quite a bit of one time configuration is necessary - this is **not** a complete documentation and if you really want to use the VCR.NET Recording Service 5 feel free to contact me:

- A copy of the [core configuration](config.template) must be created locally.
- The configuration assumes that the DVB devices on the computer running the docker container should be accessed - using the DVB.NET TCP proxy. If the proxy is running on another computer the parameter **Adapter.Server** must be updated accordingly.
- The default configuration is limited to 5 DVB devices - if more are needed additional _dnp_ files must be created.
- If unchanged the default configuration uses the [Astra satelite on 19.2° east](config.template/DVBNETProfiles/card1.dnp).
- The [start configuration](starter.sample) must be updated to the local environment.
- The service and be accessed unencrypted and unprotected or SSL encrypted and password protected using the HTTP basic protocol. In the default configuration both options are activated - to use the secure access a SSL zertificate and password file have to be provided. You may want to disable either mode - for the [docker container](docker-compose.yml) and the [nginx reverse proxy](default.conf). For the regular use in the local secure intranet just remove the nginx service from the docker configuration.
