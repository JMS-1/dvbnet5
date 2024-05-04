import { ICommand, Command } from '../../../lib/command/command'
import { DateTimeUtils } from '../../../lib/dateTimeUtils'
import { INumberWithSlider, NumberWithSlider } from '../../../lib/edit/number/slider'
import { IConnectable, IView } from '../../../lib/site'
import { IPlanExceptionContract, updateException } from '../../../web/IPlanExceptionContract'

// Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
export interface IPlanException extends IConnectable {
    // Der Regler zur Einstellung der Startzeitverschiebung.
    readonly startSlider: INumberWithSlider

    // Der Regler zur Einstellung der Laufzeitveränderung.
    readonly durationSlider: INumberWithSlider

    // Die Darstellung für den Zustand der Ausnahme.
    readonly exceptionMode: string

    // Meldet den Startzeitpunkt als Text.
    readonly currentStart: string

    // Meldet den Endzeitpunkt als Text.
    readonly currentEnd: string

    // Meldet die aktuelle Dauer.
    readonly currentDuration: number

    // Verwendet die ursprüngliche Aufzeichnungsdaten.
    readonly originalTime: ICommand

    // Deaktiviert die Aufzeichnung vollständig.
    readonly skip: ICommand

    // Aktualisiert die Aufzeichnung.
    readonly update: ICommand
}

// Erweiterte Schnittstelle zur Pflege einer einzelnen Ausnahmeregel.
export class PlanException implements IPlanException {
    // Erstellt ein neies Präsentationsmodell.
    constructor(
        private model: IPlanExceptionContract,
        private _entryId: string,
        private _reload: () => void
    ) {
        this._originalStart = new Date(model.originalStart as string)
        this.startSlider = new NumberWithSlider(model, 'startShift', () => this.refreshUi(), -480, +480)
        this.durationSlider = new NumberWithSlider(
            model,
            'timeDelta',
            () => this.refreshUi(),
            -model.originalDuration,
            +480
        )
    }

    // Der ursprüngliche Startzeitpunkt
    private _originalStart: Date

    // Der Regler zur Einstellung der Startzeitverschiebung.
    readonly startSlider: NumberWithSlider

    // Der Regler zur Einstellung der Laufzeitveränderung.
    readonly durationSlider: NumberWithSlider

    // Befehl zum Zurücksetzen des Aufzeichnungsbereichs of die originalen Werte.
    readonly originalTime = new Command(() => this.setToOriginal(), 'Ursprüngliche Planung')

    // Befehl zum Deaktivieren der Aufzeichnung.
    readonly skip = new Command(() => this.setToDisable(), 'Nicht aufzeichnen')

    // Befehl zum Abspeichern der Änderungen.
    readonly update = new Command(() => this.save(), 'Einstellungen übernehmen')

    // Die Darstellung für den Zustand der Ausnahme.
    get exceptionMode(): string {
        return this.model.startShift !== 0 || this.model.timeDelta !== 0 ? 'exceptOn' : 'exceptOff'
    }

    // Meldet den Startzeitpunkt als Text.
    private start(): Date {
        return new Date(this._originalStart.getTime() + 60 * this.model.startShift * 1000)
    }

    get currentStart(): string {
        return DateTimeUtils.formatStartTime(this.start())
    }

    // Meldet den Endzeitpunkt als Text.
    private end(): Date {
        return new Date(this.start().getTime() + 60 * (this.model.originalDuration + this.model.timeDelta) * 1000)
    }

    get currentEnd(): string {
        return DateTimeUtils.formatEndTime(this.end())
    }

    // Meldet die aktuelle Dauer.
    get currentDuration(): number {
        return this.model.originalDuration + this.model.timeDelta
    }

    // Setzt alles auf den Eingangszustand zurück.
    reset(): void {
        this.skip.reset()
        this.update.reset()
        this.originalTime.reset()

        this.startSlider.reset()
        this.durationSlider.reset()
    }

    // Verwendet die ursprüngliche Aufzeichnungsdaten.
    private setToOriginal(): void {
        this.startSlider.sync(0)
        this.durationSlider.sync(0)
    }

    // Deaktiviert die Aufzeichnung vollständig.
    private setToDisable(): void {
        this.startSlider.sync(0)
        this.durationSlider.sync(-this.model.originalDuration)
    }

    // Aktualisiert die Ausnahmeregel.
    private save(): void {
        // Änderung anfordern und Ergebnis asynchron bearbeiten.
        updateException(this._entryId, this.model.referenceDay, this.model.startShift, this.model.timeDelta).then(
            this._reload
        )
    }

    // Beachrichtigungen einrichten.
    view: IView

    // Fordert die Oberfläche zur Aktualisierung auf.
    refreshUi(): void {
        if (this.view) this.view.refreshUi()
    }
}
