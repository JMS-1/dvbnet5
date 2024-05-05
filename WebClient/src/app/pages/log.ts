import { DateTimeUtils } from '../../lib/dateTimeUtils'
import { IToggableFlag, BooleanProperty } from '../../lib/edit/boolean/flag'
import { IValueFromList, SelectSingleFromList, IUiValue, uiValue } from '../../lib/edit/list'
import { ProfileCache } from '../../web/ProfileCache'
import { getProtocolEntries } from '../../web/IProtocolEntryContract'
import { Application } from '../app'
import { ILogEntry, LogEntry } from './log/entry'
import { IPage, Page } from './page'

// Schnittstelle zur Anzeige des Protokolls.
export interface ILogPage extends IPage {
    // Alle benutzen Geräte.
    readonly profiles: IValueFromList<string>

    // Auswahl des Startzeitpunkts zur Anzeige.
    readonly startDay: IValueFromList<string>

    // Anzahl zur Anzeige von Aktualisierungen der Programmzeitschrift.
    readonly showGuide: IToggableFlag

    // Auswahl zur Anzige der Aktualisierungen der Quellen.
    readonly showScan: IToggableFlag

    // Auswahl zur Anzeige von LIVE Verwendung.
    readonly showLive: IToggableFlag

    // Alle anzuzeigenden Protokolleinträge.
    readonly entries: ILogEntry[]
}

// Präsentationmodell zur anzeige der Protokolleinträge.
export class LogPage extends Page implements ILogPage {
    // Aktualisierung in der Initialisierungsphase unterbinden.
    private _disableLoad = true

    // Alle benutzen Geräte.
    readonly profiles = new SelectSingleFromList<string>({}, 'value', 'Protokollbereich', () => this.load(), [])

    // Anzahl zur Anzeige von Aktualisierungen der Programmzeitschrift.
    readonly showGuide = new BooleanProperty({}, 'value', 'Programmzeitschrift', () => this.refreshUi())

    // Auswahl zur Anzige der Aktualisierungen der Quellen.
    readonly showScan = new BooleanProperty({}, 'value', 'Sendersuchlauf', () => this.refreshUi())

    // Auswahl zur Anzeige von LIVE Verwendung.
    readonly showLive = new BooleanProperty({}, 'value', 'Zapping', () => this.refreshUi())

    // Auswahl des Startzeitpunkts zur Anzeige.
    readonly startDay: SelectSingleFromList<string>

    // Alle Protokolleinträge.
    private _entries: LogEntry[] = []

    get entries(): ILogEntry[] {
        // Aktuellen Filter berücksichtigen.
        return this._entries.filter((e) => {
            if (e.isGuide) return this.showGuide.value
            if (e.isScan) return this.showScan.value
            if (e.isLive) return this.showLive.value

            return true
        })
    }

    // Erstellt ein neues Präsentationsmodell.
    constructor(application: Application) {
        super('log', application)

        // Die Liste der Starttage erstellen wir nur ein einziges Mal.
        var now = new Date()
        var start = new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate()))
        var days: IUiValue<string>[] = []

        for (var i = 0; i < 10; i++) {
            // Zur Auswahl durch den Anwender.
            days.push(
                uiValue(
                    start.toISOString(),
                    DateTimeUtils.formatNumber(start.getUTCDate()) +
                        '.' +
                        DateTimeUtils.formatNumber(1 + start.getUTCMonth())
                )
            )

            // Eine Woche zurück.
            start = new Date(Date.UTC(start.getUTCFullYear(), start.getUTCMonth(), start.getUTCDate() - 7))
        }

        // Auswahlliste aufsetzen.
        this.startDay = new SelectSingleFromList({}, '_value', undefined, () => this.load(), days)
    }

    // Initialisiert das Präsentationsmodell.
    reset(sections: string[]): void {
        // Kontrolliertes Laden der Protokolliste.
        this._disableLoad = true

        // Auswahl zurücksetzen.
        this.startDay.value = this.startDay.allowedValues[0].value

        // Liste der Geräte anfordern.
        ProfileCache.getAllProfiles().then((profiles) => {
            // Auswahlliste vorbereiten.
            this.profiles.allowedValues = profiles.map((p) => uiValue(p.name))
            this.profiles.value = profiles[0] && profiles[0].name

            // Zurück in den Normalbetrieb.
            this._disableLoad = false

            // Protokollliste laden.
            this.load()
        })
    }

    // Protokolliste neu laden.
    private load(): void {
        // Das dürfen wir mal eben nicht.
        if (this._disableLoad) return

        // Suchparameter erstellen.
        var profile = this.profiles.value
        var endDay = new Date(this.startDay.value ?? 0)
        var startDay = new Date(endDay.getTime() - 7 * 86400000)

        // Protokolle vom VCR.NET Recording Service anfordern.
        getProtocolEntries(profile!, startDay, endDay).then((entries) => {
            // Die Anzeige erfolgt immer mit den neuesten als erstes.
            entries?.reverse()

            // Präsentationsmodell erstellen.
            var toggleDetail = this.toggleDetail.bind(this)

            this._entries = entries?.map((e) => new LogEntry(e, toggleDetail)) || []

            // Die Anwendung darf nun verwendet werden.
            this.application.isBusy = false

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi()
        })
    }

    // Detailansicht eines einzelnen Protkolleintrags umschalten.
    private toggleDetail(entry: LogEntry): void {
        // Beim Anschalten alle anderen Detailansichten abschalten.
        if (entry.showDetail.value) this._entries.forEach((e) => (e.showDetail.value = e === entry))

        // Oberfläche zur Aktualisierung auffordern.
        this.refreshUi()
    }

    // Der Titel für die Anzeige des Präsentationsmodells.
    get title(): string {
        return `Aufzeichnungsprotokolle einsehen`
    }
}
