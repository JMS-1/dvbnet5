﻿import { IConnectable } from '../../site'
import { Property } from '../edit'

// Steuert die Pflege einer Zahl über einen Schieberegler.
export interface INumberWithSlider extends IConnectable {
    // Meldet oder ändert die relative (0..1) Position des Reglers.
    position: number

    // Aktiviert die Positionsveränderung.
    isDragging: boolean

    // Erlaubt eine Feineinstellung des zugrundeliegenden Wertes.
    delta(delta: number): void

    // Meldet den aktuellen Wert.
    readonly value: number | null
}

// Steuerung für einen Schieberegeler für die Auswahl eines Wertes.
export class NumberWithSlider<TDataType> extends Property<TDataType, number> implements INumberWithSlider {
    // Die aktuelle relative (0..1) Position des Reglers.
    private _position = 0

    // Gesetzt, wenn Bewegungsbefehle umgesetzt werden sollen.
    private _moving = false

    // Der Anfangswert.
    private readonly _initial: number | null

    // Erstellt eine neue Steuerung.
    constructor(
        data: TDataType,
        prop: keyof TDataType,
        onChange: () => void,
        private _min: number,
        private _max: number
    ) {
        super(data, prop, null, onChange)

        // Anfangswert merken.
        this._initial = this.value

        // Synchronisiert den Regler mit dem aktuellen Wert.
        this.sync()
    }

    // Bei der Anzeige werden immer alle Einstellungen auf den Grundwert zurückgesetzt.
    protected onSiteChanged(): void {
        super.onSiteChanged()

        // Zurück auf den Anfang.
        this.reset()

        this._moving = false
    }

    // Grundeinstellungen vornehmen.
    reset(): void {
        this.sync(this._initial)
    }

    // Synchronisiert den Regeler mit dem aktuellen Wert.
    sync(newValue?: number | null): void {
        // Auf Wunsch kann direkt ein neuer Wert eingestellt werden.
        if (newValue !== undefined) this.value = newValue

        // In eine relative Position umsetzen.
        this.position = ((this.value ?? 0) - this._min) / (this._max - this._min)
    }

    // Nimmt eine Feineinstellung vor.
    delta(delta: number): void {
        // Relative Position setzen, der Wert gleicht sich dann automatisch mit an.
        this.position = ((this.value ?? 0) + delta - this._min) / (this._max - this._min)
    }

    // Meldet oder ändert die aktuelle Position des Schiebereglers.
    get position(): number {
        return this._position
    }

    set position(newPosition: number) {
        // Relative Grenzen beachten.
        if (newPosition < 0) newPosition = 0
        else if (newPosition > 1) newPosition = 1

        if (newPosition !== this._position) {
            // Schiebregler anpassen.
            this._position = newPosition

            // Tatsächlichen Wert direkt mit ändern.
            this.value = Math.round(this._min + newPosition * (this._max - this._min))

            // Anzeige des Schiebereglers verändern.
            this.refresh()
        }
    }

    // Meldet oder legt fest, ob sich die Position aktuell verändert.
    get isDragging(): boolean {
        return this._moving
    }

    set isDragging(nowDragging: boolean) {
        // Das machen wir bereits schon so.
        if (nowDragging === this._moving) return

        // Änderung vermerken.
        this._moving = nowDragging

        // Eventuell die Oberfläche anpassen (Feedback).
        this.refresh()
    }
}
