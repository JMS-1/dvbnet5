import { IEditJobContract } from './IEditJobContract'
import { IEditScheduleContract } from './IEditScheduleContract'
import { IJobScheduleInfoContract } from './IJobScheduleInfoContract'
import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse JobScheduleData
export interface IJobScheduleDataContract {
    // Der Auftrag
    job: IEditJobContract

    // Die Aufzeichnung im Auftrag
    schedule: IEditScheduleContract
}

export function createScheduleFromGuide(
    legacyId: string,
    epgId: string
): Promise<IJobScheduleInfoContract | undefined> {
    return doUrlCall(`edit/${legacyId}?epg=${epgId}`)
}

export function updateSchedule(jobId: string, scheduleId: string, data: IJobScheduleDataContract): Promise<void> {
    let method = 'POST'
    let url = 'edit'

    if (jobId != null) {
        url += '/' + jobId

        if (scheduleId != null) {
            url += scheduleId

            method = 'PUT'
        }
    }

    // Befehl ausführen
    return doUrlCall<void, IJobScheduleDataContract>(url, method, data)
}
