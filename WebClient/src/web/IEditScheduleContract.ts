import { IEditJobScheduleCommonContract } from './IEditJobScheduleCommonContract'
import { IPlanExceptionContract } from './IPlanExceptionContract'
import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse EditSchedule
export interface IEditScheduleContract extends IEditJobScheduleCommonContract {
    // Der erste Startzeitpunkt in ISO Notation
    firstStartISO: string

    // Die Wochentage, an denen eine Wiederholung stattfinden soll
    repeatPatternJSON: number

    // Der Tag, an dem die letzte Aufzeichnung stattfinden soll
    lastDayISO: string

    // Die Dauer in Minuten
    duration: number

    // Die Liste der Ausnahmeregeln
    exceptions: IPlanExceptionContract[]
}

export function deleteSchedule(jobId: string, scheduleId: string): Promise<void> {
    return doUrlCall<void, void>(`edit/recording/${jobId}${scheduleId}`, 'DELETE')
}
