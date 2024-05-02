module VCRServer {
    // Repräsentiert die Klasse SecuritySettings
    export interface SecuritySettingsContract extends SettingsContract {
        // Die Windows Gruppe der normalen Benutzer
        users: string

        // Die Windows Gruppe der Administratoren
        admins: string
    }

    export function getSecuritySettings(): Promise<SecuritySettingsContract | undefined> {
        return doUrlCall(`configuration?security`)
    }

    export function setSecuritySettings(data: SecuritySettingsContract): Promise<boolean | undefined> {
        return doUrlCall(`configuration?security`, `PUT`, data)
    }

    export function getWindowsGroups(): Promise<string[] | undefined> {
        return doUrlCall(`info?groups`)
    }
}
