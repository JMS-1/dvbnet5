import { GuideEntry } from './guide/entry'
import { IPage, Page } from './page'

import { Command, ICommand } from '../../lib/command/command'
import { DateTimeUtils } from '../../lib/dateTimeUtils'
import { BooleanProperty, IToggableFlag } from '../../lib/edit/boolean/flag'
import { IValueFromList, SingleListProperty, uiValue } from '../../lib/edit/list'
import { IString, StringProperty } from '../../lib/edit/text/text'
import { guideEncryption } from '../../web/guideEncryption'
import { GuideInfoCache } from '../../web/GuideInfoCache'
import { guideSource } from '../../web/guideSource'
import { IGuideFilterContract, queryProgramGuide } from '../../web/IGuideFilterContract'
import { IGuideInfoContract } from '../../web/IGuideInfoContract'
import { IGuideItemContract } from '../../web/IGuideItemContract'
import { getProfileJobInfos } from '../../web/IProfileJobInfoContract'
import { ISavedGuideQueryContract } from '../../web/ISavedGuideQueryContract'
import { ProfileCache } from '../../web/ProfileCache'
import { Application } from '../app'

// Schnittstelle zum Blättern in der Programmzeitschrift.
export interface IGuidePageNavigation {
    // Befehl zum Wechseln auf den Anfang der aktuellen Ergebnisliste.
    readonly firstPage: ICommand

    // Befehl zum Wechseln auf die vorherige Seite der aktuellen Ergebnisliste.
    readonly prevPage: ICommand

    // Befehl zum Wechseln auf die nächste Seite der aktuellen Ergebnisliste.
    readonly nextPage: ICommand
}

// Schnittstelle zur Anzeige der Programmzeitschrift.
export interface IGuidePage extends IPage, IGuidePageNavigation {
    // Der anzuzeigende Ausschnitt der aktuellen Ergebnisliste.
    readonly entries: GuideEntry[]

    // Alle bekannten Geräte.
    readonly profiles: IValueFromList<string>

    // Alle Quellen auf dem aktuell ausgewählten Gerät.
    readonly sources: IValueFromList<string>

    // Auswahl des Verschlüsselungsfilters.
    readonly encrpytion: IValueFromList<guideEncryption>

    // Gesetzt, wenn der Verschlüsselungsfilter angezeigt werden soll.
    readonly showEncryption: boolean

    // Auswahl der Einschränkung auf die Art der Quelle.
    readonly sourceType: IValueFromList<guideSource>

    // Gesetzt, wenn die Einschränkung der Art der Quelle angezeigt werden soll.
    readonly showSourceType: boolean

    // Setzt den Anfang der Ergebnisliste auf ein bestimmtes Datum.
    readonly days: IValueFromList<string>

    // Setzt den Anfang der Ergebnisliste auf eine bestimmte Uhrzeit.
    readonly hours: IValueFromList<number>

    // Der aktuelle Text zur Suche in allen Einträgen der Programmzeitschrift.
    readonly queryString: IString

    // Gesetzt, wenn auch in der Beschreibung gesucht werden soll.
    readonly withContent: IToggableFlag

    // Befehl zum Zurücksetzen aller Einschränkungen.
    readonly resetFilter: ICommand

    // Befehl zum Anlegen einer neuen gespeicherten Suche.
    readonly addFavorite: ICommand
}

// Ui View Model zur Anzeige der Programmzeitschrift.
export class GuidePage extends Page implements IGuidePage {
    // Optionen zur Auswahl der Einschränkung auf die Verschlüsselung.
    private static readonly _cryptOptions = [
        uiValue(guideEncryption.FREE, 'Nur unverschlüsselt'),
        uiValue(guideEncryption.PAY, 'Nur verschlüsselt'),
        uiValue(guideEncryption.ALL, 'Alle Quellen'),
    ]

    // Optionen zur Auswahl der Einschränkuzng auf die Art der Quelle.
    private static readonly _typeOptions = [
        uiValue(guideSource.TV, 'Nur Fernsehen'),
        uiValue(guideSource.RADIO, 'Nur Radio'),
        uiValue(guideSource.ALL, 'Alle Quellen'),
    ]

    // Für den Start der aktuellen Ergebnisliste verfügbaren Auswahloptionen für die Uhrzeit.
    private static readonly _hours = [
        uiValue(0, '00:00'),
        uiValue(6, '06:00'),
        uiValue(12, '12:00'),
        uiValue(18, '18:00'),
        uiValue(20, '20:00'),
        uiValue(22, '22:00'),
    ]

    // Die aktuellen Einschränkungen.
    private _filter: IGuideFilterContract = {
        content: '',
        cryptFilter: guideEncryption.ALL,
        device: '',
        index: 0,
        size: 20,
        start: '',
        station: '',
        title: '',
        typeFilter: guideSource.ALL,
    }

    // Schnittstelle zur Auswahl des zu betrachtenden Gerätes.
    readonly profiles = new SingleListProperty<string>(
        this._filter,
        'device',
        'Gerät',
        () => this.onDeviceChanged(true),
        []
    )

    // Schnittstelle zur Auswahl der Quelle.
    readonly sources = new SingleListProperty<string>(this._filter, 'station', 'Quelle', () => this.query(), [])

    // Schnittstelle zur Auswahl der Einschränkung auf die Verschlüsselung.
    readonly encrpytion = new SingleListProperty(
        this._filter,
        'cryptFilter',
        undefined,
        () => this.query(),
        GuidePage._cryptOptions
    )

    // Schnittstelle zur Auswahl der Einschränkung auf die Art der Quelle.
    readonly sourceType = new SingleListProperty(
        this._filter,
        'typeFilter',
        undefined,
        () => this.query(),
        GuidePage._typeOptions
    )

    // Schnittstelle zum Setzen eines bestimmten Tags für den Anfang der Ergebnisliste.
    readonly days = new SingleListProperty<string>(
        this._filter,
        'start',
        undefined,
        () => this.resetIndexAndQuery(),
        []
    )

    // Schnittstelle zum Setzen einer bestimmten Uhrzeit für den Anfange der Ergebnisliste.
    readonly hours = new SingleListProperty(
        { value: -1 },
        'value',
        undefined,
        () => this.resetIndexAndQuery(),
        GuidePage._hours
    )

    // Schnittstelle zur Pflege der Freitextsuchbedingung.
    readonly queryString = new StringProperty({ value: '' }, 'value', 'Suche nach', () => this.delayedQuery())

    // Schnittstelle zur Pflege der Auswahl der Freitextsuche auf die Beschreibung.
    readonly withContent = new BooleanProperty({ value: true }, 'value', 'Auch in Beschreibung suchen', () =>
        this.query()
    )

    // Aktuelle Anmeldung für verzögerte Suchanfragen.
    private _timeout?: number

    // Befehl zur Anzeige des Anfangs der Ergebnisliste.
    readonly firstPage = new Command(
        () => this.changePage(-this._filter.index),
        'Erste Seite',
        () => this._filter.index > 0
    )

    // Befehl zur Anzeige der vorherigen Seite der Ergebnisliste.
    readonly prevPage = new Command(
        () => this.changePage(-1),
        'Vorherige Seite',
        () => this._filter.index > 0
    )

    // Befehl zur Anzeige der nächsten Seite der Ergebnisliste.
    readonly nextPage = new Command(
        () => this.changePage(+1),
        'Nächste Seite',
        () => !!this._hasMore
    )

    // Befehl zum Zurücksetzen aller aktuellen Einschränkungen.
    readonly resetFilter = new Command(() => this.resetAllAndQuery(), 'Neue Suche')

    // Befehl zum Anlegen einer neuen gespeicherten Suche.
    readonly addFavorite = new Command(
        () => this.createFavorite(),
        'Aktuelle Suche als Favorit hinzufügen',
        () => !!(this.queryString.value || '').trim()
    )

    // Meldet, ob die Auswahl der Verschlüsselung angeboten werden soll.
    get showEncryption(): boolean {
        return !this._filter.station
    }

    // Meldet, ob die Auswahl der Art der Quelle angeboten werden soll.
    get showSourceType(): boolean {
        return this.showEncryption
    }

    // Der aktuell anzuzeigende Ausschnitt aus der Ergebnisliste.
    entries: GuideEntry[] = []

    // Die aktuelle Liste der für das Gerät angelegten Aufträg.
    private _jobSelector = new SingleListProperty<string>({}, 'value', 'zum Auftrag').addRequiredValidator()

    // Gesetzt, wenn eine nächste Seite der Ergebnisliste existiert.
    private _hasMore? = false

    // Beschreibt den Gesamtauszug der Programmzeitschrift zum aktuell ausgewählten Gerät.
    private _profileInfo: IGuideInfoContract

    // Die konkrete Art der Suche.
    private _fulltextQuery = true

    // Zeigt an, dass die Präsentation gearade die Daten für die initiale Ansicht sammelt.
    private _disableQuery: boolean

    // Erstellt eine neue Instanz zur Anzeige der Programmzeitschrift.
    constructor(application: Application) {
        super('guide', application)

        // Navigation abweichend vom Standard konfigurieren.
        this.navigation.favorites = true
        this.navigation.guide = false
    }

    // Wird aufgerufen wenn in der Oberfläche die Programmzeitschrift angezeigt werden soll.
    reset(sections: string[]): void {
        // In der Initialisierungsphase ist es wichtig, dass einige Dinge nicht getan werden.
        this._disableQuery = true

        // Anzeige löschen.
        this.entries = []
        this._hasMore = false
        this.nextPage.reset()
        this.prevPage.reset()
        this.firstPage.reset()
        this.resetFilter.reset()
        this.addFavorite.reset()

        // Größe der Anzeigeliste auf den neusten Stand bringen - alle anderen Einschränkungen bleiben erhalten!
        this._filter.size = this.application.profile.guideRows

        // Die Liste aller bekannten Geräte ermitteln.
        ProfileCache.getAllProfiles().then((profiles) => {
            // Auswahl aktualisieren.
            this.profiles.allowedValues = (profiles || []).map((p) => uiValue(p.name))
            this.profiles.validate()

            // Erstes Gerät vorauswählen.
            if (!this._filter.device || this.profiles.message)
                this._filter.device = this.profiles.allowedValues[0].value

            // Die Startphase ist erst einmal abgeschlossen.
            this._disableQuery = false

            // Ergebnisliste aktualisieren.
            this.onDeviceChanged(false)
        })
    }

    // Ruft eine Methode mit deaktivierter Reaktion auf Suchen auf.
    private disableQuery(callback: () => void): void {
        // Ursprüngliche Einstellung.
        const wasDisabled = this._disableQuery

        // Deaktivieren.
        this._disableQuery = true

        // Methode aufrufen.
        callback()

        // Ursprünglichen Zustand wieder herstellen.
        this._disableQuery = wasDisabled
    }

    // Meldet die Überschrift der Seite.
    get title(): string {
        return 'Programmzeitschrift'
    }

    // Alle Einschränkungen entfernen.
    private clearFilter(): void {
        this.disableQuery(() => {
            this._filter.cryptFilter = guideEncryption.ALL
            this._filter.typeFilter = guideSource.ALL
            this._filter.content = ''
            this._fulltextQuery = true
            this._filter.station = ''
            this._filter.start = ''
            this._filter.title = ''
            this._filter.index = 0

            this.queryString.value = ''
            this.withContent.value = true

            this.hours.value = -1
        })
    }

    // Ähnliche Aufzeichnungen suchen.
    findInGuide(model: IGuideItemContract): void {
        this.clearFilter()

        this.disableQuery(() => {
            // Textsuche auf den Namen auf dem selben Gerät.
            this._filter.device = model.id.split(':')[1]
            this._filter.station = model.station

            this.queryString.value = model.name
            this.withContent.value = false

            // Die exakte Suche kann über die Oberfläche nicht direkt aktiviert werden.
            this._fulltextQuery = false
        })

        // Eventuell kommen wir dabei aus einem anderen Teilt der Anwendung.
        if (this.application.page === this) this.query()
        else this.application.gotoPage(this.route)
    }

    // Vordefinierte Suche als Suchbedingung laden.
    loadFilter(filter: ISavedGuideQueryContract): void {
        this.clearFilter()

        this.disableQuery(() => {
            // Der Suchtext beginnt immer mit der Art des Vergleichs.
            const query = filter.text || ''

            this._filter.cryptFilter = filter.encryption
            this._filter.typeFilter = filter.sourceType
            this._filter.station = filter.source

            this.queryString.value = query.substr(1)
            this._fulltextQuery = query[0] === '*'

            this.withContent.value = !filter.titleOnly
        })

        // Zur Programmzeitschrift wechseln.
        this.application.gotoPage(this.route)
    }

    // Nach der Auswahl des Gerätes alle Listen aktualisieren.
    private onDeviceChanged(deviceHasChanged: boolean) {
        // Nicht während der Initialisierung des Präsentationsmodells.
        if (this._disableQuery) return

        // Und auch kontrolliert immer nur einmal.
        this._disableQuery = true

        GuideInfoCache.getPromise(this._filter.device)
            .then((info) => {
                // Informationen zur Programmzeitschrift des Gerätes festhalten.
                this._profileInfo = info

                // Daraus die Liste der Quellen und möglichen Starttage ermitteln.
                this.refreshSources()
                this.refreshDays()

                // Liste der Aufträge laden.
                return getProfileJobInfos(this._filter.device)
            })
            .then((jobs) => {
                // Liste der bekannten Aufträge aktualisieren.
                const selection = jobs?.map((job) => uiValue(job.id, job.name))

                selection?.unshift(uiValue('', '(neuen Auftrag anlegen)'))

                this._jobSelector.allowedValues = selection ?? []

                // Von jetzt arbeiten wir wieder normal.
                this._disableQuery = false

                // Ergebnisliste neu laden - bei Wechsel des Gerätes werden alle Einschränkungen entfernt.
                if (deviceHasChanged) this.resetAllAndQuery()
                else this.query()
            })
    }

    // Die Liste der Quellen des aktuell ausgewählten Gerätes neu ermitteln.
    private refreshSources(): void {
        // Der erste Eintrag erlaubt immer die Anzeige ohne vorausgewählter Quelle.
        this.sources.allowedValues = [uiValue('', '(Alle Sender)')].concat(
            (this._profileInfo.stations || []).map((s) => uiValue(s))
        )
    }

    // Die Liste der möglichen Starttage ermitteln.
    private refreshDays(): void {
        // Als Basis kann immer die aktuelle Uhrzeit verwendet werden.
        const days = [uiValue<string>('', 'Jetzt')]

        // Das geht nur, wenn mindestens ein Eintrag in der Programmzeitschrift der aktuellen Quelle vorhanden ist.
        if (this._profileInfo.first && this._profileInfo.last) {
            // Die Zeiten werden immer in UTC gemeldet, die Anzeige erfolgt aber immer lokal - das kann am ersten Tag zu fehlenden Einträgen führen.
            let first = new Date(this._profileInfo.first)
            const last = new Date(this._profileInfo.last)

            // Es werden maximal 14 mögliche Starttage angezeigt.
            for (let i = 0; i < 14 && first.getTime() <= last.getTime(); i++) {
                // Korrigieren.
                const start = new Date(first.getFullYear(), first.getMonth(), first.getDate())

                // Auswahlelement anlegen.
                days.push(uiValue(start.toISOString(), DateTimeUtils.formatShortDate(start)))

                // Nächsten Tag auswählen.
                first = new Date(start.getFullYear(), start.getMonth(), start.getDate() + 1)
            }
        }

        this.days.allowedValues = days
    }

    // Deaktiviert die verzögerte Ausführung einer Suchanfrage.
    private clearTimeout(): void {
        // Ist schon passiert.
        if (this._timeout === undefined) return

        // Sicherstellen, dass hier nichts mehr nachkommt.
        clearTimeout(this._timeout)

        this._timeout = undefined
    }

    // Aktiviert die verzögerte Ausführung einer Suchanfrage.
    private delayedQuery(): void {
        this.clearTimeout()

        // Das dürfen wir gerade nicht.
        if (this._disableQuery) return

        // Nach Eingabe gibt es immer die Volltextsuche.
        this._fulltextQuery = true

        // Nach einer viertel Sekunde Suche starten.
        this._timeout = window.setTimeout(() => {
            // Wir werden gar nicht mehr angezeigt.
            if (!this.view) return

            // Suche starten.
            this._filter.index = 0
            this.query()
        }, 250)
    }

    // Entfernt alle Einschränkungen und führt eine neue Suche aus.
    private resetAllAndQuery(): void {
        this.clearFilter()
        this.query()
    }

    // Setzt die Ergebnisliste auf den Anfang und führt eine neue Suche aus.
    private resetIndexAndQuery(): void {
        this._filter.index = 0
        this.query()
    }

    // Führt eine Suche aus.
    private query(): void {
        // Das dürfen wir leider gerade nicht.
        if (this._disableQuery) return

        // Eine eventuell ausstehende verzögerte Suche deaktivieren.
        this.clearTimeout()

        // Ausstehende Änderung der Startzeit einmischen.
        if ((this.hours.value ?? 0) >= 0) {
            // Vollen Startzeitpunkt bestimmen.
            const start = this._filter.start ? new Date(this._filter.start) : new Date()

            this._filter.start = new Date(
                start.getFullYear(),
                start.getMonth(),
                start.getDate(),
                this.hours.value ?? 0
            ).toISOString()

            // Das machen wir immer nur einmal - die Auswahl wirkt dadurch wie eine Schaltfläche.
            this.disableQuery(() => (this.hours.value = -1))
        }

        // Suchbedingung vorbereiten und übernehmen.
        const query = this.queryString.value?.trim()

        this._filter.title = !query ? '' : `${this._fulltextQuery ? '*' : '='}${query}`
        this._filter.content = this.withContent.value && this._fulltextQuery ? this._filter.title : ''

        // Auszug aus der Programmzeitschrift abrufen.
        queryProgramGuide(this._filter).then((items) => {
            // Einträge im Auszug auswerten.
            const toggleDetails = this.toggleDetails.bind(this)
            const createNew = this.createNewSchedule.bind(this)
            const similiar = this.findInGuide.bind(this)

            this.entries = (items || [])
                .slice(0, this._filter.size)
                .map((i) => new GuideEntry(i, similiar, toggleDetails, createNew, this._jobSelector))
            this._hasMore = items && items.length > this._filter.size

            // Anwendung zur Bedienung freischalten.
            this.application.isBusy = false

            // Anzeige aktualisieren.
            this.refreshUi()
        })
    }

    // Legt eine neue Aufzeichnung an.
    private createNewSchedule(entry: GuideEntry): void {
        this.application.gotoPage(`${this.application.editPage.route};id=*${entry.jobSelector.value};epgid=${entry.id}`)
    }

    // Aktualisiert die Detailanzeige für einen Eintrag.
    private toggleDetails(entry: GuideEntry): void {
        // Anzeige auf die eine Sendung umschalten.
        const show = entry.showDetails

        this.entries.forEach((e) => (e.showDetails = false))

        entry.showDetails = !show

        // Auswahl zurücksetzen.
        this._jobSelector.value = ''

        // Oberfläche zur Aktualisierung auffordern.
        this.refreshUi()
    }

    // In der Ergebnisliste bättern.
    private changePage(delta: number): void {
        // Startseite ändern und neue Suche ausführen.
        this._filter.index += delta
        this.query()
    }

    // Legt eine neue gespeicherte Suche an.
    private createFavorite(): Promise<void> {
        // Protokollstruktur anlegen.
        const query: ISavedGuideQueryContract = {
            device: this._filter.device,
            encryption: this._filter.station ? guideEncryption.ALL : this._filter.cryptFilter,
            source: this._filter.station,
            sourceType: this._filter.station ? guideSource.ALL : this._filter.typeFilter,
            text: `${this._fulltextQuery ? '*' : '='}${this.queryString.value}`,
            titleOnly: !this.withContent.value,
        }

        // Zur Ausführung verwenden wir das Präsentationsmodell der gespeicherten Suchen.
        return this.application.favoritesPage.add(query)
    }
}
