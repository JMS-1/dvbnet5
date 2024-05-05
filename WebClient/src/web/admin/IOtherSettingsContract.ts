import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse OtherSettings
export interface IOtherSettingsContract extends ISettingsContract {
    // Gesetzt, um den Übergang in den Schlafzustand erlauben
    mayHibernate: boolean

    // Gesetzt, um StandBy für den Schlafzustand zu verwenden
    useStandBy: boolean

    // Die Verweildauer von Aufträgen im Archiv (in Wochen)
    archiveTime: number

    // Die Verweildauer von Protokollen (in Wochen)
    protocolTime: number

    // Die Vorlaufzeit beim Aufwecken aus dem Schlafzustand (in Sekunden)
    hibernationDelay: number

    // Gesetzt, um eine PCR Erzeugunbg bei H.264 Material zu vermeiden
    disablePCRFromH264: boolean

    // Gesetzt, um eine PCR Erzeugung aus MPEG-2 Material zu vermeiden
    disablePCRFromMPEG2: boolean

    // Die minimale Verweildauer im Schlafzustand
    forcedHibernationDelay: number

    // Gesetzt, wenn die minimale Verweildauer im Schlafzustand ignoriert werden soll
    suppressHibernationDelay: boolean

    // Gesetzt, um auch das Basic Protokoll zur Autentisierung zu erlauben
    allowBasic: boolean

    // Gesetzt, um die Verbindung zu verschlüsseln
    useSSL: boolean

    // Der TCP/IP Port für verschlüsselte Verbindungen
    sslPort: number

    // Der TCP/IP Port für reguläre Anfragen
    webPort: number

    // Die Art der Protokollierung
    logging: string
}

export function getOtherSettings(): Promise<IOtherSettingsContract | undefined> {
    return doUrlCall('configuration/other')
}

export function setOtherSettings(data: IOtherSettingsContract): Promise<boolean | undefined> {
    return doUrlCall('configuration/other', 'PUT', data)
}
