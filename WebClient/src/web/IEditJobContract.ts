import { IEditJobScheduleCommonContract } from './IEditJobScheduleCommonContract'
import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse EditJob
export interface IEditJobContract extends IEditJobScheduleCommonContract {
    // Das zu verwendende Aufzeichnungsverzeichnis
    directory: string

    // Das zu verwendende Gerät
    device: string

    // Gesetzt, wenn die Aufzeichnung auf jeden Fall auf dem angegebenen Geräte erfolgen soll
    lockedToDevice: boolean
}

export function getRecordingDirectories(): Promise<string[] | undefined> {
    return doUrlCall('info?directories')
}
