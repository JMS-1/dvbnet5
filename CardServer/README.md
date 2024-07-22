# JMS.DVB.NET.CardServer

Der **CardServer** ist im Kern eine sehr kleine Anwendung, die auf einer gesonderten Assembly basiert, in der alle notwendigen Abläufe implmentiert sind. Für jede Aufzeichnung wird im Allgemeine eine gesonderte Instanz der Anwendung gestartet und mit dieser über anonyme Pipes kommuniziert. Dadurch wird die Ausführung einer Aufzeichnungen von möglicherweise parallen Aufzeichnungen isoliert und schwere Fehler in einer Aufzeichnung beeinflussen die anderen nicht negativ.
