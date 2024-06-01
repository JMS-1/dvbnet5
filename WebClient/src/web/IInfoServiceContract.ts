import { doUrlCall, setFtpPort } from './VCRServer'

// Repräsentiert die Klasse InfoService
export interface IInfoServiceContract {
    // Die aktuelle Version des Dienstes in der Notation MAJOR.MINOR [DD.MM.YYYY]
    version: string

    // Die aktuelle Version der Installation in der Notation MAJOR.MINOR.BUILD
    installedVersion: string

    // Gesetzt, wenn mindestens ein Gerät eine Aufzeichnung oder Aufgabe ausführt
    isRunning: boolean

    // Gesetzt, wenn die Aktualisierung der Quellen verfügbar ist
    sourceScanEnabled: boolean

    // Gesetzt, wenn die Aktualisierung der Programmzeitschrift verfügbar ist
    guideUpdateEnabled: boolean

    // FTP port für das Demux.
    ftpPort: number
}

export function getServerVersion(): Promise<IInfoServiceContract | undefined> {
    return doUrlCall<IInfoServiceContract | undefined, never>('info').then((i) => {
        if (i) setFtpPort(i.ftpPort)

        return i
    })
}
