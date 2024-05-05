import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse GuideInfo
export interface IGuideInfoContract {
    // Alle Quellen eines Gerätes, für das Einträge in der Programmzeitschrift zur Verfügung stehen
    sourceNames: string[]

    // Der Zeitpunkt in ISO Notation, an dem die früheste Sendung beginnt
    firstStartISO: string

    // Der Zeitpunkt in ISO Notation, an dem die späteste Sendung beginnt
    lastStartISO: string
}

export function getGuideInfo(device: string): Promise<IGuideInfoContract | undefined> {
    return doUrlCall(`guide/info/${device}`)
}
