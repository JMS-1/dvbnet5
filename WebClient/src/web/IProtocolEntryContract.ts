import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse ProtocolEntry
export interface IProtocolEntryContract {
    // Der Startzeitpunkt in ISO Notation
    startTimeISO: string

    // Der Endzeitpunkt in ISO Notation
    endTimeISO: string

    // Der Name der zuerst verwendeten Quelle
    source: string

    // Der Name der primären Aufzeichnungsdatei
    primaryFile: string

    // Die Liste der erzeugten Dateien
    files: string[]

    // Ein Hinweis zur Anzeige der Größe
    sizeHint: string
}

export function getProtocolEntries(
    device: string,
    startDay: Date,
    endDay: Date
): Promise<IProtocolEntryContract[] | undefined> {
    return doUrlCall(`protocol/${device}?start=${startDay.toISOString()}&end=${endDay.toISOString()}`)
}
