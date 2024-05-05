import * as React from 'react'

import { HelpComponent } from './helpComponent'

import { IPage } from '../../app/pages/page'
import { ExternalLink } from '../../lib.react/command/externalLink'
import { InternalLink } from '../../lib.react/command/internalLink'
import { Pictogram } from '../../lib.react/command/pictogram'

export class Log extends HelpComponent {
    readonly title = 'Protokolle'

    render(page: IPage): JSX.Element {
        return (
            <div>
                Der VCR.NET Recording Service erstellt für jede ausgeführte Aktivität
                <InternalLink pict='devices' view={page.application.devicesPage.route} /> einen gesonderten
                Protokolleintrag. Eine Aktivität ist hierbei immer ein geschlossener Nutzungszeitraum eines DVB Gerätes.
                In diesem Zeitraum kann entweder eine einzelne Sonderaufgabe oder eine{' '}
                <InternalLink view={`${page.route};parallelrecording`}>Gruppe von Aufzeichnungen</InternalLink>{' '}
                ausgeführt worden sein. Die{' '}
                <InternalLink view={page.application.logPage.route}>Protokolleinträge</InternalLink> können direkt über
                die Web Oberfläche abgerufen werden.
                <br />
                <br />
                In der Liste sieht man immer die Einträge einer Woche für ein einzelnes DVB Geräte. Über Auswahllisten
                ist es möglich, sowohl den Zeitraum als auch das betrachtete Gerät festzulegen. Die Konfiguration
                <InternalLink pict='admin' view={`${page.application.adminPage.route};other`} /> des VCR.NET Recording
                Service sieht vor, dass veraltete Protokolleinträge automatisch entfernt werden. Man beachte aber, dass
                dies nicht periodisch geschieht, sondern nur bei Aufruf der Liste über die Web Oberfläche.
                <br />
                <br />
                Erst einmal werden nur die Protokolleinträge angezeigt, die von Aufzeichnungen erzeugt wurden. Auf
                Wunsch ist es möglich, auch die Einträge der{' '}
                <InternalLink view={`${page.route};tasks`}>Sonderaufgaben</InternalLink> hinzu zu schalten - einzeln pro
                Art der Aufgabe.
                <br />
                <br />
                Wählt man einen einzelnen Eintrag an, so wird eine Detailanzeige geöffnet. In dieser lässt sich für
                Aufzeichnungen zum Beispiel die gesamte Größe aller zugehörigen Aufzeichnungsdateien sowie Beginn und
                Ende der Aktivität über alle enthaltenen Aufzeichnungen ablesen.
                <br />
                <br />
                Sind die <InternalLink view={`${page.route};filecontents`}>Aufzeichnungsdateien</InternalLink> zu einem
                Eintrag noch vorhanden, so wird dies durch kleine Symbole <Pictogram name='recording' /> bei dem Eintrag
                visualisiert. Ist auf dem Rechner, von dem die Web Oberfläche aufgerufen wurde, der{' '}
                <ExternalLink url='http://www.psimarron.net/DVBNETViewer/'>DVB.NET / VCR.NET Viewer</ExternalLink> lokal
                installiert, so kann durch Klicken auf ein Symbol die zugehörige Aufzeichnungsdatei zur Anzeige geöffnet
                werden.
            </div>
        )
    }
}
