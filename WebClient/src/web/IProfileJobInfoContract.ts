import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse ProfileJobInfo
export interface IProfileJobInfoContract {
    // Der Name des Auftrags
    name: string

    // Die eindeutige Kennung des Auftrags
    id: string
}

export function getProfileJobInfos(device: string): Promise<IProfileJobInfoContract[] | undefined> {
    return doUrlCall(`profile/${device}?activeJobs`)
}
