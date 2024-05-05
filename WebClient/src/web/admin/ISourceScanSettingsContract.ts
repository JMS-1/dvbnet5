import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse SourceScanSettings
export interface ISourceScanSettingsContract extends ISettingsContract {
    // Das Zeitinterval (in Stunden) für vorgezogene Aktualisierungen
    joinDays: string

    // Das minimale Intervall (in Tagen) zwischen den Aktualisierungen - negative Werte für eine ausschließlich manuelle Aktualisierung
    interval: number

    // Die maximale Dauer einer Aktualisierung (in Minuten)
    duration: number

    // Die vollen Stunden, zu denen eine Aktualisierung stattfinden soll
    hours: number[]

    // Gesetzt, wenn die neu ermittelten Listen mit den alten zusammengeführt werden sollen
    merge: boolean
}

export function getSourceScanSettings(): Promise<ISourceScanSettingsContract | undefined> {
    return doUrlCall('configuration/scan')
}

export function setSourceScanSettings(data: ISourceScanSettingsContract): Promise<boolean | undefined> {
    return doUrlCall('configuration/scan', 'PUT', data)
}
