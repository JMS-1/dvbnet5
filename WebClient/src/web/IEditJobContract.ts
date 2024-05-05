import { IEditJobScheduleCommonContract } from './IEditJobScheduleCommonContract'
import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse EditJob
export interface IEditJobContract extends IEditJobScheduleCommonContract {
    // Das zu verwendende Aufzeichnungsverzeichnis
    recordingDirectory: string

    // Das zu verwendende Gerät
    profile: string

    // Gesetzt, wenn die Aufzeichnung auf jeden Fall auf dem angegebenen Geräte erfolgen soll
    useProfileForRecording: boolean
}

export function getRecordingDirectories(): Promise<string[] | undefined> {
    return doUrlCall('info/folder')
}
