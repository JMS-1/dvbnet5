import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse GuideItem
export interface IGuideItemContract {
    // Der Startzeitpunkt in ISO Notation
    startTimeISO: string

    // Die Dauer in Sekunden
    durationInSeconds: number

    // Der Name der Sendung
    name: string

    // Die Sprache, in der die Sendung ausgestrahlt wird
    language: string

    // Die Quelle
    station: string

    // Die Liste der Alterfreigaben
    ratings: string[]

    // Die Liste der Kategorien
    categories: string[]

    // Die ausführliche Beschreibung
    description: string

    // Die Kurzbeschreibung
    summary: string

    // Gesetzt, wenn das Ende der Sendung in der Zukunft liegt
    isActive: boolean

    // Die eindeutige Kennung der Sendung
    identifier: string
}

export function getGuideItem(
    device: string,
    source: string,
    start: Date,
    end: Date
): Promise<IGuideItemContract | undefined> {
    return doUrlCall(`guide/${encodeURIComponent(device)}?source=${source}&pattern=${start.getTime()}-${end.getTime()}`)
}
