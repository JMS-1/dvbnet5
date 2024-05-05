import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse GuideSettings
export interface IGuideSettingsContract extends ISettingsContract {
    // Das Zeitintervall (in Stunden) für vorgezogene Aktualisierungen
    threshold: string

    // Das minimale Intervall (in Stunden) zwischen Aktualisierungen
    interval: string

    // Die maximale Dauer einer Aktualisierung (in Minuten)
    duration: number

    // Die vollen Stunden, zu denen eine Aktualisierung stattfinden soll
    hours: number[]

    // Alle Quellen, die bei der Aktualisierung mit berücksichtigt werden sollen
    sources: string[]

    // Gesetzt, um auch die britische Programmzeitschrift auszuwerten
    withUKGuide: boolean
}

export function getGuideSettings(): Promise<IGuideSettingsContract | undefined> {
    return doUrlCall('configuration/guide')
}

export function setGuideSettings(data: IGuideSettingsContract): Promise<boolean | undefined> {
    return doUrlCall('configuration/guide', 'PUT', data)
}
