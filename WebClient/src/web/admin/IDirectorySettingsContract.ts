import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse DirectorySettings
export interface IDirectorySettingsContract extends ISettingsContract {
    // Alle Aufzeichnungsverzeichnisse
    directories: string[]

    // Das Muster für die Erzeugung von Dateinamen
    pattern: string
}

export function getDirectorySettings(): Promise<IDirectorySettingsContract | undefined> {
    return doUrlCall('configuration/folder')
}

export function setDirectorySettings(data: IDirectorySettingsContract): Promise<boolean | undefined> {
    return doUrlCall('configuration/folder', 'PUT', data)
}

export function validateDirectory(path: string): Promise<boolean | undefined> {
    return doUrlCall(`configuration/validate?directory=${encodeURIComponent(path)}`)
}

export function browseDirectories(root: string, children: boolean): Promise<string[] | undefined> {
    return doUrlCall(`configuration/browse?toParent=${!children}&root=${encodeURIComponent(root)}`)
}
