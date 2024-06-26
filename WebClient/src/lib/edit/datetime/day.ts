﻿import { Command, ICommand } from '../../command/command'
import { DateTimeUtils } from '../../dateTimeUtils'
import { IDisplay } from '../../localizable'
import { IConnectable } from '../../site'
import { Property } from '../edit'

// Schnittstelle zur Anzeige eines einzelnen Tags.
export interface ISelectableDay {
    // Das volle Datum.
    readonly date: Date

    // Die Nummer des Tags im Monat.
    readonly display: string

    // Gesetzt, wenn sich der Tag im aktuellen Monat befindet - und nicht ein Überhang aus den Nachbarmonaten ist.
    readonly isCurrentMonth: boolean

    // Gesetzt, wenn es sich um den aktuell ausgewählten Tag handelt.
    readonly isCurrentDay: boolean

    // Gesetzt, wenn der Tag das heute Datum bezeichnet.
    readonly isToday: boolean

    // Wird zur Auswahl des Tags ausgelöst - nur gesetzt, wenn es sich nicht bereits um den aktuell ausgewählten Tag handelt.
    select?(): void
}

// Schnittstelle zur Auswahl eines Tags.
export interface IDaySelector extends IDisplay, IConnectable {
    // Blättert einen Monat zurück.
    readonly monthBackward: ICommand

    // Blättert einen Monat vor.
    readonly monthForward: ICommand

    // Verschiebt die Anzeige auf den aktuellen Monat.
    readonly today: ICommand

    // Setzt die Anzeige auf den Monat mit dem ausgewählten Tag zurück.
    readonly reset: ICommand

    // Meldet oder setzt den aktuellen Monat.
    month: string

    // Die Anzeigetexte aller Monate.
    readonly months: string[]

    // Meldet oder setzt das aktuelle Jahr.
    year: string

    // Alle direkt zur Auswahl angebotenen Jahre.
    readonly years: string[]

    // Die Namen aller Tage - genutzt als Spaltenüberschriften.
    readonly dayNames: string[]

    // Alle anzuzeigenden Tage - je 7 pro Woche mit einer ganzen Anzahl von Wochen.
    readonly days: ISelectableDay[]

    // Eine optionale Fehlermeldung.
    readonly message?: string
}

// Präsentastionsmodell zur Auswahl eines Tags.
export class DayProperty<TDataType> extends Property<TDataType, string> implements IDaySelector {
    // Die Namen aller Wochentage, wobei wir die Woche am Montag beginnen lassen.
    readonly dayNames = DateTimeUtils.germanDays.map((d, i) => DateTimeUtils.germanDays[(i + 1) % 7])

    // Auf den Vormonat wechseln.
    readonly monthBackward = new Command(() => this.goBackward())

    private goBackward(): void {
        // Aktuelle Auswahl prüfen.
        let monthIndex = this.months.indexOf(this._month)
        if (monthIndex < 0) return

        // Monat anpassen.
        if (monthIndex-- === 0) {
            monthIndex = 11

            // Jahr anpassen.
            this._year = `${parseInt(this._year) - 1}`
        }

        // Textrepräsentation merken.
        this._month = this.months[monthIndex]

        // Anzeige aktualisieren.
        this.fillSelectors()
    }

    // Auf den Folgemonat wechseln.
    readonly monthForward = new Command(() => this.goForward())

    private goForward(): void {
        // Laufende Nummer ermitteln.
        let monthIndex = this.months.indexOf(this._month)
        if (monthIndex < 0) return

        // Monat anpassen.
        if (monthIndex++ === 11) {
            monthIndex = 0

            // Jahr mitführen.
            this._year = `${parseInt(this._year) + 1}`
        }

        // Anzeigetext des Monats setzen.
        this._month = this.months[monthIndex]

        // Anzeige aktualisieren.
        this.fillSelectors()
    }

    // Anzeige auf den aktuellen Monat setzen.
    readonly today = new Command(
        () => this.goToday(),
        'Heute',
        () => this.days.every((d) => !d.isToday)
    )

    private goToday(): void {
        this.moveTo(new Date())
    }

    // Anzeige auf den Monat mit der aktuellen Auswahl setzen.
    readonly reset = new Command(
        () => this.goSelected(),
        'Zurück',
        () => this.days.every((d) => !d.isCurrentDay)
    )

    private goSelected(): void {
        this.moveTo(this.day)
    }

    // Anzeige auf einen bestimmten Tag anpassen.
    private moveTo(date: Date): void {
        // Anzeigetexte ermitteln.
        this._month = this.months[date.getMonth()]
        this._year = `${date.getFullYear()}`

        // Anzeige aktualisieren.
        this.fillSelectors()
    }

    // Die Anzeigenamen aller Monate.
    private static _months = [
        'Januar',
        'Februar',
        'März',
        'April',
        'Mai',
        'Juni',
        'Juli',
        'August',
        'September',
        'Oktober',
        'November',
        'Dezember',
    ]

    readonly months: string[] = DayProperty._months

    // Verwaltet den aktuellen Monat.
    private _month: string

    get month(): string {
        return this._month
    }

    set month(newMonth: string) {
        // Es ist gar keine echte Änderung.
        if (newMonth === this._month) return

        // Monat vermerken.
        this._month = newMonth

        // Anzeige aktualisieren.
        this.fillSelectors()

        // Anzeige aktualisieren.
        this.refresh()
    }

    // Verwaltet das aktuell ausgewählte Jahr.
    private _year: string

    get year(): string {
        return this._year
    }

    set year(newYear: string) {
        // Keine Änderung.
        if (newYear === this._year) return

        // Änderung übernehmen.
        this._year = newYear

        // Anzeige aktualisieren.
        this.fillSelectors()

        // Anzeige aktualisieren.
        this.refresh()
    }

    // Die Anzeigetexte aller zur Auswahl angebotenen Jahre.
    years: string[]

    // Die aktuell zur Auswahl anzubietenden Wochentage.
    days: ISelectableDay[]

    // Der aktuell ausgewählte Tag.
    private get day(): Date {
        // Aktuelles Datum auslesen.
        let oldDay = new Date(this.value ?? 0)

        // Wird nur ein Datum (ohne Mitpflegen der Uhrzeit) verwaltet, verwenden wird eine interne UTC Repräsentation.
        if (this._pureDate) oldDay = new Date(oldDay.getUTCFullYear(), oldDay.getUTCMonth(), oldDay.getUTCDate())

        // Die aktuelle Auswahl.
        return oldDay
    }

    private set day(newDay: Date) {
        // Der aktuelle Wert.
        const oldDay = this.day

        // Bei einem reinen Datum die Uhrzeit ausblenden.
        if (this._pureDate) newDay = new Date(Date.UTC(newDay.getFullYear(), newDay.getMonth(), newDay.getDate()))

        // Gespeichert wird das Datum (mit oder ohne Uhrzeit) als ISO Zeichenkette.
        this.value = newDay.toISOString()
    }

    // Bereitet die Anzeige nach dem Anmelden eines Oberflächenelementes vor.
    protected onSiteChanged(): void {
        this.goSelected()
    }

    // Auswahlliste für das Jahr vorbereiten - fünf Jahre vor und fünf nach der aktuellen Auswahl.
    private fillYearSelector(): void {
        // Die aktuelle Auswahl passt schon zu Liste.
        if (this.years) if (this.years[5] === this._year) return

        // Aktuelles Jahr ermitteln.
        const year = parseInt(this._year)

        // Auswahlliste füllen.
        this.years = []

        for (let i = -5; i <= +5; i++) this.years.push(`${year + i}`)
    }

    // Die Liste der auswählbaren Tage erstellen - im Wesentlichen der aktuelle Monat und die Tage davor und danach, die zu jeweils vollen Wochen benötigt werden.
    private fillDaySelector(): void {
        // Aktuelle Auswahl.
        const year = parseInt(this._year)
        const month = this.months.indexOf(this._month)

        // Der erste des aktuellen Monats.
        let current = new Date(year, month, 1)

        // Zurück auf den Montag der Woche mit dem 1.
        current = new Date(year, month, 1 - ((current.getDay() + 6) % 7))

        // Zum Vergleich auch noch heute.
        let now = new Date()

        // Aber nur das Datum.
        now = new Date(now.getFullYear(), now.getMonth(), now.getDate())

        // Die aktuelle Auswahl.
        let selected = this.day

        // Auch hier nur das Datum.
        selected = new Date(selected.getFullYear(), selected.getMonth(), selected.getDate())

        this.days = []

        // Alle Wochen des aktuellen Monats durchgehen.
        do {
            // Jeweils sieben Tage auf einen Rutsch und dabei immer einen Tag weiter gehen.
            for (
                let i = 7;
                i-- > 0;
                current = new Date(current.getFullYear(), current.getMonth(), current.getDate() + 1)
            ) {
                // Auswahlelement vorbereiten.
                const day: ISelectableDay = {
                    date: current,
                    display: `${current.getDate()}`,
                    isCurrentDay: current.getTime() === selected.getTime(),
                    isCurrentMonth: current.getMonth() === month,
                    isToday: current.getTime() === now.getTime(),
                }

                // Auswahl ausser für die aktuelle Auswahl ermöglichen.
                if (!day.isCurrentDay) day.select = () => this.selectDay(day)

                // Die Konfiguration merken wir uns.
                this.days.push(day)
            }
        } while (current.getMonth() === month)
    }

    // Erstelle ein präsentationsmodell zur Auswahl eines Datums.
    constructor(
        data: TDataType,
        prop: keyof TDataType,
        text?: string,
        onChange?: () => void,
        private _pureDate: boolean = false
    ) {
        super(data, prop, text, onChange)
    }

    // Wählt ein neues Datum aus.
    private selectDay(day: ISelectableDay): void {
        const oldSelected = this.day

        // Die neue Auswahl erhält die Uhrzeit eingemischt.
        const newSelected = new Date(
            day.date.getFullYear(),
            day.date.getMonth(),
            day.date.getDate(),
            oldSelected.getHours(),
            oldSelected.getMinutes(),
            oldSelected.getSeconds(),
            oldSelected.getMilliseconds()
        )

        // Aktuelle Auswahl verändern.
        this.day = newSelected

        // Anzeige anpassen.
        this.goSelected()
    }

    // Aktualisiert die Anzeige.
    private fillSelectors(): void {
        // Auswahllisten neu aufbauen.
        this.fillDaySelector()
        this.fillYearSelector()

        // Anzeige aktualisieren.
        this.refresh()
    }

    // Ergänzt eine Prüfung auf einen vorhandenen Wert.
    addRequiredValidator(message = 'Es muss ein Datum ausgewählt werden.'): this {
        return this.addValidator(() => {
            if (this.value === null) return message
        })
    }
}
