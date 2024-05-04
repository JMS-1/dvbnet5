import { IEditJobContract } from './IEditJobContract'
import { IEditScheduleContract } from './IEditScheduleContract'

// Repräsentiert die Klasse JobScheduleInfo
export interface IJobScheduleInfoContract {
    // Der Auftrag
    job: IEditJobContract

    // Die Aufzeichnung im Auftrag
    schedule: IEditScheduleContract

    // Optional die eindeutige Kennung des Auftrags
    jobId: string

    // Optional die eindeutige Kennung der Aufzeichnung
    scheduleId: string
}
