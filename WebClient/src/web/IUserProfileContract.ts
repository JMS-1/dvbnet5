import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse UserProfile
export interface IUserProfileContract {
    // Die Anzahl der Einträge im Aufzeichnungsplan
    planDays: number

    // Die Liste der zuletzt verwendeten Quellen
    recentSources: string[]

    // Die Art der Quelle
    typeFilter: string

    // Die Art der Verschlüsselung
    encryptionFilter: string

    // Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen
    languages: boolean

    // Gesetzt, wenn die Dolby Digital Tonspur aufgezeichnet werden soll
    dolby: boolean

    // Gesetzt, wenn der Videotext aufgezeichnet werden soll
    videotext: boolean

    // Gesetzt, wenn DVB Untertitel aufgezeichnet werden sollen
    subtitles: boolean

    // Gesetzt, wenn bei der Programmierung aus der Programmzeitschrift heraus danach in diese zurück gekehrt werden soll
    backToGuide: boolean

    // Die Anzahl der Sendungen auf einer Seite der Programmzeitschrift
    guideRows: number

    // Die Vorlaufzeit bei Programmierung aus der Programmzeitschrift heraus
    guideAheadStart: number

    // Die Nachlaufzeit bei Programmierung aus der Programmzeitschrift heraus
    guideBeyondEnd: number

    // Die maximal erlaubte Anzahl von zuletzt verwendeten Quellen
    recentSourceLimit: number

    // Die gespeicherten Suchen der Programmzeitschrift
    guideSearches: string
}

export function getUserProfile(): Promise<IUserProfileContract | undefined> {
    return doUrlCall('user')
}

export function setUserProfile(profile: IUserProfileContract): Promise<void> {
    return doUrlCall('user', 'PUT', profile)
}
