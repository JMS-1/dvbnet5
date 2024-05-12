import * as React from 'react'

import { DeviceControl } from './devices/control'
import { Device } from './devices/entry'
import { DeviceGuide } from './devices/guide'

import { IDevicesPage } from '../app/pages/devices'
import { HelpLink } from '../common/helpLink'
import { InlineHelp } from '../common/inlineHelp'
import { ExternalLink } from '../lib.react/command/externalLink'
import { InternalLink } from '../lib.react/command/internalLink'
import { Pictogram } from '../lib.react/command/pictogram'
import { DetailRow } from '../lib.react/detailRow'
import { ComponentWithSite } from '../lib.react/reactUi'

// React.Ks Komponente zur Anzeige der Geräteübersicht.
export class Devices extends ComponentWithSite<IDevicesPage> {
    // Oberflächenelemente erstellen.
    render(): JSX.Element {
        return (
            <div className='vcrnet-devices'>
                Hier werden alle Aufzeichnungen und Aufgaben aufgelistet, die zum aktuellen Zeitpunkt auf irgendeinem
                Gerät ausgeführt werden. Für alle Geräte, die gerade keine Aufzeichnung oder Aufgaben ausführen, wird
                die jeweils nächste Aufzeichnung angezeigt - dabei werden allerdings periodische Aufgaben
                <HelpLink page={this.props.uvm} topic='tasks' /> wie etwa die Aktualisierung der Programmzeitschrift
                nicht berücksichtigt.
                {this.getHelp()}
                <table className='vcrnet-table'>
                    <thead>
                        <tr>
                            <td className='vcrnet-column-mode'>&nbsp;</td>
                            <td className='vcrnet-column-start'>Beginn</td>
                            <td className='vcrnet-column-end'>Ende</td>
                            <td>Quelle</td>
                            <td>Name</td>
                            <td>Gerät</td>
                            <td>Größe</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.uvm.infos.map((i, index) => [
                            <Device key={index} page={this.props.uvm} uvm={i} />,
                            i.showGuide.value && (
                                <DetailRow key={`${index}Guide`} dataColumns={6} prefixColumns={1}>
                                    <DeviceGuide uvm={i} />
                                </DetailRow>
                            ),
                            i.showControl.value && (
                                <DetailRow key={`${index}Control`} dataColumns={6} prefixColumns={1}>
                                    <DeviceControl page={this.props.uvm} uvm={i.controller} />
                                </DetailRow>
                            ),
                        ])}
                    </tbody>
                </table>
            </div>
        )
    }

    // Beschreibung zur Funktionalität.
    private getHelp(): JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Zur Anzeige werden die Aktivitäten aller Geräte ermittelt, die der VCR.NET Recording Service verwenden
                darf
                <HelpLink page={this.props.uvm} topic='devices' />. Führt ein Gerät gerade eine oder mehrere
                Aufzeichnungen oder Aufgaben aus, so werden diese einzeln angezeigt. Ist ein Gerät nicht in Benutzung,
                so wird die nächste Aufzeichnung angezeigt, die für dieses Gerät vorgesehen ist. Sollte in den nächsten
                Monaten keine Aufzeichnung geplant sein, so erscheint in diesem Fall kein Eintrag für das Gerät. Im
                Extremfall kann die Liste auch ganz leer bleiben, obwohl Geräte zur Nutzung durch den VCR.NET Recording
                Service freigegeben sind und Aufzeichnung existieren, die aber erst in ferner Zukunft beginnen.
                <br />
                <br />
                Die einzelnen Aktivitäten werden in der Liste nach dem Beginn der jeweiligen Aufzeichnung oder der
                zugehörigen Aufgabe sortiert. Das Symbol ganz links beschreibt dabei die Aufzeichnungsplanung:
                <br />
                <table>
                    <tbody>
                        <tr>
                            <td>
                                <Pictogram name='running' />
                            </td>
                            <td>Die Aufzeichnung oder Aufgabe wird in diesem Moment ausgeführt.</td>
                        </tr>
                        <tr>
                            <td>
                                <Pictogram name='intime' />
                            </td>
                            <td>Eine zukünftige Aufzeichnung wird wie programmiert ausgeführt.</td>
                        </tr>
                        <tr>
                            <td>
                                <Pictogram name='late' />
                            </td>
                            <td>Eine zukünftige Aufzeichnung beginnt verspätet, eventuell fehlt der Anfang.</td>
                        </tr>
                        <tr>
                            <td>
                                <Pictogram name='null' />
                            </td>
                            <td>
                                Die Aufzeichnung oder Aufgabe wird gerade beendet - dies kann einige Sekunden dauern.
                            </td>
                        </tr>
                    </tbody>
                </table>
                <br />
                Wenn gerade eine Aufzeichnung oder Aufgabe ausgeführt wird, so kann das zugehörige Symbol
                <Pictogram name='running' /> ausgewählt werden um den Endzeitpunkt dieser Aktivtät zu verändern - oder
                sie gänzlich zu beenden
                <HelpLink page={this.props.uvm} topic='currentstream' />. Handelt es sich bei der Aktivität um eine
                Aufzeichnung, so ist es zusätzlich möglich diese LIVE oder zeitversetzt mit dem{' '}
                <ExternalLink url='http://www.psimarron.net/DVBNETViewer/html/vcrcurrent.html'>
                    DVB.NET / VCR.NET Viewer
                </ExternalLink>{' '}
                zu betrachten - sofern dieser lokal installiert ist und geeignet konfiguriert wurde
                <HelpLink page={this.props.uvm} topic='streaming' />.
                <br />
                <br />
                Sollte zu einer Aufzeichnung ein Eintrag in der Programmzeitschrift
                <InternalLink pict='guide' view={this.props.uvm.application.guidePage.route} /> existieren, so kann
                dieser durch Auswahl des Verweises auf dem Startzeitpunkt eingeblendet werden - angezeigt wird der
                Eintrag, der am besten zum gesamten Aufzeichnungszeitraum passt. Ebenso ist es bei einem Eintrag zu
                einer Aufzeichnung über den Verweis auf dem Namen der Aufzeichnung möglich, die zugehörigen
                Einstellungen direkt zu verändern.
                <br />
                <br />
                Führt ein Gerät gerade eine oder mehrere
                <HelpLink page={this.props.uvm} topic='parallelrecording' /> Aufzeichnungen aus, so wird als Größe die
                Gesamtzahl der seit dem Beginn der Gerätenutzung aufgezeichneten Bytes angezeigt. Diese Zahl enthält
                nicht nur die Größe aller aktuellen Aufzeichnungen, sondern schliesst vielmehr auch die Aufzeichnungen
                ein, die bereits abgeschlossen wurden. Der hier angezeigte Wert erscheint nach dem Beenden der
                Gerätenutzung auch im Protokoll
                <HelpLink page={this.props.uvm} topic='log' />.
            </InlineHelp>
        )
    }
}
