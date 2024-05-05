// Repräsentiert die Klasse InfoSchedule
export interface IInfoScheduleContract {
    // Der Name der Aufzeichnung
    name: string

    // Der erste Start der Aufzeichnung in ISO Notation
    startTimeISO: string

    // Die Wochentage, an denen die Aufzeichnugn wiederholt werden soll
    repeatPatternJSON: number

    // Der Name der Quelle
    source: string

    // Die eindeutige Kennung der Aufzeichnung
    webId: string
}
