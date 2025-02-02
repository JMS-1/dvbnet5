import * as React from 'react'

import { HelpComponent } from './helpComponent'

import { IPage } from '../../app/pages/page'
import { InternalLink } from '../../lib.react/command/internalLink'

export class Configuration extends HelpComponent {
    readonly title = 'Sonstige Konfiguration'

    render(page: IPage): React.JSX.Element {
        return (
            <div>
                Der VCR.NET Recording Service bietet auch zusätzliche Konfigurationsmöglichkeiten an
                <InternalLink pict='admin' view={page.application.adminPage.route} />, die hier kurz vorgestellt werden
                sollen.
                <br />
                <br />
                Nach der Installation darf jeder Benutzer nicht nur Aufzeichnungen programmieren sondern auch sämtliche
                Konfigurationsoptionen nutzen. Mit Hilfe von Windows Benutzergruppen ist es aber möglich festzulegen
                <InternalLink pict='admin' view={`${page.application.adminPage.route};security`} />, welche Anwender
                Aufzeichnungen programmieren dürfen und welchen Anwendern zusätzlich die Konfiguration des VCR.NET
                Recording Service gestattet wird.
                <br />
                <br />
                Es empfiehlt sich unbedingt mindestens ein Verzeichnis zu bestimmen
                <InternalLink pict='admin' view={`${page.application.adminPage.route};directories`} />, in dem die
                Aufzeichnungsdateien abgelegt werden - nach der Installation wird ein Unterverzeichnis des
                Installationsverzeichnisses verwendet, was fast immer unerwünscht ist. Es können mehrere Verzeichnisse
                angegeben werden und mit der Auswahl eines Verzeichnisses werden auch alle Unterverzeichnisse zur Ablage
                freigeschaltet. Wenn keine gesonderte Auswahl bei der{' '}
                <InternalLink view={`${page.route};jobsandschedules`}>Programmierung einer Aufzeichnung</InternalLink>{' '}
                erfolgt, so verwendet der VCR.NET Recording Service immer das erste angegebene Verzeichnis.
                <br />
                <br />
                Sind auf dem Rechner mehrere DVB Geräte vorhanden, so kann es dem VCR.NET Recording Service gestattet
                werden
                <InternalLink pict='admin' view={`${page.application.adminPage.route};devices`} />, eine beliebige
                Anzahl davon zu verwenden. Dazu werden die entsprechenden DVB.NET Geräteprofile in einer Auswahlliste
                markiert und eines davon als Voreinstellung ausgezeichnet.
                <br />
                <br />
                Die Web Oberfläche des VCR.NET Recording Service ist üblicherweise über den Standard Web Server Port 80
                zu erreichen. Dieser kann an die eigenen Bedürfnissen angepasst werden
                <InternalLink pict='admin' view={`${page.application.adminPage.route};other`} /> - man beachte
                allerdings, dass eine Umschaltung einen erneuten Aufruf des Browsers mit dem veränderten Port
                erforderlich macht.
                <br />
                <br />
                Zusätzlich ist es möglich, den Umfang der Protokollierung einzustellen
                <InternalLink pict='admin' view={`${page.application.adminPage.route};other`} />, die der VCR.NET
                Recording Service im Ereignisprokoll von Windows (Bereich Anwendung) vornimmt.
            </div>
        )
    }
}
