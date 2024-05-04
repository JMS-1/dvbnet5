import * as React from 'react'
import { IPlanPage } from '../app/pages/plan'
import { HelpLink } from '../common/helpLink'
import { InlineHelp } from '../common/inlineHelp'
import { InternalLink } from '../lib.react/command/internalLink'
import { Pictogram } from '../lib.react/command/pictogram'
import { ToggleCommand } from '../lib.react/command/toggle'
import { DetailRow } from '../lib.react/detailRow'
import { SingleSelectButton } from '../lib.react/edit/buttonList'
import { ComponentWithSite } from '../lib.react/reactUi'
import { PlanException } from './plan/exception'
import { PlanGuide } from './plan/guide'
import { PlanRow } from './plan/row'

// React.Js Komponente zur Anzeige des Aufzeichnungsplans.
export class Plan extends ComponentWithSite<IPlanPage> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        var jobs = this.props.uvm.jobs

        return (
            <div className='vcrnet-plan'>
                Hier sieht man einen Ausschnitt der geplanten Aufzeichnungen für die nächsten Wochen.
                <HelpLink page={this.props.uvm} topic='parallelrecording' />
                {this.getHelp()}
                <div className='vcrnet-plan-filter vcrnet-bar'>
                    <SingleSelectButton uvm={this.props.uvm.startFilter} merge={true} />
                    <ToggleCommand uvm={this.props.uvm.showTasks} />
                </div>
                {jobs && (
                    <table className='vcrnet-table'>
                        <thead>
                            <tr>
                                <td className='vcrnet-column-mode'>&nbsp;</td>
                                <td className='vcrnet-column-start'>Beginn</td>
                                <td className='vcrnet-column-end'>Ende</td>
                                <td>Quelle</td>
                                <td>Name</td>
                                <td>&nbsp;</td>
                                <td>Gerät</td>
                            </tr>
                        </thead>
                        <tbody>
                            {jobs.map((job, index) => [
                                <PlanRow uvm={job} key={index} />,
                                job.showEpg && (
                                    <DetailRow prefixColumns={1} dataColumns={6} key={`${index}Details`}>
                                        <PlanGuide uvm={job} page={this.props.uvm} />
                                    </DetailRow>
                                ),
                                job.showException && (
                                    <DetailRow prefixColumns={1} dataColumns={6} key={`${index}Exceptions`}>
                                        <PlanException uvm={job.exception} page={this.props.uvm} />
                                    </DetailRow>
                                ),
                            ])}
                        </tbody>
                    </table>
                )}
            </div>
        )
    }

    // Hilfe erstellen.
    private getHelp(): JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Über die Datumsauswahl im linken Bereich kann der zeitliche Anfang des angezeigten Ausschnitts
                festgelegt werden. Das Ende des Ausschnitts ergibt sich daraus und aus der gewünschten Anzahl von zu
                berücksichtigenden Tagen.
                <InternalLink view={this.props.uvm.application.settingsPage.route} pict='settings' />
                <br />
                <br />
                Die beiden Schaltflächen direkt rechts neben der Datumsauswahl erlauben es zusätzlich zu den regulären
                Aufzeichnungen auch die vorgesehenen Zeiten für die Aktualisierung
                <HelpLink page={this.props.uvm} topic='tasks' /> der Programmzeitschrift oder der Senderliste in der
                Liste darzustellen.
                <br />
                <br />
                Links vor jeder Aufzeichnung in der Liste befindet sich ein kleines Symbol, das darüber informiert, ob
                die Aufzeichnung wie gewünscht ausgeführt werden kann oder nicht.
                <br />
                <br />
                <table>
                    <tbody>
                        <tr>
                            <td>
                                <Pictogram name='intime' />
                            </td>
                            <td>Die Aufzeichnung wird wie programmiert ausgeführt.</td>
                        </tr>
                        <tr>
                            <td>
                                <Pictogram name='late' />
                            </td>
                            <td>Die Aufzeichnung beginnt verspätet, eventuell fehlt der Anfang.</td>
                        </tr>
                        <tr>
                            <td>
                                <Pictogram name='lost' />
                            </td>
                            <td>Die Aufzeichnung kann nicht ausgeführt werden.</td>
                        </tr>
                    </tbody>
                </table>
                <br />
                Direkt rechts neben dem Symbol wird der Beginn der Aufzeichnung als Verweis angezeigt. Wird dieser
                Verweis angeklickt, so werden weitere Details zur Aufzeichnung sichtbar. Je nach vorhandenen Daten wird
                auch der zugehörige Eintrag der Programmzeitschrift
                <HelpLink page={this.props.uvm} topic='epg' /> abgerufen und angezeigt.
                <br />
                <br />
                Durch Anwählen des Verweises auf den Namen der Aufzeichnung kann diese bearbeitet werden.
                <br />
                <br />
                <table>
                    <tbody>
                        <tr>
                            <td>
                                <Pictogram name='exceptOff' />
                            </td>
                            <td>
                                Handelt es sich um eine sich wiederholende Aufzeichnung
                                <HelpLink page={this.props.uvm} topic='repeatingschedules' />, so kann auch das Symbol
                                direkt rechts neben dem Namen angeklickt werden, um die Ausnahmeregelung für die
                                jeweilige Aufzeichnung anzuzeigen und zu ändern.
                            </td>
                        </tr>
                    </tbody>
                </table>
            </InlineHelp>
        )
    }
}
