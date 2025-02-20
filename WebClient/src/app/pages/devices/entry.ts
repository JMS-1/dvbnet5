﻿import { Controller, IDeviceController } from './controller'

import { DateTimeUtils } from '../../../lib/dateTimeUtils'
import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IConnectable, IView } from '../../../lib/site'
import { ITimeBar, TimeBar } from '../../../lib/timebar'
import { getGuideItem, IGuideItemContract } from '../../../web/IGuideItemContract'
import { IPlanCurrentContract } from '../../../web/IPlanCurrentContract'
import { getDemuxRoot, getDeviceRoot } from '../../../web/VCRServer'
import { GuideInfo, IGuideInfo } from '../guide/entry'

// Ansicht einer Aktivität.
export interface IDeviceInfo extends IConnectable {
    // Name - im Allgemeinen der Aufzeichnung.
    readonly name: string

    // Beginn als Datum mit Uhrzeit.
    readonly displayStart: string

    // Ende als Uhrzeit.
    readonly displayEnd: string

    // Zugehörige Quelle, sofern vorhanden - wie bei Aufzeichnungen: Sonderaufgaben haben im Allgemeinen keine dedizierte Quelle.
    readonly source: string

    // Verwendetes Gerät.
    readonly device: string

    // Informationen zum Umfang der Aktivität - die Dateigröße bei Aufzeichnung, die Anzahl der Quellen beim Sendersuchlauf usw.
    readonly size: string

    // Status der Aktivität - unterscheidet etwa zwischen laufenden und geplanten Aufzeichnungen.
    readonly mode?: 'running' | 'late' | 'intime' | 'null' | 'done'

    // Optional die eindeutige Kennung der Aufzeichnung.
    readonly id?: string

    // Gesetzt um den zugehörigen Eintrag der Programmzeitschrift zu sehen.
    readonly showGuide: IFlag

    // Gesetzt um eine laufende Aufzeichnung anzusehen und zu manipulieren.
    readonly showControl: IFlag

    // Zugehörige Information aus der Programmzeitschrift - sofern vorhanden.
    readonly guideItem: IGuideInfo | null

    // Zugehörige Zeitinformationen zur Information aus der Programmzeitschrift.
    readonly guideTime: ITimeBar

    // Optional die Steuereinheit für laufende Aufzeichnung.
    readonly controller: IDeviceController

    // Die URL zum Starten des LIVE Betrachtens.
    readonly liveUri?: string

    // URL zum Demux abgeschlossener Teilaufzeichungen.
    readonly demux?: string
}

// Präsentationsmodell zur Anzeige einer Aktivität.
export class Info implements IDeviceInfo {
    // Das aktuell angebundene Oberflächenelement.
    view: IView

    // Erstellt ein neues Präsentationsmodell.
    constructor(
        private readonly _model: IPlanCurrentContract,
        toggleDetails: (info: Info, guide: boolean) => void,
        reload: () => void,
        private readonly _findInGuide: (model: IGuideItemContract) => void
    ) {
        // Für Geräte ohne laufende oder geplante Aufzeichnung können wir nicht viel tun.
        if (!_model.isIdle) {
            // Ansonsten müssen wir die Zeiten aus der ISO Notation umrechnen.
            this._start = new Date(_model.startTimeISO)
            this._end = new Date(this._start.getTime() + _model.durationInSeconds * 1000)

            // Und zur Anzeige aufbereiten.
            this.displayStart = DateTimeUtils.formatStartTime(this._start)
            this.displayEnd = DateTimeUtils.formatEndTime(this._end)

            // Das Präsentationsmodell für die Steuerung bei Bedarf erstellen.
            if (this.mode === 'running') this.controller = new Controller(_model, reload)
        }

        // Präsentationsmodell für die Detailansicht erstellen.
        this.showGuide = new BooleanProperty(
            {} as { value?: boolean },
            'value',
            undefined,
            () => toggleDetails(this, true),
            () => !this._model.hasGuideEntry || !this._model.profileName || !this._model.source || this.mode === 'null'
        )
        this.showControl = new BooleanProperty(
            {} as { value?: boolean },
            'value',
            undefined,
            () => toggleDetails(this, false),
            () => !this.controller
        )
    }

    // Gesetzt um den zugehörigen Eintrag der Programmzeitschrift zu sehen.
    readonly showGuide: IFlag

    // Gesetzt um eine laufende Aufzeichnung anzusehen und zu manipulieren.
    readonly showControl: IFlag

    // Die URL zum Starten des LIVE Betrachtens.
    get liveUri() {
        return `${getDeviceRoot()}${encodeURIComponent(this.device)}/0/Live`
    }

    // Status der Aktivität - unterscheidet etwa zwischen laufenden und geplanten Aufzeichnungen.
    get mode() {
        // Gerät wird nicht verwendet.
        if (this._model.isIdle) return undefined

        // Alread done - duration is zero.
        if (this._model.durationInSeconds <= 0) return 'done'

        // Aktivität wurde bereits beendet.
        if (this._end <= new Date()) return 'null'

        // Aufzeichnung läuft.
        if (this._model.planIdentifier) return 'running'

        // Sonderaufgaben sollten eigentlich nur als laufend in der Liste erscheinen.
        if (this._model.sourceName === 'PSI') return undefined
        if (this._model.sourceName === 'EPG') return undefined

        // Geplanten Zustand melden.
        return this._model.isLate ? 'late' : 'intime'
    }

    // Verweis zum Demux.
    get demux(): string {
        return `${getDemuxRoot()}${this._model.fileHashes[0]}`
    }

    // Optional die Steuereinheit für laufende Aufzeichnung.
    readonly controller: Controller

    // Name - im Allgemeinen der Aufzeichnung.
    get name(): string {
        return this._model.name
    }

    // Beginn der Aktivität.
    private readonly _start: Date

    // Voraussichtliches Ende der Aktivität.
    private readonly _end: Date

    // Beginn als Datum mit Uhrzeit.
    readonly displayStart: string

    // Ende als Uhrzeit.
    readonly displayEnd: string

    // Zugehörige Quelle, sofern vorhanden - wie bei Aufzeichnungen: Sonderaufgaben haben im Allgemeinen keine dedizierte Quelle.
    get source(): string {
        return this._model.sourceName
    }

    // Verwendetes Gerät.
    get device(): string {
        return this._model.profileName
    }

    // Informationen zum Umfang der Aktivität - die Dateigröße bei Aufzeichnung, die Anzahl der Quellen beim Sendersuchlauf usw.
    get size(): string {
        return this._model.sizeHint
    }

    // Optional die eindeutige Kennung der Aufzeichnung.
    get id(): string {
        return this._model.identifier
    }

    // Zugehörige Zeitinformationen zur Information aus der Programmzeitschrift.
    private _guideTime: TimeBar

    get guideTime(): ITimeBar {
        return this._guideTime
    }

    // Zugehörige Information aus der Programmzeitschrift - sofern vorhanden.
    private _guideItem: GuideInfo | null

    get guideItem(): IGuideInfo | null {
        // Wird nicht unterstützt.
        if (this.showGuide.isReadonly) return null

        // Es wurde bereits ein Ladeversuch unternommen und kein Eintrag gefunden.
        if (this._guideItem !== undefined) return this._guideItem

        // Programmzeitschrift nach einem passenden Eintrag absuchen.
        getGuideItem(this._model.profileName, this._model.source, this._start, this._end).then((item) => {
            // Ergebnis übernehmen.
            this._guideItem = item ? new GuideInfo(item, this._findInGuide) : null

            // Im Erfolgsfall auch die Zeitschiene aufsetzen.
            if (this._guideItem)
                this._guideTime = new TimeBar(this._start, this._end, this._guideItem.start, this._guideItem.end)

            // Anzeige zur Aktualisierung auffordern.
            this.refreshUi()
        })

        // Erst einmal abwarten.
        return null
    }

    // Die Anzeige zur Aktualisierung auffordern.
    private refreshUi(): void {
        if (this.view) this.view.refreshUi()
    }
}
