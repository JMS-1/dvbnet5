import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse ProfileInfo
export interface IProfileInfoContract {
    name: string
}

export function getProfileInfos(): Promise<IProfileInfoContract[] | undefined> {
    return doUrlCall('profile/profiles')
}
