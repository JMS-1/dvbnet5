import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { INumber, NumberProperty } from '../../../lib/edit/number/number'
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
    readonly active

    // Planungspriorität des Gerätes.
    readonly priority

    // Anzahl der gleichzeitg entschlüsselbaren Quellen.
    readonly decryption

    // Anzahl der gleichzeitig empfangbaren Quellen.
    readonly sources

    // Erstellt ein neues Präsentationsmodell.
    constructor(
        profile: IProfileContract,
        onChange: () => void,
        private readonly _defaultDevice: () => string
    ) {
        this.name = profile.name
        this.priority = new NumberProperty(profile, 'priority', undefined, onChange)
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(100)
        this.decryption = new NumberProperty(profile, 'decryptionLimit', undefined, onChange)
            .addRequiredValidator()
            .addMinValidator(0)
            .addMaxValidator(16)
        this.sources = new NumberProperty(profile, 'sourceLimit', undefined, onChange)
            .addRequiredValidator()
            .addMinValidator(1)
            .addMaxValidator(32)
        this.active = new BooleanProperty(profile, 'usedForRecording', undefined, onChange).addValidator((flag) =>
            this.validateDefaultDevice(flag)
        )
    }

    // Wird ein Gerät nicht verwendet, so darf es auch nicht als Vorgabegerät eingetragen sein.
    private validateDefaultDevice(active: BooleanProperty<IProfileContract>): string | undefined {
        if (!active.value)
            if (this.name === this._defaultDevice())
                return 'Das bevorzugte Geräteprofil muss auch für Aufzeichnungen verwendet werden.'
    }

    // Prüft, ob die Konfiguration des Gerätes gültig ist.
    get isValid(): boolean {
        return !this.active.message && !this.priority.message && !this.decryption.message && !this.sources.message
    }
}
