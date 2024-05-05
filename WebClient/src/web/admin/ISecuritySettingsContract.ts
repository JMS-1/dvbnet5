import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse SecuritySettings
export interface ISecuritySettingsContract extends ISettingsContract {
    // Die Windows Gruppe der normalen Benutzer
    users: string

    // Die Windows Gruppe der Administratoren
    admins: string
}

export function getSecuritySettings(): Promise<ISecuritySettingsContract | undefined> {
    return doUrlCall('configuration/security')
}

export function setSecuritySettings(data: ISecuritySettingsContract): Promise<boolean | undefined> {
    return doUrlCall('configuration/security', 'PUT', data)
}

export function getWindowsGroups(): Promise<string[] | undefined> {
    return doUrlCall('info/groups')
}
