﻿import { Command, ICommand } from '../../command/command'
import { IView } from '../../site'
import { IProperty, Property } from '../edit'
import { NumberProperty } from '../number/number'

// Schnittstelle zur Pflege einer Eigenschaft mit einem Wahrheitswert.
export interface IFlag extends IProperty<boolean> {}

// Schnittstelle zur Pflege einer Eigenschaft mit einem Wahrheitswert.
export interface IToggableFlag extends IFlag {
    // Befehl zum Umschalten des Wahrheitswertes.
    readonly toggle: ICommand
}

// Verwaltet den Wahrheitswert in einer Eigenschaft - hier können wir uns vollständig auf die Implementierung der Basisklasse verlassen.
export class BooleanProperty<TDataType> extends Property<TDataType, boolean> implements IToggableFlag {
    // Befehl zum Umschalten des Wahrheitswertes.
    private _toggle: Command<void>

    get toggle(): Command<void> {
        // Einmalig anlegen.
        if (!this._toggle)
            this._toggle = new Command<void>(
                () => {
                    this.value = !this.value
                },
                this.text,
                () => !this.isReadonly
            )

        return this._toggle
    }

    // Legt eine neue Verwaltung an.
    constructor(
        data: TDataType,
        prop: keyof TDataType,
        name?: string,
        onChange?: () => void,
        testReadOnly?: () => boolean
    ) {
        super(data, prop, name, onChange, testReadOnly)
    }
}

// Verwaltet ein Bitfeld von Wahrheitswerten in einer Eigenschaft mit einer Zahl als Wert.
export class FlagSet<TDataType> implements IFlag {
    // Prüfungen werden hierbei nicht individuell unterstützt.
    readonly message = ''

    // Erstelle eine Verwaltungsinstanz auf Basis der Verwaltung der elementaren Zahl.
    constructor(
        private _mask: number,
        private readonly _flags: NumberProperty<TDataType>,
        public text: string
    ) {}

    // Das zugehörige Oberflächenelement.
    view: IView

    // Meldet den aktuellen Wert oder verändert diesen.
    get value(): boolean {
        return ((this._flags.value ?? 0) & this._mask) !== 0
    }

    set value(newValue: boolean) {
        // Änderung bitweise an die eigentliche Eigenschaft übertragen.
        const flags = newValue ? (this._flags.value ?? 0) | this._mask : (this._flags.value ?? 0) & ~this._mask

        // Keine Änderung.
        if (flags === this._flags.value) return

        // Änderung durchführen.
        this._flags.value = flags

        // Oberfläche aktualisieren.
        if (this.view) this.view.refreshUi()
    }

    // Gesetzt, wenn der Wert der Eigenschaft nicht verändert werden darf.
    get isReadonly(): boolean | undefined {
        return this._flags.isReadonly
    }
}
