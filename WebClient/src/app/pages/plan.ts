import { DateTimeUtils } from '../../lib/dateTimeUtils'
import { IToggableFlag, Flag } from '../../lib/edit/boolean/flag'
import { IValueFromList, SelectSingleFromList, IUiValue, uiValue } from '../../lib/edit/list'
import { getPlan } from '../../web/PlanActivityContract'
import { Application } from '../app'
import { IPage, Page } from './page'
import { IPlanEntry, PlanEntry } from './plan/entry'

// Schnittstelle zur Anzeige des Aufzeichnungsplans.
export interface IPlanPage extends IPage {
    // Die aktuell anzuzeigende Liste der Aufzeichnungen.
    readonly jobs: IPlanEntry[]

    // Auswahl des Datums für die erste anzuzeigende Aufzeichnung.
    readonly startFilter: IValueFromList<Date>

    // Auswahl für die Anzeige der Aufgaben zusätzlich zu den Aufzeichnungen.
    readonly showTasks: IToggableFlag
}

// Steuert die Anzeige des Aufzeichnungsplan.
export class PlanPage extends Page implements IPlanPage {
    // Alle aktuell bekannten Aufträge
    private _jobs: PlanEntry[] = []

    // Ermittelt die aktuell anzuzeigenden Aufräge.
    get jobs(): IPlanEntry[] {
        return this._jobs.filter((job) => this.filterJob(job))
    }

    // Pflegt die Anzeige der Aufgaben.
    readonly showTasks = new Flag({}, 'value', 'Aufgaben einblenden', () => this.fireRefresh())

    // Alle bekannten Datumsfilter.
    readonly startFilter = new SelectSingleFromList<Date>({}, 'value', undefined, () => this.fireRefresh(true), [])

    // Gesetzt, wenn keine Abfragen abgesendet werden sollen.
    private _disableQuery: boolean

    // Erzeugt eine neue Steuerung.
    constructor(application: Application) {
        super('plan', application)

        // Navigation abweichend vom Standard konfigurieren.
        this.navigation.plan = false
        this.navigation.refresh = true
    }

    // Daten neu anfordern.
    reload(): void {
        this.query()
    }

    // Wird beim Aufruf der Seite aktiviert.
    reset(sections: string[]): void {
        // Aktuelles Dateum ermitteln - ohne Berücksichtigung der Uhrzeit.
        var now = new Date()

        now = new Date(now.getFullYear(), now.getMonth(), now.getDate())

        // Datumsfilter basierend darauf erstellen.
        var start: IUiValue<Date>[] = []

        for (var i = 0; i < 7; i++) {
            // Eintrag erstellen.
            start.push(uiValue(now, i === 0 ? 'Jetzt' : DateTimeUtils.formatShortDate(now)))

            // Um die gewünschte Anzahl von Tagen weiter setzen.
            now = new Date(now.getFullYear(), now.getMonth(), now.getDate() + this.application.profile.planDays)
        }

        this.startFilter.allowedValues = start
        this.startFilter.value = start[0].value

        // Internen Zustand zurück setzen
        this.showTasks.value = false
        this._jobs = []

        // Daten erstmalig anfordern.
        this.query()
    }

    // Prüft, ob ein Auftrag den aktuellen Einschränkungen entspricht.
    private filterJob(job: PlanEntry): boolean {
        // Datumsfilter.
        var startDay = this.startFilter.value
        var endDay = new Date(
            startDay?.getFullYear() ?? 0,
            startDay?.getMonth() ?? 0,
            (startDay?.getDate() ?? 0) + this.application.profile.planDays
        )

        if (job.start >= endDay) return false
        else if (job.end <= startDay!) return false

        // Aufgabenfilter.
        if (!this.showTasks.value) if (!job.mode) return false

        return true
    }

    // Ermittelt die aktuell gültigen Aufträge.
    private query(): void {
        // Wir schauen maximal 13 Wochen in die Zukunft
        var endOfTime = new Date(Date.now() + 13 * 7 * 86400000)

        // Zusätzlich beschränken wir uns auf maximal 500 Einträge
        getPlan(500, endOfTime).then((plan) => {
            // Anzeigedarstellung für alle Aufträge erstellen.
            var similiar = this.application.guidePage.findInGuide.bind(this.application.guidePage)
            var toggleDetail = this.toggleDetail.bind(this)
            var reload = this.reload.bind(this)

            this._jobs = plan?.map((job) => new PlanEntry(job, toggleDetail, this.application, reload, similiar)) ?? []

            // Die Seite kann nun normal verwendet werden.
            this.application.isBusy = false

            // Anzeige aktualisieren lassen.
            this.fireRefresh()
        })
    }

    // Schaltet die Detailanzeige für einen Auftrag um.
    private toggleDetail(job: PlanEntry, epg: boolean): void {
        // Anzeige einfach nur ausblenden.
        if (job.showEpg && epg) job.showEpg = false
        else if (job.showException && !epg) job.showException = false
        else {
            // Gewünschte Anzeige für genau diesen einen Auftrag aktivieren.
            this._jobs.forEach((j) => (j.showEpg = j.showException = false))

            if (epg) job.showEpg = true
            else job.showException = true
        }

        // Anzeige aktualisieren.
        this.fireRefresh()
    }

    // Anzeige aktualisieren.
    private fireRefresh(full = false): void {
        // Alle Details zuklappen.
        if (full && this._jobs) this._jobs.forEach((j) => (j.showEpg = j.showException = false))

        // Anzeige aktualisieren.
        this.refreshUi()
    }

    // Meldet die Überschrift der Seite.
    get title(): string {
        var days = this.application.profile.planDays

        return `Geplante Aufzeichnungen für ${days} Tag${days === 1 ? '' : 'e'}`
    }
}
