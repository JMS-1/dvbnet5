import { IPlanExceptionContract } from './IPlanExceptionContract'
import { doUrlCall } from './VCRServer'

// Beschreibt einen Eintrag im Aufzeichnungsplan.
export interface IPlanActivityContract {
    // Beginn der Aufzeichnung im ISO Format.
    startTimeISO?: string

    // Dauer der Aufzeichung in Sekunden.
    durationInSeconds: string

    // Name der Aufzeichnung.
    fullName: string

    // Gerät, auf dem aufgezeichnet wird.
    device?: string

    // Sender, von dem aufgezeichnet wird.
    station?: string

    // Gesetzt, wenn die Aufzeichnung verspätet beginnt.
    isLate: boolean

    // Gesetzt, wenn die Aufzeichnung gar nicht ausgeführt wird.
    isHidden: boolean

    // Gesetzt, wenn Informationen aus der Programmzeitschrift vorliegen.
    hasGuideEntry: boolean

    // Das Gerät, in dessen Programmzeitschrift die Aufzeichnung gefunden wurde.
    guideEntryDevice?: string

    // Die Quelle zur Aufzeichnung in der Programzeitschrift.
    source?: string

    // Die eindeutige Kennung der Aufzeichnung.
    legacyReference: string

    // Gesetzt, wenn die Endzeit durch eine Sommer-/Winterzeitumstellung nicht korrekt ist.
    endTimeCouldBeWrong: boolean

    // Gesetzt, wenn alle Tonspuren aufgezeichnet werden sollen.
    allLanguages: boolean

    // Gesetzt, wenn Dolby Tonspuren aufgezeichnet werden sollen.
    dolby: boolean

    // Gesetzt, wenn der Videotext mit aufgezeichnet werden soll.
    videoText: boolean

    // Gesetzt, wenn DVB Untertitel mit aufgezeichnet werden sollen.
    subTitles: boolean

    // Gesetzt, wenn die Aufzeichnung laut Programmzeitschrift gerade läuft.
    currentProgramGuide: boolean

    // Aktive Ausnahmeregel für die Aufzeichnung.
    exceptionRule?: IPlanExceptionContract
}

export function getPlan(limit: number, end: Date): Promise<IPlanActivityContract[] | undefined> {
    return doUrlCall(`plan?limit=${limit}&end=${end.toISOString()}`)
}
