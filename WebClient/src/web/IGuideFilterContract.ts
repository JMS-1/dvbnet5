import { guideEncryption } from './guideEncryption'
import { guideSource } from './guideSource'
import { IGuideItemContract } from './IGuideItemContract'
import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse GuideFilter
export interface IGuideFilterContract {
    // Der Name des aktuell ausgewählten Geräteprofils
    profileName: string

    // Der Name der aktuell ausgewählten Quelle
    source: string

    // Der minimale Startzeitpunkt in ISO Notation
    startISO: string

    // Das Suchmuster für den Namen einer Sendung
    titlePattern: string

    // Das Suchmuster für die Beschreibung einer Sendung
    contentPattern: string

    // Die Anzahl von Sendungen pro Anzeigeseite
    pageSize: number

    // Die aktuelle Seite
    pageIndex: number

    // Einschränkung auf die Art der Quellen
    sourceType: guideSource

    // Einschränkung auf die Verschlüsselung der Quellen
    sourceEncryption: guideEncryption
}

export function queryProgramGuide(filter: IGuideFilterContract): Promise<IGuideItemContract[] | undefined> {
    return doUrlCall('guide/query', 'POST', filter)
}

export function countProgramGuide(filter: IGuideFilterContract): Promise<number | undefined> {
    return doUrlCall('guide/count', 'POST', filter)
}
