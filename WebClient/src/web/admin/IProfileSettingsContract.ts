import { IProfileContract } from '../IProfileContract'
import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse ProfileSettings
export interface IProfileSettingsContract extends ISettingsContract {
    // Alle DVB.NET Geräte auf dem Rechner, auf dem der VCR.NET Recording Service läuft
    profiles: IProfileContract[]

    // Das bevorzugte Gerät für neue Aufzeichnungen
    defaultProfile: string
}

export function getProfileSettings(): Promise<IProfileSettingsContract | undefined> {
    return doUrlCall('configuration/profiles')
}

export function setProfileSettings(data: IProfileSettingsContract): Promise<boolean | undefined> {
    return doUrlCall('configuration/profiles', 'PUT', data)
}
