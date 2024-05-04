import * as React from 'react'
import { IJobPage } from '../app/pages/jobs'
import { HelpLink } from '../common/helpLink'
import { InlineHelp } from '../common/inlineHelp'
import { InternalLink } from '../lib.react/command/internalLink'
import { SingleSelectButton } from '../lib.react/edit/buttonList'
import { ComponentWithSite } from '../lib.react/reactUi'

// React.Js Komponente zur Anzeige aller Aufträge.
export class Jobs extends ComponentWithSite<IJobPage> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        return (
            <div className='vcrnet-jobs'>
                Diese Ansicht zeigt alle im VCR.NET Recording Service gespeicherten Aufträge.
                <HelpLink page={this.props.uvm} topic='archive' /> Je nach Auswahl beschränkt sich die Liste entweder
                auf die noch aktiven Aufträge oder bereits ausgeführte Aufträge, die bereits in das Archiv übertragen
                wurden.
                {this.getHelp()}
                <div>
                    <SingleSelectButton uvm={this.props.uvm.showArchived} merge={true} />
                </div>
                <fieldset>
                    {this.props.uvm.jobs.map((job, index) => (
                        <div key={index}>
                            <div>{job.name}</div>
                            {job.schedules.map((schedule) => (
                                <InternalLink key={schedule.url} view={schedule.url}>
                                    {schedule.name}
                                </InternalLink>
                            ))}
                        </div>
                    ))}
                </fieldset>
            </div>
        )
    }

    // Hilfe erstellen.
    private getHelp(): JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                <div>
                    Zu jedem Auftrag wir die Liste der zugeordneten Aufzeichnungen angezeigt.
                    <HelpLink page={this.props.uvm} topic='jobsandschedules' /> Durch Auswahl einer solchen Aufzeichnung
                    kann unmittelbar zur Pflege der Daten dieser Aufzeichnung gewechselt werden. Der jeweils erste
                    Eintrag unterhalb des Auftrags selbst wird dabei verwendet, um einem bereits existierenden Auftrag
                    eine ganz neue Aufzeichnung hinzu zu fügen.
                </div>
            </InlineHelp>
        )
    }
}
