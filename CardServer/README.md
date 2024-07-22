# JMS.DVB.NET.CardServer

The **CardServer** is essentially a very small application based on a separate assembly in which all the necessary processes are implemented. For each recording, generally a separate instance of the application is started and communicates with the controlling process using anonymous pipes. This isolates the execution of one recording from potentially parallel recordings and prevents serious errors in one recording from adversely affecting the others.
