﻿import { IProperty, Property } from './edit'
import { IToggableUiValue, IUiValue } from './list'

import { Command } from '../command/command'

// Beschreibt die Auswahl einer Liste von Werten aus einer Liste von erlaubten Werten.
export interface IMultiValueFromList<TValueType> extends IProperty<TValueType[]> {
    readonly allowedValues: IToggableUiValue<TValueType>[]
}

// Beschreibt einen auswählbaren Wert.
class SelectableValue<TValueType> implements IToggableUiValue<TValueType> {
    // Erstellt eine neue Beschreibung für einen Wert.
    constructor(
        value: IUiValue<TValueType>,
        private readonly _onChange: () => void
    ) {
        this.selected = value.isSelected || false
        this.display = value.display
        this.value = value.value
    }

    // Befehl zur Auswahl.
    private _toggle: Command<void>

    get toggle(): Command<void> {
        // Einmalig anlegen.
        if (!this._toggle)
            this._toggle = new Command<void>(() => {
                this.isSelected = !this.isSelected
            }, this.display)

        return this._toggle
    }

    // Meldet, ob der Wert ausgewählt wurde - beim direkten Zugriff werden keine Benachrichtigungen ausgelöst.
    selected: boolean

    get isSelected(): boolean {
        return this.selected
    }

    // Legt fest, ob der Wert ausgewählt wurde.
    set isSelected(newValue: boolean) {
        // Es gibt keine Änderung.
        if (newValue === this.selected) return

        // Änderung merken.
        this.selected = newValue

        // Anzeige aktualisieren.
        this._onChange()
    }

    // Der dem Anwender präsentierte Wert.
    readonly display: string

    // Der tatsächlich zu speichernde Wert.
    readonly value: TValueType
}

// Präsentationsmodell für eine Mehrfachauswahl von Werten aus einer Liste erlaubter Werte.
export class MultiListProperty<TDataType, TValueType = string>
    extends Property<TDataType, TValueType[]>
    implements IMultiValueFromList<TValueType>
{
    // Legt eine neue Liste an.
    constructor(
        data: TDataType,
        prop: keyof TDataType,
        name?: string,
        onChange?: () => void,
        allowedValues: IToggableUiValue<TValueType>[] = []
    ) {
        super(data, prop, name, onChange)

        // Originalwerte kapseln.
        this.allowedValues = allowedValues
    }

    // Die Liste der erlaubten Werte.
    private _allowedValues: SelectableValue<TValueType>[]

    get allowedValues(): IToggableUiValue<TValueType>[] {
        return this._allowedValues
    }

    // Ändert die Liste der erlaubten Werte.
    set allowedValues(newValues: IToggableUiValue<TValueType>[]) {
        this._allowedValues = newValues.map(
            (v) => new SelectableValue<TValueType>(v, () => this.setValueFromSelection())
        )

        // Auswahlliste auswerten und als Wertefeld übernehmen.
        this.setValueFromSelection()

        // Anzeige aktualisieren.
        this.refresh()
    }

    // Aktuelle Auswahl in die Werteliste übernehmen.
    private setValueFromSelection(): void {
        this.value = this.allowedValues.filter((v) => v.isSelected).map((v) => v.value)
    }

    // Anzeige aktualisieren.
    refresh(): void {
        // Tatsächlich ausgewählte Werte in der Auswahlliste markieren.
        const values = this.value || []

        this._allowedValues.forEach((av) => (av.selected = values.some((v) => v === av.value)))

        // Anzeige aktualisieren.
        super.refresh()
    }
}
