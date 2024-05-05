import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse PlanException
export interface IPlanExceptionContract {
    // Der zugehörige Tag als interner Schlüssel, der unverändert zwischen Client und Service ausgetauscht wird
    exceptionDateTicks: string

    // Der zugehörige Tag repräsentiert Date.getTime() Repräsentation
    exceptionDateUnix: string

    // Die aktuelle Verschiebung des Startzeitpunktes in Minuten
    exceptionStartShift: number

    // Die aktuelle Veränderung der Laufzeit in Minuten
    exceptionDurationDelta: number

    // Der ursprüngliche Startzeitpunkt in ISO Notation
    plannedStartISO: string

    // Die ursprüngliche Dauer in Minuten
    plannedDuration: number
}

export function updateException(
    legacyId: string,
    referenceDay: string,
    startDelta: number,
    durationDelta: number
): Promise<void> {
    return doUrlCall<void, void>(
        `exception/${legacyId}?when=${referenceDay}&startDelta=${startDelta}&durationDelta=${durationDelta}`,
        'PUT'
    )
}
