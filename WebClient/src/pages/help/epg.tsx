﻿import * as React from 'react'

import { HelpComponent } from './helpComponent'

import { IPage } from '../../app/pages/page'
import { InternalLink } from '../../lib.react/command/internalLink'

export class ProgramGuide extends HelpComponent {
    readonly title = 'Die Programmzeitschrift'

    render(page: IPage): React.JSX.Element {
        return (
            <div>
                Die elektronische Programmzeitschrift (EPG) ist ein optionales Leistungsmerkmal, dass von vielen
                Sendeanstalten angeboten wird. Zusätzlich zur digitalen Ausstrahlung von Bild und Ton werden
                Informationen übermittelt, welche Sendungen in nächster Zeit auf welcher Quelle (Radio- oder
                Fernsehsender) ausgestrahlt werden. Im Allgemeinen sind in diesen Informationen auch kurze
                Beschreibungen der Inhalte vorhanden, einige Sender beschränken sich aber auf den Titel und den
                Sendezeitraum. Genauso unterschiedlich wie der Umfang des Inhalts ist auch die zeitliche Vorschau:
                zurzeit wird für die Sender der ARD eine Vorschau von etwa 4 Wochen angeboten, die Sender rund um das
                ZDF beschränken sich auf eine Woche und bei einigen Privatsendern sind es nur wenige Tage.
                <br />
                <br />
                Der VCR.NET Recording Service kann dazu{' '}
                <InternalLink view={`${page.route};epgconfig`}>konfiguriert</InternalLink> werden, für eine ausgewählte
                Liste von Quellen die Programmzeitschrift periodisch zu aktualisieren - üblich ist ein oder zweimal am
                Tag. Auf Basis dieser Daten lassen sich dann Aufzeichnungen programmieren. Zur Auswahl
                <InternalLink pict='guide' view={page.application.guidePage.route} /> stehen dabei erst einmal alle
                Informationen aller Quellen aller DVB Geräte, die der VCR.NET Recording Service verwenden darf. Um die
                richtige Sendung zu finden gibt es neben der Inhaltssuche auf einen Freitext weitere
                Filtermöglichkeiten. So ist es möglich, sich auf die Sendungen einer einzelnen Quelle zu beschränken
                oder alle Sendung ab einer bestimmten Uhrzeit eines bestimmten Tages anzeigen zu lassen.
                <br />
                <br />
                Die Programmierung über die Programmzeitschrift ist ansonsten einfach gehalten. Normalerweise wird nach
                der Auswahl eine neue Aufzeichnung angelegt - auf Wunsch kann auch eine Aufzeichnung zu einem
                existierenden Auftrag ergänzt werden. Die Zeiten aus der Programmzeitschrift werden um Vor- und
                Nachlaufzeiten gemäß den Voreinstellungen
                <InternalLink pict='settings' view={page.application.settingsPage.route} /> des Anwenders erweitert. Die
                eigentliche Programmierung muss noch einmal bestätigt werden, wobei dann auch noch Detailänderungen
                möglich sind - zum Beispiel um direkt eine{' '}
                <InternalLink view={`${page.route};repeatingschedules`}>Serienaufzeichnung</InternalLink> anzulegen.
            </div>
        )
    }
}
