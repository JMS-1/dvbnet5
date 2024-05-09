import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse OtherSettings
export interface IOtherSettingsContract extends ISettingsContract {
    // Die Verweildauer von Aufträgen im Archiv (in Wochen)
    archiveTime: number

    // Die Verweildauer von Protokollen (in Wochen)
    protocolTime: number

    // Gesetzt, um eine PCR Erzeugunbg bei H.264 Material zu vermeiden
    disablePCRFromH264: boolean

    // Gesetzt, um eine PCR Erzeugung aus MPEG-2 Material zu vermeiden
    disablePCRFromMPEG2: boolean

    // Die Art der Protokollierung
    logging: string
}

export function getOtherSettings(): Promise<IOtherSettingsContract | undefined> {
    return doUrlCall('configuration/other')
}

export function setOtherSettings(data: IOtherSettingsContract): Promise<boolean | undefined> {
    return doUrlCall('configuration/other', 'PUT', data)
}
