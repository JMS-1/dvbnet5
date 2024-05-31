import { DateTimeUtils } from '../../../lib/dateTimeUtils'
import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IProtocolEntryContract } from '../../../web/IProtocolEntryContract'
import { getDeviceRoot, getFilePlayUrl } from '../../../web/VCRServer'

// Schnittstelle zur Anzeige eines einzelnen Protokolleintrags.
export interface ILogEntry {
    // Start der Aufzeichnung.
    readonly start: string

    // Ende der Aufzeichnung.
    readonly end: string

    // Ende der Aufzeichnung (nur Uhrzeit).
    readonly endTime: string

    // Verwendete Quelle.
    readonly source: string

    // Umfang der Aufzeichnung (etwa Größe der Aufzeichnungsdateien oder Anzahl der Einträge der Programmzeitschrift).
    readonly size: string

    // Name der (eventuell transienten) primäre Aufzeichnungsdatei.
    readonly primary: string

    // Gesetzt wenn Aufzeichnungsdateien existieren.
    readonly hasFiles: boolean

    // Verweise zur Anzeige der Aufzeichnungsdateien.
    readonly files: string[]

    // Gesetzt wenn Aufzeichnungsdateien existieren.
    readonly hasHashes: boolean

    // Verweise zur Anzeige der Aufzeichnungsdateien.
    readonly fileHashes: string[]

    // Umschaltung für die Detailansicht.
    readonly showDetail: IFlag
}

export class LogEntry implements ILogEntry {
    // Start der Aufzeichnung.
    get start(): string {
        return DateTimeUtils.formatStartTime(new Date(this._model.startTimeISO))
    }

    // Ende der Aufzeichnung.
    get end(): string {
        return DateTimeUtils.formatStartTime(new Date(this._model.endTimeISO))
    }

    // Ende der Aufzeichnung (nur Uhrzeit).
    get endTime(): string {
        return DateTimeUtils.formatEndTime(new Date(this._model.endTimeISO))
    }

    // Umfang der Aufzeichnung (etwa Größe der Aufzeichnungsdateien oder Anzahl der Einträge der Programmzeitschrift).
    get size(): string {
        return this._model.sizeHint
    }

    // Name der (eventuell transienten) primäre Aufzeichnungsdatei.
    get primary(): string {
        return this._model.primaryFile
    }

    // Gesetzt wenn Aufzeichnungsdateien existieren.
    get hasFiles(): boolean {
        return !!this._model.files?.length
    }

    get hasHashes(): boolean {
        return !!this._model.fileHashes?.length
    }

    // Verweise zur Anzeige der Aufzeichnungsdateien.
    get files(): string[] {
        return (this._model.files || []).map(getFilePlayUrl)
    }

    get fileHashes(): string[] {
        return (this._model.fileHashes || []).map((h) => `${getDeviceRoot().replace(/^dvbnet5:/, 'demux:')}${h}`)
    }

    // Verwendete Quelle.
    readonly source: string

    // Gesetzt, wenn es sich um die Aktualisierung der Programmzeitschrift handelt.
    readonly isGuide: boolean = false

    // Gesetzt, wenn es sich um einen Sendersuchlauf handelt.
    readonly isScan: boolean = false

    // Gesetzt, wenn es sich um die LIVE Verwendung handelt.
    readonly isLive: boolean = false

    // Umschaltung für die Detailansicht.
    readonly showDetail

    // Erstellt ein neues Präsentationsmodell.
    constructor(
        private readonly _model: IProtocolEntryContract,
        toggleDetail: (entry: LogEntry) => void
    ) {
        this.showDetail = new BooleanProperty({ value: false }, 'value', undefined, () => toggleDetail(this))

        // Art der Aufzeichnung zum Filtern umsetzen.
        switch (_model.source) {
            case 'EPG':
                this.source = 'Programmzeitschrift'
                this.isGuide = true
                break
            case 'PSI':
                this.source = 'Sendersuchlauf'
                this.isScan = true
                break
            case 'LIVE':
                this.source = 'Zapping'
                this.isLive = true
                break
            default:
                this.source = _model.source
                break
        }
    }
}
