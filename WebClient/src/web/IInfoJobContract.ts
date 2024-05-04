import { IInfoScheduleContract } from './IInfoScheduleContract'
import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse InfoJob
export interface IInfoJobContract {
    // Der Name des Auftrags
    name: string

    // Die eindeutige Kennung des Auftrags
    id: string

    // Alle Aufzeichnung zum Auftrag
    schedules: IInfoScheduleContract[]

    // Gesetzt, wenn der Auftrag noch nicht in das Archiv übertragen wurde
    active: boolean
}

export function getInfoJobs(): Promise<IInfoJobContract[] | undefined> {
    return doUrlCall('info?jobs')
}
