import { DurationEditor, IDurationEditor } from './duration'
import { IScheduleException, ScheduleException } from './exception'
import { IJobScheduleEditor, JobScheduleEditor } from './jobSchedule'

import { DateTimeUtils } from '../../../lib/dateTimeUtils'
import { FlagSet, IFlag } from '../../../lib/edit/boolean/flag'
import { DayProperty, IDaySelector } from '../../../lib/edit/datetime/day'
import { INumber, NumberProperty } from '../../../lib/edit/number/number'
import { IEditScheduleContract } from '../../../web/IEditScheduleContract'
import { IPage } from '../page'

// Schnittstelle zur Pflege einer Aufzeichnung.
export interface IScheduleEditor extends IJobScheduleEditor {
    // Datum der ersten Aufzeichnung.
    readonly firstStart: IDaySelector

    // Laufzeit der Aufzeichnung.
    readonly duration: IDurationEditor

    // Wiederholungsmuster als Ganzes und aufgespalten als Wahrheitswert pro Wochentag.
    readonly repeat: INumber

    readonly onMonday: IFlag
    readonly onTuesday: IFlag
    readonly onWednesday: IFlag
    readonly onThursday: IFlag
    readonly onFriday: IFlag
    readonly onSaturday: IFlag
    readonly onSunday: IFlag

    // Ende der Wiederholung.
    readonly lastDay: IDaySelector

    // Bekannte Ausnahmen der Wiederholungsregel.
    readonly hasExceptions: boolean

    readonly exceptions: IScheduleException[]
}

// Beschreibt die Daten einer Aufzeichnung.
export class ScheduleEditor extends JobScheduleEditor<IEditScheduleContract> implements IScheduleEditor {
    // Erstellt ein neues Präsentationsmodell.
    constructor(
        page: IPage,
        model: IEditScheduleContract,
        favoriteSources: string[],
        onChange: () => void,
        hasJobSource: () => boolean
    ) {
        super(page, model, favoriteSources, onChange)

        // Anpassungen.
        if (!model.lastDayISO) model.lastDayISO = ScheduleEditor.maximumDate.toISOString()

        // Pflegbare Eigenschaften anlegen.
        this.repeat = new NumberProperty(model, 'repeatPattern', 'Wiederholung', () => this.onChange(onChange))
        this.duration = new DurationEditor(model, 'firstStart', 'duration', 'Zeitraum', () => this.onChange(onChange))
        this.firstStart = new DayProperty(model, 'firstStart', 'Datum', onChange).addValidator(() =>
            this.validateFirstRecording()
        )
        this.lastDay = new DayProperty(model, 'lastDay', 'wiederholen bis zum', () => this.onChange(onChange), true)
            .addRequiredValidator()
            .addValidator((day) => ScheduleEditor.validateDateRange(day))

        this.onMonday = new FlagSet(ScheduleEditor.flagMonday, this.repeat, DateTimeUtils.germanDays[1])
        this.onTuesday = new FlagSet(ScheduleEditor.flagTuesday, this.repeat, DateTimeUtils.germanDays[2])
        this.onWednesday = new FlagSet(ScheduleEditor.flagWednesday, this.repeat, DateTimeUtils.germanDays[3])
        this.onThursday = new FlagSet(ScheduleEditor.flagThursday, this.repeat, DateTimeUtils.germanDays[4])
        this.onFriday = new FlagSet(ScheduleEditor.flagFriday, this.repeat, DateTimeUtils.germanDays[5])
        this.onSaturday = new FlagSet(ScheduleEditor.flagSaturday, this.repeat, DateTimeUtils.germanDays[6])
        this.onSunday = new FlagSet(ScheduleEditor.flagSunday, this.repeat, DateTimeUtils.germanDays[0])

        // Ausnahmeregeln.
        this.exceptions = (model.exceptions || []).map(
            (e) => new ScheduleException(e, () => this.onExceptionsChanged())
        )

        // Zusätzliche Prüfung einrichten.
        this.source.sourceName.addValidator((c) => {
            if (!hasJobSource())
                if ((c.value || '').trim().length < 1)
                    return 'Entweder für die Aufzeichnung oder für den Auftrag muss eine Quelle angegeben werden.'
        })

        // Initiale Prüfung.
        this.repeat.validate()
        this.lastDay.validate()
        this.firstStart.validate()
        this.source.sourceName.validate()
    }

    // Bei Änderungen an den Aufzeichnungsdaten muss eine übergreifende Gesamtprüfungen stattfinden, die wir an das Startdatum gebunden haben.
    private onChange(onOuterChange: () => void): void {
        // Gesamtprüfung anstossen.
        this.lastDay.validate()
        this.firstStart.validate()

        // Durchreichen.
        onOuterChange()
    }

    // Datum der ersten Aufzeichnung.
    readonly firstStart: DayProperty

    // Uhrzeit der ersten Aufzeichnung.
    readonly duration: DurationEditor

    // Muster zur Wiederholung.
    readonly repeat: NumberProperty

    // Ende der Wiederholung
    readonly lastDay: DayProperty

    // Bekannte Ausnahmen der Wiederholungsregel.
    get hasExceptions(): boolean {
        return this.exceptions.length > 0
    }

    readonly exceptions: ScheduleException[]

    // Hilfsmethode zum Arbeiten mit Datumswerten.
    private static makePureDate(date: Date): Date {
        return new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()))
    }

    // Der kleinste erlaubte Datumswert.
    static readonly minimumDate = ScheduleEditor.makePureDate(new Date(1963, 8, 29))

    // Der höchste erlaubte Datumswert.
    static readonly maximumDate = ScheduleEditor.makePureDate(new Date(2099, 11, 31))

    // Der höchste erlaubte Datumswert.
    private static readonly maximumDateLegacy = ScheduleEditor.makePureDate(new Date(2999, 11, 31))

    // Das Bit für Montag.
    static readonly flagMonday: number = 0x01

    readonly onMonday: FlagSet

    // Das Bit für Dienstag.
    static readonly flagTuesday: number = 0x02

    readonly onTuesday: FlagSet

    // Das Bit für Mittwoch.
    static readonly flagWednesday: number = 0x04

    readonly onWednesday: FlagSet

    // Das Bit für Donnerstag.
    static readonly flagThursday: number = 0x08

    readonly onThursday: FlagSet

    // Das Bit für Freitag.
    static readonly flagFriday: number = 0x10

    readonly onFriday: FlagSet

    // Das Bit für Samstag.
    static readonly flagSaturday: number = 0x20

    readonly onSaturday: FlagSet

    // Das Bit für Sonntag.
    static readonly flagSunday: number = 0x40

    readonly onSunday: FlagSet

    // Die Bitmasken aller Wochentage in der Ordnung von JavaScript (Date.getDay()).
    private static readonly _flags = [
        ScheduleEditor.flagSunday,
        ScheduleEditor.flagMonday,
        ScheduleEditor.flagTuesday,
        ScheduleEditor.flagWednesday,
        ScheduleEditor.flagThursday,
        ScheduleEditor.flagFriday,
        ScheduleEditor.flagSaturday,
    ]

    // Prüft ob eon ausgewähltes Datum im unterstützten Bereich liegt.
    private static validateDateRange(day: DayProperty): string | undefined {
        const lastDay = new Date(day.value ?? 0)

        if (lastDay < ScheduleEditor.minimumDate) return 'Datum liegt zu weit in der Vergangenheit.'
        else if (lastDay > ScheduleEditor.maximumDateLegacy) return 'Datum liegt zu weit in der Zukunft.'
    }

    // Prüft ob die Aufzeichnung überhaupt einmal stattfinden wird.
    private validateFirstRecording(): string | undefined {
        // Der letzte Tage einer Wiederholung.
        const lastDay = new Date(this.lastDay.value ?? 0)

        // Geplanter erster (evt. einziger Start).
        let start = new Date(this.firstStart.value ?? 0)

        // Die echte Dauer unter Berücksichtigung der Zeitumstellung ermitteln.
        const duration = DateTimeUtils.getRealDurationInMinutes(this.firstStart.value ?? '', this.duration.value ?? 0)

        // Ende der ersten Aufzeichnung ermitteln - das sollte in den meisten Fällen schon passen.
        let end = new Date(
            start.getFullYear(),
            start.getMonth(),
            start.getDate(),
            start.getHours(),
            start.getMinutes() + duration
        )

        // Aktuelle Uhrzeit ermitteln.
        const now = new Date()

        // Ansonsten kann uns nur noch das Wiederholen retten.
        const repeat = this.repeat.value

        if (repeat !== 0) {
            // Zur Vereinfachung der Vergleiche beginnen wir etwas vor dem aktuellen Tag.
            start = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 2, start.getHours(), start.getMinutes())

            // Von dort aus schauen wir in die Zukunft.
            for (;;) {
                // Den nächsten Wochentag suchen, an dem eine Wiederholung erlaubt ist.
                do {
                    // Dabei den Startzeitpunkt immer um einen Tag vorrücken, bis es passt.
                    start = new Date(
                        start.getFullYear(),
                        start.getMonth(),
                        start.getDate() + 1,
                        start.getHours(),
                        start.getMinutes()
                    )
                } while ((ScheduleEditor._flags[start.getDay()] & (repeat ?? 0)) === 0)

                // Dazu das eine Datum ermitteln - UTC, da auch unser Enddatum UTC ist.
                const startDay = ScheduleEditor.makePureDate(start)

                // Der Startzeitpunkt ist leider verboten.
                if (startDay > lastDay) break

                // Nun müssen wir uns das zugehörige Ende der Aufzeichnung anschauen.
                end = new Date(
                    start.getFullYear(),
                    start.getMonth(),
                    start.getDate(),
                    start.getHours(),
                    start.getMinutes() + duration
                )

                // Liegt dieses echt in der Zukunft ist alles gut.
                if (end > now) return
            }
        }
        // Wenn die Aufzeichnung in der Zukunft endet ist alles gut.
        else if (end > now) return

        // Die Aufzeichnung findet sicher niemals statt.
        return 'Die Aufzeichnung liegt in der Vergangenheit.'
    }

    // Gesetzt, wie die Daten der Aufzeichnung konsistent sind.
    isValid(): boolean {
        // Erst einmal die Basisklasse fragen.
        if (!super.isValid()) return false

        // Dann alle unseren eigenen Präsentationsmodelle.
        if (this.repeat.message) return false
        if (this.firstStart.message) return false
        if (this.repeat.value !== 0) if (this.lastDay.message) return false
        if (!this.duration.isValid()) return false

        return true
    }

    // Die Liste der Ausnahmen wird immer mit aktualisiert.
    private onExceptionsChanged(): void {
        // Alle übernehmen, die nicht explizit zum Löschen deaktiviert wurden.
        this.model.exceptions = this.exceptions.filter((e) => e.isActive.value).map((e) => e.model)
    }
}
