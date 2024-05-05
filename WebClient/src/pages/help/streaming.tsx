import * as React from 'react'

import { HelpComponent } from './helpComponent'
import { ScreenShot } from './screenShot'

import { IPage } from '../../app/pages/page'
import { ExternalLink } from '../../lib.react/command/externalLink'
import { InternalLink } from '../../lib.react/command/internalLink'

export class Streaming extends HelpComponent {
    readonly title = 'Netzwerkversand'

    render(page: IPage): JSX.Element {
        return (
            <div>
                Laufen Aufzeichnungen
                <InternalLink pict='devices' view={page.application.devicesPage.route} />, so erlaubt es der VCR.NET
                Recording Service auch, die{' '}
                <InternalLink view={`${page.route};filecontents`}>Aufzeichnungsdateien</InternalLink> an eine
                Netzwerkadresse zu senden. Übertragen wird dabei genau der Inhalt der Aufzeichnungsdatei. Dieses Format
                wird außer von dem{' '}
                <ExternalLink url='http://www.psimarron.net/DVBNETViewer/'>DVB.NET / VCR.NET Viewer</ExternalLink> auch
                von anderen Media Player Anwendungen wie <ExternalLink url='http://www.videolan.org/'>VLC</ExternalLink>{' '}
                verstanden und kann zur Anzeige verwendet werden. Bei Benutzung des DVB.NET / VCR.NET Viewers übernimmt
                dieser alle notwendigen Maßnahmen zur Steuerung des Netzwerkversands.
                <ScreenShot description='Netzwerkversand' name='faq/sendtonetwork' />
                Ist auf dem Rechner, von dem aus die Web Oberfläche aufgerufen wurde, der DVB.NET / VCR.NET Viewer lokal
                installiert, so kann dieser über den ersten Verweis gestartet werden, wobei die Wiedergabe der aktuellen
                Aufzeichnung erfolgt. Der zweite Verweis handelt analog, startet allerdings die Wiedergabe am Anfang der
                Aufzeichnungsdatei - im Viewer ist es im Allgemeinen über die{' '}
                <ExternalLink url='http://www.psimarron.net/DVBNETViewer/html/vcrcurrent.html'>
                    Stern-Taste
                </ExternalLink>{' '}
                auf dem Nummernfeld der Tastatur möglich zwischen diesen Anzeigealternativen zu wechseln.
                <br />
                <br />
                Werden die Daten einer Aufzeichnung gerade versendet, so zeigt der VCR.NET Recording Service wie oben im
                Bild die Netzwerkadresse des Empfängers der Daten an.
                <br />
                <br />
                Man beachte, dass für einen Empfang der über Netzwerk versendeten Daten im Allgemeinen eine
                Systemkonfiguration vorgenommen werden muss, wenn der VCR.NET Recording Service nicht auf dem Rechner
                läuft, der auch die empfangenen Daten darstellen soll. Dies betrifft vor allem die Einstellungen der
                Firewalls auf allen beteiligten Netzwerkkomponenten.
            </div>
        )
    }
}
