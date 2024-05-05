import { ScheduleEditor } from './edit/schedule'
import { IPage, Page } from './page'

import { DateTimeUtils } from '../../lib/dateTimeUtils'
import { IValueFromList, SingleListProperty, uiValue } from '../../lib/edit/list'
import { getInfoJobs } from '../../web/IInfoJobContract'
import { IInfoScheduleContract } from '../../web/IInfoScheduleContract'
import { Application } from '../app'

// Schnittstelle zur Anzeige einer Aufzeichnung.
export interface IJobPageSchedule {
    // Der Name der Aufzeichnung.
    readonly name: string

    // Der Verweis zur Pflege der Aufzeichnung.
    readonly url: string
}

// Schnittstelle zur Anzeige eines Auftrags.
export interface IJobPageJob {
    // Der Name des Auftrags.
    readonly name: string

    // Alle Aufzeichnungen zum Auftrag.
    readonly schedules: IJobPageSchedule[]

    // Gesetzt, wenn sich der Auftrag noch nicht im Archiv befindet.
    readonly isActive: boolean
}

// Schnittstelle zur Anzeige der Auftragsübersicht.
export interface IJobPage extends IPage {
    // Schaltet zwischen der Ansicht der aktiven und archivierten Aufzeichnungen um.
    readonly showArchived: IValueFromList<boolean>

    // Alle Aufträge.
    readonly jobs: IJobPageJob[]
}

// Präsentationsmodell zur Ansicht aller Aufträge.
export class JobPage extends Page implements IJobPage {
    // Optionen zur Auswahl der Art des Auftrags.
    private static readonly _types = [uiValue(false, 'Aktiv'), uiValue(true, 'Archiviert')]

    // Schaltet zwischen der Ansicht der aktiven und archivierten Aufzeichnungen um.
    readonly showArchived = new SingleListProperty({}, 'value', undefined, () => this.refreshUi(), JobPage._types)

    // Alle Aufträge.
    private _jobs: IJobPageJob[] = []

    get jobs(): IJobPageJob[] {
        const archived = this.showArchived.value

        return this._jobs.filter((job) => job.isActive !== archived)
    }

    // Erstellt ein neues Präsentationsmodell.
    constructor(application: Application) {
        super('jobs', application)
    }

    // Initialisiert die Anzeige des Präsentationsmodells.
    reset(sections: string[]): void {
        // Bereich vorwählen.
        this.showArchived.value = sections[0] === 'archive'

        // Aktuelle Liste von VCR.NET Recording Service abrufen.
        getInfoJobs().then((info) => {
            // Rohdaten in primitive Präsentationsmodell wandeln.
            this._jobs =
                info?.map((j) => {
                    const job: IJobPageJob = { isActive: j.active, name: j.name, schedules: [] }

                    // Es wird immer ein erster Eintrag zum Anlegen neuer Aufzeichnungen zum Auftrag hinzugefügt.
                    job.schedules.push({
                        name: '(Neue Aufzeichnung)',
                        url: `${this.application.editPage.route};id=${j.id}`,
                    })
                    job.schedules.push(...j.schedules.map((s) => this.createSchedule(s)))

                    return job
                }) ?? []

            // Die Anwendung wird nun zur Bedienung freigegeben.
            this.application.isBusy = false

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi()
        })
    }

    // Erstellt ein neues Präsenationsmodell für eine einzelne Aufzeichnung.
    private createSchedule(schedule: IInfoScheduleContract): IJobPageSchedule {
        // Der Name ist je nach konkreter Konfiguration etwas aufwändiger zu ermitteln.
        const name = schedule.name || 'Aufzeichnung'
        const startTime = new Date(schedule.start)
        const repeat = schedule.repeatPattern
        let start = ''

        if (repeat === 0) start = DateTimeUtils.formatStartTime(startTime)
        else {
            if (repeat & ScheduleEditor.flagMonday) start += DateTimeUtils.germanDays[1]
            if (repeat & ScheduleEditor.flagTuesday) start += DateTimeUtils.germanDays[2]
            if (repeat & ScheduleEditor.flagWednesday) start += DateTimeUtils.germanDays[3]
            if (repeat & ScheduleEditor.flagThursday) start += DateTimeUtils.germanDays[4]
            if (repeat & ScheduleEditor.flagFriday) start += DateTimeUtils.germanDays[5]
            if (repeat & ScheduleEditor.flagSaturday) start += DateTimeUtils.germanDays[6]
            if (repeat & ScheduleEditor.flagSunday) start += DateTimeUtils.germanDays[0]

            start += ` ${DateTimeUtils.formatEndTime(startTime)}`
        }

        // Präsenationsmodell erstellen.
        return {
            name: `${name}: ${start} auf ${schedule.sourceName}`,
            url: `${this.application.editPage.route};id=${schedule.id}`,
        }
    }

    // Der Name des Präsentationsmodell zur Darstellung einer Überschrift in der Oberfläche.
    get title(): string {
        return 'Alle Aufträge'
    }
}
