import { IDisplay } from '../localizable'
import { IConnectable, IView } from '../site'

// Bietet den Wert einer Eigenschaft zur Pflege in der Oberfläche an.
export interface IProperty<TValueType> extends IDisplay, IConnectable {
    // Meldet den aktuellen Wert oder verändert diesen - gemeldet wird immer der ursprüngliche Wert.
    value: TValueType | null

    // Gesetzt, wenn der Wert der Eigenschaft nicht verändert werden darf.
    readonly isReadonly?: boolean

    // Die zuletzt ermittelten Prüfinformationen. Eine leere Zeichenkette bedeutet, dass die Eigenschaft einen gültigen Wert besitzt.
    readonly message?: string
}

// Basisklasse zur Pflege des Wertes einer einzelnen Eigenschaft.
export abstract class Property<TDataType, TValueType, TProp = keyof TDataType> implements IProperty<TValueType> {
    // Initialisiert die Verwaltung des Wertes einer einzelnen Eigenschaft (_prop) im Modell (_data).
    protected constructor(
        private _data: TDataType = {} as TDataType,
        private readonly _prop: TProp,
        public readonly text: string | null = null,
        private readonly _onChange?: () => void,
        private readonly _testReadOnly?: () => boolean
    ) {}

    // Alle Prüfalgorithmen.
    private _validators: ((property: this) => string | undefined)[] = []

    // Vermerkt einen Prüfalgorithmus.
    addValidator(validator: (property: this) => string | undefined): this {
        this._validators.push(validator)

        return this
    }

    // Das zugehörige Oberflächenelement.
    private _site: IView

    // Benachrichtigt die Oberfläche zur Aktualisierung der Anzeige.
    protected refresh(): void {
        // Prüfergebnis aktualisieren.
        this.validate()

        // Präsentationsmodell über Änderungen informieren.
        if (this._onChange) this._onChange()

        // Unmittelbar verbundenes Oberflächenelement aktualisieren.
        this.refreshUi()
    }

    // Fordert die Anzeige zur Aktualisierung auf.
    refreshUi(): void {
        if (this._site) this._site.refreshUi()
    }

    // Meldet die Oberfläche an.
    set view(site: IView) {
        this._site = site

        // Interne Benachrichtigung auslösen.
        if (this._site) this.onSiteChanged()
    }

    // Ermittelt das aktuell zugeordnete Oberflächenelement.
    get view(): IView {
        return this._site
    }

    // Wird ausgelöst, wenn eine Anmeldung der Oberfläche erfolgt ist.
    protected onSiteChanged(): void {
        //
    }

    // Meldet den aktuellen Wert.
    get value(): TValueType | null {
        return this.data[this._prop] as TValueType
    }

    // Verändert den aktuellen Wert.
    set value(newValue: TValueType | null) {
        // Prüfen, ob der aktuelle Wert durch einen anderen ersetzt werden soll.
        if (newValue === this.value) return

        // Neuen Wert ins Modell übertragen.
        this.data[this._prop] = newValue

        // Modelländerung melden und Oberfläche aktualisieren.
        this.refresh()
    }

    // Meldet, ob der Wert der Eigenschaft nicht verändert werden darf.
    get isReadonly(): boolean | undefined {
        return this._testReadOnly && this._testReadOnly()
    }

    // Verwaltung des Prüfergebnisses - die Basisimplementierung meldet die Eigenschaft immer als gültig (leere Zeichenkette).
    private _message? = ''

    get message(): string | undefined {
        return this._message
    }

    validate(): void {
        // Alle Prüfalgorithmen durchgehen.
        for (let i = 0; i < this._validators.length; i++) if ((this._message = this._validators[i](this))) return

        // Kein Fehler.
        this._message = ''
    }

    // Meldet das aktuell zugeordnete Modell.
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    get data(): any {
        return this._data
    }

    // Ändert das aktuell zugeordnete Modell.
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    set data(newValue: any) {
        this._data = newValue

        // Dadurch kann sich natürlich der aktuelle Wert verändert haben.
        this.refresh()
    }
}
