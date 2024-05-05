import { DateTimeUtils } from '../../../lib/dateTimeUtils'
import { ITime, TimeProperty } from '../../../lib/edit/datetime/time'
import { Property } from '../../../lib/edit/edit'
import { IDisplay } from '../../../lib/localizable'

// Schnittstelle zur Einstellung der Dauer einer Aufzeichnung.
export interface IDurationEditor extends IDisplay {
    // Beginn (als Uhrzeit).
    readonly startTime: ITime

    // Ende (als Uhrzeit).
    readonly endTime: ITime
}

// Präsentationsmodell zur Eingabe der Dauer einer Aufzeichnung als Paar von Uhrzeiten.
export class DurationEditor extends Property<number> implements IDurationEditor {
    // Beginn (als Uhrzeit).
    readonly startTime: TimeProperty

    // Ende (als Uhrzeit).
    readonly endTime: TimeProperty

    // Erstellt ein neues Präsentationsmodell.
    constructor(data: any, propTime: string, propDuration: string, text: string, onChange: () => void) {
        super(data, propDuration, text, onChange)

        // Die Startzeit ändert direkt den entsprechenden Wert in den Daten der Aufzeichnung.
        this.startTime = new TimeProperty(data, propTime, undefined, () => this.onChanged())

        // Aus der aktuellen Startzeit und der aktuellen Dauer das Ende ermitteln.
        const end = new Date(new Date(this.startTime.value ?? 0).getTime() + 60000 * (this.value ?? 0))

        // Die Endzeit wird hier als absolute Zeit verwaltet.
        this.endTime = new TimeProperty({ value: end.toISOString() }, 'value', undefined, () =>
            this.onChanged()
        ).addValidator((t) => this.checkLimit())

        // Initiale Prüfungen ausführen.
        this.startTime.validate()
        this.endTime.validate()
        this.validate()
    }

    // Wird bei jeder Eingabe von Start- oder Endzeit ausgelöst.
    private onChanged(): void {
        // Wir greifen hier direkt auf die Roheingaben zu - ansonsten müssten wir die Uhrzeit aus der tatsächlich verwalteten ISO Zeichenkette mühsam ermitteln.
        const start = DateTimeUtils.parseTime(this.startTime.rawValue)
        const end = DateTimeUtils.parseTime(this.endTime.rawValue)

        if (start !== null && end !== null) {
            // Die Dauer ist einfach die Differen aus Ende oder Start - liegt das Ende vor dem Start wird einfach nur von einem Tagessprung ausgegangen.
            let duration = (end - start) / 60000
            if (duration <= 0) duration += 24 * 60

            // Das ist nun erst einmal der aktuelle Wert.
            this.value = duration
        }

        // Auf jeden Fall die Änderung melden.
        this.refresh()
    }

    // Prüft die Dauer gegen die absolute Grenze.
    private checkLimit(): string | undefined {
        if ((this.value ?? 0) >= 24 * 60) return 'Die Aufzeichnungsdauer muss kleiner als ein Tag sein.'
    }

    // Prüft, ob die Eingabe der Dauer gültig ist.
    isValid(): boolean {
        // Wir haben keine eigene Fehlermeldung und mißbrauchen die Endzeit dafür.
        if (this.startTime.message) return false
        if (this.endTime.message) return false

        return true
    }
}
