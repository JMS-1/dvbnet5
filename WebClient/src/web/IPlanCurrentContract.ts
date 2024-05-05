import { doUrlCall } from './VCRServer'

export interface IPlanCurrentContract {
    // Das Gerät, auf dem die Aktivität stattfindet
    profileName: string

    // Der Name der Aktivität
    name: string

    // Die Kennung der Quelle
    source: string

    // Der Name der Quelle
    sourceName: string

    // Der Startzeitpunkt in ISO Notation
    startTimeISO: string

    // Die Dauer in Sekunden
    durationInSeconds: number

    // Gesetzt, wenn Daten aus der Programmzeitschrift für die Dauer der Aktivität vorliegen
    hasGuideEntry: boolean

    // Eine eindeutige Kennung einer Aufzeichnung zum Abruf der Detailinformationen
    identifier: string

    // Eine eindeutige Kennung einer laufenden Aufzeichnung oder Aufgabe, mit Hilfe derer diese beendet werden kann
    planIdentifier: string

    // Gesetzt, wenn eine zukünftige Aktivität verspätet beginnen wird
    isLate: boolean

    // Zeigt an, dass dieser Eintrag nur ein Platzhalter für ein Gerät ist, für das keine Planungsdaten vorliegen.
    isIdle: boolean

    // Hinweistext mit einer Größenangabe
    sizeHint: string

    // Die interne laufende Nummer des Aufzeichnungsdatenstroms
    index: number

    // Optional die TCP/IP Adresse, an die gerade ein Netzwerkversand stattfindet
    streamTarget: string

    // Die verbleibende Anzahl von Minuten einer aktiven Aufzeichnung oder Aufgabe
    remainingTimeInMinutes: number
}

export function getPlanCurrent(): Promise<IPlanCurrentContract[] | undefined> {
    return doUrlCall('plan/current')
}

export function updateEndTime(
    device: string,
    suppressHibernate: boolean,
    scheduleIdentifier: string,
    newEnd: Date
): Promise<void> {
    return doUrlCall<void, void>(
        `profile/endtime/${device}?disableHibernate=${suppressHibernate}&schedule=${scheduleIdentifier}&endTime=${newEnd.toISOString()}`,
        'PUT'
    )
}

export function triggerTask(taskName: 'guide' | 'scan'): Promise<void> {
    return doUrlCall<void, void>(`plan/${taskName}`, 'POST')
}
