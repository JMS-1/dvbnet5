import { guideEncryption } from './guideEncryption'
import { guideSource } from './guideSource'
import { IGuideItemContract } from './IGuideItemContract'
import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse GuideFilter
export interface IGuideFilterContract {
    // Der Name des aktuell ausgewählten Geräteprofils
    device: string

    // Der Name der aktuell ausgewählten Quelle
    station: string

    // Der minimale Startzeitpunkt in ISO Notation
    start: string

    // Das Suchmuster für den Namen einer Sendung
    title: string

    // Das Suchmuster für die Beschreibung einer Sendung
    content: string

    // Die Anzahl von Sendungen pro Anzeigeseite
    size: number

    // Die aktuelle Seite
    index: number

    // Einschränkung auf die Art der Quellen
    typeFilter: guideSource

    // Einschränkung auf die Verschlüsselung der Quellen
    cryptFilter: guideEncryption
}

export function queryProgramGuide(filter: IGuideFilterContract): Promise<IGuideItemContract[] | undefined> {
    return doUrlCall('guide/query', 'POST', filter)
}

export function countProgramGuide(filter: IGuideFilterContract): Promise<number | undefined> {
    return doUrlCall('guide/count', 'POST', filter)
}
