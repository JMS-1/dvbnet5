﻿import { IProperty, Property } from '../edit'

// Beschreibt eine Eigenschaft mit einer Zahl.
export interface INumber extends IProperty<number> {
    // Falls die Eingabe über einen Text erfolgt wird diese Eigenschaft zur Pflege verwendet.
    rawValue: string
}

// Verwaltet eine Eigenschaft mit einer (ganzen) Zahl (mit maximal 6 Ziffern - ohne führende Nullen).
export class NumberProperty<TDataType> extends Property<TDataType, number> implements INumber {
    // Erlaubt sind beliebige Sequenzen von Nullen oder maximal 6 Ziffern - mit einer beliebigen Anzahl von führenden Nullen.
    private static readonly _positiveNumber = /^(0+|(0*[1-9][0-9]{0,5}))$/

    // Legt eine neue Verwaltung an.
    constructor(data: TDataType, prop: keyof TDataType, name?: string, onChange?: () => void) {
        super(data, prop, name, onChange)

        // Spezielle Prüfung auf Fehleingaben.
        this.addValidator((num) => num.isValidNumber)
    }

    // Prüft, ob eine gültige Zahl vorliegt.
    private get isValidNumber(): string | undefined {
        if (this._rawInput !== undefined) return 'Ungültige Zahl'
    }

    // Entählt die aktuelle Fehleingabe.
    private _rawInput?: string

    // Meldet die aktuelle Eingabe - entweder eine Fehleingabe oder der Wert als Zeichenkette.
    get rawValue(): string {
        if (this._rawInput === undefined) return this.value == null ? '' : this.value.toString()

        return this._rawInput
    }

    // Übermittelt eine neue Eingabe.
    set rawValue(newValue: string) {
        // Leerzeichen ignorieren wir für die Prüfung.
        const test = (newValue || '').trim()

        // Keine Eingabe und ein Wert ist optional.
        if (test.length < 1) {
            this._rawInput = undefined
            this.value = null
        }

        // Eine (nach unseren Regeln) gültige Zahl.
        else if (NumberProperty._positiveNumber.test(test)) {
            this._rawInput = undefined
            this.value = parseInt(test)
        }

        // Die Eingabe ist grundsätzlich unzulässig.
        else {
            this._rawInput = newValue
        }

        // Anzeige aktualisieren.
        this.refresh()
    }

    // Ergänzt eine Prüfung auf einen vorhandenen Wert.
    addRequiredValidator(message = 'Es muss eine Zahl eingegeben werden.'): this {
        return this.addValidator((p) => {
            if (this.value === null) return message
        })
    }

    // Eine Prüfung auf eine Untergrenze.
    addMinValidator(min: number, message?: string): this {
        return this.addValidator((p) => {
            if (this._rawInput === undefined)
                if (this.value !== null) if (this.value < min) return message || `Die Zahl muss mindestens ${min} sein`
        })
    }

    // Eine Prüfung auf eine Obergrenze.
    addMaxValidator(max: number, message?: string): this {
        return this.addValidator((p) => {
            if (this._rawInput === undefined)
                if (this.value !== null) if (this.value > max) return message || `Die Zahl darf höchstens ${max} sein`
        })
    }
}
