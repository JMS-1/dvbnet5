// Repräsentiert die Klasse ConfigurationProfile
export interface IProfileContract {
    // Der Name des Gerätes
    name: string

    // Gesetzt, wenn es für Aufzeichnungen verwendet werden darf
    usedForRecording: boolean

    // Die maximale Anzahl gleichzeitig empfangener Quellen
    sourceLimit: number

    // Die maximale Anzahl gleichzeitig entschlüsselbarer Quellen
    decryptionLimit: number

    // Die Aufzeichnungspriorität
    priority: number
}
