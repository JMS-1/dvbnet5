import { guideEncryption } from './guideEncryption'
import { guideSource } from './guideSource'
import { doUrlCall } from './VCRServer'

export interface ISavedGuideQueryContract {
    // Das zu berücksichtigende Gerät
    device: string

    // Optional die Quelle
    source: string

    // Der Text zur Suche in der üblichen Notation mit der Suchart als erstem Zeichen
    text: string

    // Gesetzt, wenn nur im Titel gesucht werden soll
    titleOnly: boolean

    // Die Art der zu berücksichtigenden Quelle
    sourceType: guideSource

    // Die Art der Verschlüsselung
    encryption: guideEncryption
}

export function updateSearchQueries(queries: ISavedGuideQueryContract[]): Promise<void> {
    return doUrlCall<void, ISavedGuideQueryContract[]>('userprofile?favorites', 'PUT', queries)
}
