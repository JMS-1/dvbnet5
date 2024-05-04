import { IFlag, Flag } from '../../../lib/edit/boolean/flag'
import { INumber } from '../../../lib/edit/number/number'
import { Number } from '../../../lib/edit/number/number'
import { IProfileContract } from '../../../web/IProfileContract'

// Schnittstelle zur Pflege eines einzelnen Geräteprofils.
export interface IDevice {
    // Der Name des Gerätes.
    readonly name: string

    // Gesetzt, wenn das Gerät für den VCR.NET Recording Service verwendet werden soll.
    readonly active: IFlag

    // Planungspriorität des Gerätes.
    readonly priority: INumber

    // Anzahl der gleichzeitg entschlüsselbaren Quellen.
    readonly decryption: INumber

    // Anzahl der gleichzeitig empfangbaren Quellen.
    readonly sources: INumber
}

// Das Präsentationsmodell zur Pflege eines Gerätes.
export class Device implements IDevice {
    // Der Name des Gerätes.
    readonly name: string

    // Gesetzt, wenn das Gerät für den VCR.NET Recording Service verwendet werden soll.
    readonly active: Flag

    // Planungspriorität des Gerätes.
    readonly priority: Number

    // Anzahl der gleichzeitg entschlüsselbaren Quellen.
    readonly decryption: Number

    // Anzahl der gleichzeitig empfangbaren Quellen.
    readonly sources: Number

    // Erstellt ein neues Präsentationsmodell.
    constructor(
        profile: IProfileContract,
        onChange: () => void,
        private readonly _defaultDevice: () => string
    ) {
        this.name = profile.name
        this.priority = new Number(profile, 'priority', undefined, onChange)
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(100)
        this.decryption = new Number(profile, 'ciLimit', undefined, onChange)
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(16)
        this.sources = new Number(profile, 'sourceLimit', undefined, onChange)
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(32)
        this.active = new Flag(profile, 'active', undefined, onChange).addValidator((flag) =>
            this.validateDefaultDevice(flag)
        )
    }

    // Wird ein Gerät nicht verwendet, so darf es auch nicht als Vorgabegerät eingetragen sein.
    private validateDefaultDevice(active: Flag): string | undefined {
        if (!active.value)
            if (this.name === this._defaultDevice())
                return `Das bevorzugte Geräteprofil muss auch für Aufzeichnungen verwendet werden.`
    }

    // Prüft, ob die Konfiguration des Gerätes gültig ist.
    get isValid(): boolean {
        return !this.active.message && !this.priority.message && !this.decryption.message && !this.sources.message
    }
}
