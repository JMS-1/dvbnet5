// Repräsentiert die Klasse SourceInformation
export interface ISourceInformationContract {
    // Der volle Name der Quelle
    nameWithProvider: string

    // Gesetzt, wenn die Quelle verschlüsselt ist
    encrypted: boolean
}
