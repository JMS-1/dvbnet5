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
    return doUrlCall(`edit/recording/${legacyId}?epg=${epgId}`)
}

export function updateSchedule(jobId: string, scheduleId: string, data: IJobScheduleDataContract): Promise<void> {
    let method = 'POST'
    let url = 'edit/job'

    if (jobId) {
        url = 'edit/recording/' + jobId

        if (scheduleId) {
            url += scheduleId

            method = 'PUT'
        }
    }

    // Befehl ausführen
    return doUrlCall<void, IJobScheduleDataContract>(url, method, data)
}
