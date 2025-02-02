import * as React from 'react'

import { LogDetails } from './log/details'

import { ILogPage } from '../app/pages/log'
import { Field } from '../common/field'
import { HelpLink } from '../common/helpLink'
import { InlineHelp } from '../common/inlineHelp'
import { ExternalLink } from '../lib.react/command/externalLink'
import { InternalLink } from '../lib.react/command/internalLink'
import { ToggleCommand } from '../lib.react/command/toggle'
import { DetailRow } from '../lib.react/detailRow'
import { SingleSelect } from '../lib.react/edit/list'
import { ComponentWithSite } from '../lib.react/reactUi'

// React.Js Komponente zur Anzeige des Protokolls.
export class Log extends ComponentWithSite<ILogPage> {
    // Oberflächenelemente anlegen.
    render(): React.JSX.Element {
        return (
            <div className='vcrnet-log'>
                Für jede Nutzung eines Gerätes erstellt der VCR.NET Recording Service einen Protokolleintrag
                <HelpLink page={this.props.uvm} topic='log' />, der hier eingesehen werden kann. Bei überlappenden
                Aufzeichnung wird ein einziger Eintrag erstellt, der den gesamten Nutzungszeitraum beschreibt.
                {this.getHelp()}
                <form className='vcrnet-bar'>
                    <Field label={`${this.props.uvm.profiles.text}:`} page={this.props.uvm}>
                        <SingleSelect uvm={this.props.uvm.profiles} />
                        <SingleSelect uvm={this.props.uvm.startDay} />
                        <ToggleCommand uvm={this.props.uvm.showGuide} />
                        <ToggleCommand uvm={this.props.uvm.showScan} />
                        <ToggleCommand uvm={this.props.uvm.showLive} />
                    </Field>
                </form>
                <table className='vcrnet-table'>
                    <thead>
                        <tr>
                            <td className='vcrnet-column-start'>Beginn</td>
                            <td className='vcrnet-column-end'>Ende</td>
                            <td>Quelle</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.uvm.entries.map((e, index) => [
                            <tr key={index}>
                                <td>
                                    <InternalLink view={() => (e.showDetail.value = !e.showDetail.value)}>
                                        {e.start}
                                    </InternalLink>
                                </td>
                                <td>{e.endTime}</td>
                                <td>{e.source}</td>
                            </tr>,
                            e.showDetail.value && (
                                <DetailRow key={`${index}Detail`} dataColumns={3}>
                                    <LogDetails uvm={e} />
                                </DetailRow>
                            ),
                        ])}
                    </tbody>
                </table>
            </div>
        )
    }

    // Hilfe erstellen.
    private getHelp(): React.JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Die Anzeige auf dieser Seite erfolgt immer pro Gerät und Woche, wobei sowohl das Gerät als auch die
                gewünschte Woche über die Auswahllisten vorgegeben werden können. Man beachte dabei, dass gemäß der
                Konfiguration Protokolle nur für einen bestimmten Zeitraum vorgehalten werden, so dass bei der Anzeige
                vergangener Wochen die Liste evntuell leer bleibt.
                <br />
                <br />
                Nach dem Aufruf der Seite werden erst einmal nur die regulären Aufzeichnungen angezeigt. Die Nutzung des
                jeweiligen Gerätes durch Aktualisierungen
                <HelpLink page={this.props.uvm} topic='tasks' /> und den LIVE Zugriff kann durch die entsprechenden
                Schaltflächen neben der Auswahl der Woche eingeblendet werden.
                <br />
                <br />
                Durch Anwahl des jeweiligen Startzeitpunkts eines Protokolleintrags wird die Detailanzeige geöffnet.
                Handelt es sich bei der Nutzung des Geräte um eine oder mehrere reguläre Aufzeichnungen, so sind mit
                dieser eventuell noch nicht gelöschte Aufzeichnungsdateien verbunden.
                <HelpLink page={this.props.uvm} topic='filecontents' /> Durch Anwahl des jeweiligen Symbols können diese
                zur Anzeige durch den{' '}
                <ExternalLink url='http://www.psimarron.net/DVBNETViewer/html/vcrfile.html'>
                    DVB.NET / VCR.NET Viewer
                </ExternalLink>{' '}
                abgerufen werden.
            </InlineHelp>
        )
    }
}
