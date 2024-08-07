﻿import { DateTimeUtils } from '../../../lib/dateTimeUtils'
import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IPlanExceptionContract } from '../../../web/IPlanExceptionContract'

// Schnittstelle zur Anzeige und zum Entfernen einer Ausnahmeregel.
export interface IScheduleException {
    // Wird zurückgesetzt um die Ausnahmeregel beim Speichern zu entfernen.
    readonly isActive: IFlag

    // Der Tag für den die Ausnahmeregel gilt.
    readonly dayDisplay: string

    // Die Startverschiebung (in Minuten).
    readonly startShift: number

    // Die Laufzeitänderung (in Minuten).
    readonly timeDelta: number
}

// Präsentationsmodell zur Anzeige und zum Entfernen einer einzelnen Ausnahmeregel.
export class ScheduleException implements IScheduleException {
    // Erstellt ein neues Präsentationsmodell.
    constructor(
        public readonly model: IPlanExceptionContract,
        onChange: () => void
    ) {
        this.isActive = new BooleanProperty({ value: true }, 'value', undefined, onChange)

        // Initiale Prüfungen ausführen.
        this.isActive.validate()
    }

    // Wird zurückgesetzt um die Ausnahmeregel beim Speichern zu entfernen.
    readonly isActive

    // Der Tag für den die Ausnahmeregel gilt.
    get dayDisplay(): string {
        return DateTimeUtils.formatStartDate(new Date(parseInt(this.model.exceptionDateUnix, 10)))
    }

    // Die Startverschiebung (in Minuten).
    get startShift(): number {
        return this.model.exceptionStartShift
    }

    // Die Laufzeitänderung (in Minuten).
    get timeDelta(): number {
        return this.model.exceptionDurationDelta
    }
}
