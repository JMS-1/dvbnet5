import * as React from 'react'

import { Task } from './home/task'
import { VersionCheck } from './home/versionCheck'

import { IHomePage } from '../app/pages/home'
import { HelpLink } from '../common/helpLink'
import { ExternalLink } from '../lib.react/command/externalLink'
import { InternalLink } from '../lib.react/command/internalLink'
import { Pictogram } from '../lib.react/command/pictogram'
import { ComponentWithSite } from '../lib.react/reactUi'

// Die React.Js Komponente zur Anzeige der Startseite.
export class Home extends ComponentWithSite<IHomePage> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        const versionCheck = this.props.uvm.checkVersion
        const showGuide = this.props.uvm.showStartGuide
        const showScan = this.props.uvm.showStartScan
        const application = this.props.uvm.application

        return (
            <div className='vcrnet-home'>
                <div>
                    Willkommen zur Benutzeroberfläche des VCR.NET Recording Service. Von hier aus geht es direkt zu:
                    <ul>
                        <li>
                            <InternalLink pict='plan' view={application.planPage.route}>
                                dem Aufzeichnungsplan
                            </InternalLink>{' '}
                            mit den anstehenden Aufzeichnungen
                            <HelpLink page={this.props.uvm} topic='parallelrecording' />
                        </li>
                        <li>
                            <InternalLink pict='devices' view={application.devicesPage.route}>
                                den laufenden Aufzeichnungen
                            </InternalLink>{' '}
                            mit den Aktivitäten der einzelnen DVB Geräte
                        </li>
                        <li>
                            <InternalLink pict='guide' view={application.guidePage.route}>
                                der Programmzeitschrift
                            </InternalLink>{' '}
                            zum Anlegen neuer Aufzeichnungen
                            <HelpLink page={this.props.uvm} topic='epg' />
                        </li>
                        <li>
                            <InternalLink pict='new' view={application.editPage.route}>
                                einer neuen Aufzeichnung
                            </InternalLink>
                        </li>
                    </ul>
                    <ul>
                        <li>
                            <InternalLink pict='jobs' view={application.jobPage.route}>
                                den vorhandenen Aufzeichnungen
                            </InternalLink>
                            , um diese zu verändern oder ins Archiv zu übertragen
                        </li>
                        <li>
                            <InternalLink view={`${application.jobPage.route};archive`}>
                                den archivierten Aufzeichnungen
                            </InternalLink>
                            , um diese anzusehen, zu verändern, zu reaktivieren oder endgültig zu löschen
                            <HelpLink page={this.props.uvm} topic='archive' />
                        </li>
                        <li>
                            <InternalLink view={application.logPage.route}>den Protokollen</InternalLink> von bereits
                            durchgeführten Aufzeichnungen
                            <HelpLink page={this.props.uvm} topic='log' />
                        </li>
                    </ul>
                    <ul>
                        <li>
                            <InternalLink pict='settings' view={application.settingsPage.route}>
                                den individuellen Anpassungen
                            </InternalLink>{' '}
                            der Web Oberfläche
                        </li>
                    </ul>
                </div>
                <div>
                    Je nach den zugeteilten Benutzerrechten können Sie darüber hinaus folgende administrative
                    Tätigkeiten wahrnehmen:
                    <ul>
                        <li>
                            {showGuide.isReadonly ? (
                                showGuide.text
                            ) : (
                                <InternalLink view={() => (showGuide.value = !showGuide.value)}>
                                    {showGuide.text}
                                </InternalLink>
                            )}
                            <HelpLink page={this.props.uvm} topic='epgconfig' />
                        </li>
                        {showGuide.value && (
                            <Task uvm={this.props.uvm.startGuide}>
                                Mit der Schaltfläche unter diesem Text kann eine baldmögliche Aktualisierung der
                                Programmzeitschrift
                                <HelpLink page={this.props.uvm} topic='epg' /> angefordert werden. Sind gerade
                                Aufzeichnungen aktiv oder in nächster Zeit geplant, so wird der VCR.NET Recording
                                Service die Aktualisierung auf den nächstmöglichen Zeitpunkt verschieben, da die
                                Ausführung regulärer Aufzeichnungen immer Priorität vor allen Aktualisierungen hat.
                                <HelpLink page={this.props.uvm} topic='tasks' />
                            </Task>
                        )}
                        <li>
                            {showScan.isReadonly ? (
                                showScan.text
                            ) : (
                                <InternalLink view={() => (showScan.value = !showScan.value)}>
                                    {showScan.text}
                                </InternalLink>
                            )}
                            <HelpLink page={this.props.uvm} topic='psiconfig' />
                        </li>
                        {showScan.value && (
                            <Task uvm={this.props.uvm.startScan}>
                                Hier ist es nun möglich, die Aktualisierung der Quellen der vom VCR.NET Recording
                                Service verwalteten DVB.NET Geräte anzufordern. Da die Aktualisierung der Quellen eine
                                niedrigere Priorität besitzt als die Ausführung regulärer Aufzeichnungen kann es sein,
                                dass sie nicht unmittelbar gestartet wird. Der VCR.NET Recording Service wird dies aber
                                bei nächster Gelegenheit nachholen.
                                <HelpLink page={this.props.uvm} topic='tasks' />
                            </Task>
                        )}
                        <li>
                            prüfen, ob inzwischen eine{' '}
                            <InternalLink view={() => (versionCheck.value = !versionCheck.value)}>
                                neuere Version
                            </InternalLink>{' '}
                            des VCR.NET Recording Service angeboten wird
                        </li>
                        {versionCheck.value && <VersionCheck uvm={this.props.uvm} />}
                        <li>
                            {this.props.uvm.showAdmin ? (
                                <InternalLink pict='admin' view={application.adminPage.route}>
                                    die Konfiguration des VCR.NET Recording Service verändern
                                </InternalLink>
                            ) : (
                                'die Konfiguration des VCR.NET Recording Service verändern'
                            )}
                        </li>
                    </ul>
                </div>
                {this.props.uvm.isRecording && (
                    <div className='vcrnet-warningtext'>
                        Hinweis: Der VCR.NET Recording Service führt gerade eine oder mehrere Aufzeichnungen oder
                        Aktualisierungen von Programmzeitschrift respektive Senderliste aus.
                        <InternalLink pict='info' view={this.props.uvm.application.devicesPage.route} />
                    </div>
                )}
                <div>
                    Weitere Informationen zum VCR.NET Recording Service findet man hier im Bereich der{' '}
                    <InternalLink view={`${application.helpPage.route};overview`}>Fragen &amp; Antworten</InternalLink>,
                    auf der <ExternalLink url='http://www.psimarron.net/vcrnet'>Homepage im Internet</ExternalLink> oder
                    im{' '}
                    <ExternalLink url='http://www.watchersnet.de/Default.aspx?tabid=52&g=topics&f=17'>
                        offiziellen Forum
                    </ExternalLink>
                    .
                </div>
                <div className='vcrnet-home-copyright'>
                    <ExternalLink url='http://www.psimarron.net'>
                        <Pictogram name='psimarron' />
                    </ExternalLink>
                    <span>Dr. Jochen Manns, 2003-19</span>
                </div>
            </div>
        )
    }
}
