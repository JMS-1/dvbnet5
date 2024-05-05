import { IProperty, Property } from '../edit'

// Beschreibt eine Eigenschaft der Art Zeichenkette mit Prüfergebnissen.
export interface IString extends IProperty<string> {}

// Verwaltet eine Eigenschaft der Art Zeichenkette.
export class StringProperty<TDataType> extends Property<TDataType, string> implements IString {
    // Legt eine neue Verwaltung an.
    constructor(data: TDataType, prop: keyof TDataType, name?: string, onChange?: () => void) {
        super(data, prop, name, onChange)
    }

    // Ergänzt eine Prüfung auf eine leere Zeichenkette.
    addRequiredValidator(message = 'Es muss ein Wert angegeben werden.'): this {
        return this.addValidator((p) => {
            // Der Wert darf nicht die leere Zeichenkette sein - und auch nicht nur aus Leerzeichen et al bestehen.
            const value = (p.value || '').trim()

            if (value.length < 1) return message
        })
    }

    // Ergänzt eine Prüfung auf ein festes Muster.
    addPatternValidator(matcher: RegExp, message: string): this {
        return this.addValidator((p) => {
            if (!matcher.test(p.value ?? '')) return message
        })
    }
}
