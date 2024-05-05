import { IPlanException, PlanException } from './exception'

import { DateTimeUtils } from '../../../lib/dateTimeUtils'
import { IConnectable, IView } from '../../../lib/site'
import { ITimeBar, TimeBar } from '../../../lib/timebar'
import { getGuideItem, IGuideItemContract } from '../../../web/IGuideItemContract'
import { IPlanActivityContract } from '../../../web/IPlanActivityContract'
import { GuideInfo, IGuideInfo } from '../guide/entry'

// Erweiterte Schnittstelle (View Model) zur Anzeige eines Eintrags des Aufzeichnunsplans.
export interface IPlanEntry extends IConnectable {
    // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
    readonly mode?: string

    // Anwendungsverweis zum Ändern dieses Eintrags.
    readonly editLink?: string

    // Die zugehörige Ausnahmeregel.
    readonly exception: IPlanException

    // Das verwendete Gerät.
    readonly device: string

    // Der zugehörige Sender.
    readonly station: string

    // Der Startzeitpunkt formatiert für die Darstellung.
    readonly displayStart: string

    // Der Endzeitpunkt, formatiert für die Darstellung - es werden nur Stunden und Minuten angezeigt.
    readonly displayEnd: string

    // Die Dauer der Aufzeichnung.
    readonly duration: number

    // Der Name der Aufzeichnung.
    readonly name: string

    // Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.
    readonly allAudio: boolean

    // Gesetzt, wenn Dolby Tonspuren aufgezeichnet werden sollen.
    readonly dolby: boolean

    // Gesetzt, wenn der Videotext mit aufgezeichnet werden soll.
    readonly ttx: boolean

    // Gesetzt, wenn DVB Untertitel mit aufgezeichnet werden sollen.
    readonly subs: boolean

    // Gesetzt, wenn DVB Untertitel mit aufgezeichnet werden sollen.
    readonly guide: boolean

    // Gesetzt, wenn die Endzeit evtl. wegen der Zeitumstellung nicht wie erwartet ist.
    readonly suspectTime: boolean

    // Zeigt die Programmzeitschrift an.
    readonly showEpg: boolean

    // Zeigt die Pflege der Ausnahmeregel an.
    readonly showException: boolean

    // Die am besten passenden Informationen aus der Programmzeitschrift.
    readonly guideItem: IGuideInfo | null

    // Beschreibt die Zeit von Aufzeichung und Eintrag der Programmzeitschrift.
    readonly guideTime: ITimeBar

    // Schaltet die Detailanzeige um.
    toggleDetail(epg: boolean): void
}

// Präsentationsmodell zur Anzeige eines Eintrags im Aufzeichnungsplan.
export class PlanEntry implements IPlanEntry {
    // Erstellt ein neues Präsentationsmodell.
    constructor(
        private model: IPlanActivityContract,
        private _toggleDetail: (entry: PlanEntry, epg: boolean) => void,
        reload: () => void,
        private readonly _findInGuide: (model: IGuideItemContract) => void
    ) {
        // Zeiten umrechnen
        this.duration = parseInt(model.durationInSeconds)
        this.start = new Date(model.startTimeISO ?? '')
        this.end = new Date(this.start.getTime() + 1000 * this.duration)

        // Ausnahmen auswerten
        if (model.exceptionRule) this.exception = new PlanException(model.exceptionRule, model.legacyReference, reload)
    }

    // Zeigt die Programmzeitschrift an.
    private _showEpg = false

    get showEpg(): boolean {
        return this._showEpg
    }

    set showEpg(newValue: boolean) {
        this._showEpg = newValue
    }

    // Zeigt die Pflege der Ausnahmeregel an.
    private _showException = false

    get showException(): boolean {
        return this._showException
    }

    set showException(newValue: boolean) {
        if (this.exception) this.exception.reset()

        this._showException = newValue
    }

    // Die Dauer der Aufzeichnung.
    readonly duration: number

    // Der Zeitpunkt, an dem die Aufzeichnung beginnen wird.
    readonly start: Date

    // Der Zeitpunkt, an dem die Aufzeichnung enden wird.
    readonly end: Date

    get suspectTime(): boolean {
        return this.model.endTimeCouldBeWrong
    }

    // Der Name der Aufzeichnung.
    get name(): string {
        return this.model.fullName
    }

    // Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.
    get allAudio(): boolean {
        return this.model.allLanguages
    }

    // Gesetzt, wenn Dolby Tonspuren aufgezeichnet werden sollen.
    get dolby(): boolean {
        return this.model.dolby
    }

    // Gesetzt, wenn der Videotext mit aufgezeichnet werden soll.
    get ttx(): boolean {
        return this.model.videoText
    }

    // Gesetzt, wenn DVB Untertitel mit aufgezeichnet werden sollen.
    get subs(): boolean {
        return this.model.subTitles
    }

    // Gesetzt, wenn DVB Untertitel mit aufgezeichnet werden sollen.
    get guide(): boolean {
        return this.model.hasGuideEntry
    }

    // Der Startzeitpunkt formatiert für die Darstellung.
    get displayStart(): string {
        return DateTimeUtils.formatStartTime(this.start)
    }

    // Der Endzeitpunkt, formatiert für die Darstellung - es werden nur Stunden und Minuten angezeigt.
    get displayEnd(): string {
        return DateTimeUtils.formatEndTime(this.end)
    }

    // Die zugehörige Ausnahmeregel.
    readonly exception: PlanException

    // Das verwendete Gerät.
    get device(): string {
        return this.model.device || ''
    }

    // Der zugehörige Sender.
    get station(): string {
        return this.model.station || '(unbekannt)'
    }

    // Ein Kürzel für die Qualität der Aufzeichnung, etwa ob dieser verspätet beginnt.
    get mode(): string | undefined {
        if (this.model.station === 'PSI') return
        if (this.model.station === 'EPG') return

        if (this.model.isHidden) return 'lost'
        else if (this.model.isLate) return 'late'
        else return 'intime'
    }

    // Anwendungsverweis zum Ändern dieses Eintrags.
    get editLink(): string | undefined {
        return this.mode && `edit;id=${this.model.legacyReference}`
    }

    // Schaltet die Detailanzeige um.
    toggleDetail(epg: boolean): void {
        return this._toggleDetail(this, epg)
    }

    // Das zugehörige Oberflächenelement.
    view: IView

    // Fordert die Oberfläche zur Aktualisierung auf.
    private refreshUi(): void {
        if (this.view) this.view.refreshUi()
    }

    // Beschreibt die Zeit von Aufzeichung und Eintrag der Programmzeitschrift.
    private _guideTime: TimeBar

    get guideTime(): ITimeBar {
        return this._guideTime
    }

    // Beschreibt die Zeit von Aufzeichung und Eintrag der Programmzeitschrift.
    private _guideItem: GuideInfo | null

    get guideItem(): IGuideInfo | null {
        // Das ist grundsätzlich nicht möglich.
        if (!this.model.hasGuideEntry || !this.model.guideEntryDevice || !this.model.source) return null

        // Das haben wir schon einmal probiert.
        if (this._guideItem !== undefined) return this._guideItem

        // In der Programmzeitschrift suchen und den am besten passenden Eintrag ermitteln.
        getGuideItem(this.model.guideEntryDevice, this.model.source, this.start, this.end).then((item) => {
            // Eventuell Präsentationsmodell für den Eintrag erstellen.
            this._guideItem = item ? new GuideInfo(item, this._findInGuide) : null

            // Zusätzlich ein Präsentationsmodell für die Zeitschiene erstellen.
            if (this._guideItem)
                this._guideTime = new TimeBar(this.start, this.end, this._guideItem.start, this._guideItem.end)

            // Overfläche zur Aktualisierung auffordern.
            this.refreshUi()
        })

        // Erst einmal keine Informationen - wir warten auf die Antwort des VCR.NET Recording Service.
        return null
    }
}
