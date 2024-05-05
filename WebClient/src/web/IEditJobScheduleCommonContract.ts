// Gemeinsame Schnittstelle der Klassen EditSchedule und EditJob
export interface IEditJobScheduleCommonContract {
    // Der Name der Aufzeichnung
    name: string

    // Die verwendete Quelle
    source: string

    // Gesetzt, wenn alle Sprachen aufgezeichnet werden sollen
    allLanguages: boolean

    // Gesetzt, wenn die Dolby Digital Tonspur aufgezeichnet werden soll
    dolbyDigital: boolean

    // Gesetzt, wenn der Videotext aufgezeichnet werden soll
    videotext: boolean

    // Gesetzt, wenn Untertitel aufgezeichnet werden sollen
    dvbSubtitles: boolean
}
