import { Command, ICommand } from '../../../lib/command/command'
import { IConnectable, IView } from '../../../lib/site'
import { guideEncryption } from '../../../web/guideEncryption'
import { guideSource } from '../../../web/guideSource'
import { countProgramGuide, IGuideFilterContract } from '../../../web/IGuideFilterContract'
import { ISavedGuideQueryContract } from '../../../web/ISavedGuideQueryContract'

// Schnittstelle zur Anzeige und Pflege einer gespeicherten Suche.
export interface IFavorite extends IConnectable {
    // Die Beschreibung der gespeicherten Suche.
    readonly title: string

    // Die Anzahl der Sendungen, die zur Suche passen.
    readonly count?: number | null

    // Entfernt die gespeicherte Suche.
    readonly remove: ICommand

    // Zeit die Sendungen der gespeicherten Suche in der Programmzeitschrift an.
    readonly show: ICommand
}

// Präsentationsmodell  zurn Anzeige und Pflege einer gespeicherten Suche.
export class Favorite implements IFavorite {
    // Synchronisert das Ermitteln der Anzeige der Sendungen - es wird zu jeder Zeit immer nur eine Anfrage an den VCR.NET Recording Service gestellt.
    private static _loader: Promise<void>

    // Beginnt die Synchronisation neu.
    static resetLoader(): void {
        Favorite._loader = new Promise<void>((success) => success(void 0))
    }

    // Legt ein Präsentationsmodell an.
    constructor(
        public readonly model: ISavedGuideQueryContract,
        show: (favorite: Favorite) => void,
        remove: (favorite: Favorite) => Promise<void>,
        private _refresh: () => void
    ) {
        this.remove = new Command<void>(() => remove(this), 'Löschen')
        this.show = new Command<void>(() => show(this), 'Anzeigen')

        // Wir schlagen die Anzahl immer nur ein einziges Mal nach.
        this._cacheKey = JSON.stringify(model)
    }

    // Das aktuell angemeldete Oberflächenelement.
    view: IView

    // Entfernt die gespeicherte Suche.
    readonly remove: Command<void>

    // Zeit die Sendungen der gespeicherten Suche in der Programmzeitschrift an.
    readonly show: Command<void>

    // Die Beschreibung der gespeicherten Suche.
    get title(): string {
        let display = 'Alle '

        // Einige Einschränkungen machen nur Sinn, wenn keine Quelle ausgewählt ist.
        if ((this.model.source || '') === '') {
            // Verschlüsselung.
            if (this.model.encryption === guideEncryption.Free) display += 'unverschlüsselten '
            else if (this.model.encryption === guideEncryption.Encrypted) display += 'verschlüsselten '

            // Art der Quelle.
            if (this.model.sourceType === guideSource.Television) display += 'Fernseh-'
            else if (this.model.sourceType === guideSource.Radio) display += 'Radio-'
        }

        // Gerät.
        display += 'Sendungen, die über das Gerät '
        display += this.model.device

        // Quelle.
        if (this.model.source != null)
            if (this.model.source.length > 0) {
                display += ' von der Quelle '
                display += this.model.source
            }

        // Suchtext.
        display += ' empfangen werden und deren Name '
        if (!this.model.titleOnly) display += 'oder Beschreibung '

        display += ' "'
        display += this.model.text.substr(1)
        display += '" '

        // Art der Suche.
        if (this.model.text[0] == '*') display += 'enthält'
        else display += 'ist'

        return display
    }

    // Vorhaltung von Nachschlageoperationen.
    private static _countCache: { [key: string]: number } = {}

    private readonly _cacheKey: string

    // Die Anzahl der Sendungen, die zur Suche passen.
    private _count?: number | null

    get count(): number | null {
        // Das haben wir schon einmal probiert.
        if (this._count !== undefined) return this._count

        this._count = Favorite._countCache[this._cacheKey]

        // Sicherstellen, dass nur einmal geladen wird.
        if (this._count !== undefined) return this._count

        // Ladevorgang anzeigen.
        this._count = null

        // Suchbedingung in die Protokollnotation wandeln - Auflistungen sind etwas trickreich..
        const filter: IGuideFilterContract = {
            contentPattern: this.model.titleOnly ? '' : this.model.text,
            pageIndex: 0,
            pageSize: 0,
            profileName: this.model.device,
            source: this.model.source,
            sourceEncryption: this.model.encryption,
            sourceType: this.model.sourceType,
            startISO: '',
            titlePattern: this.model.text,
        }

        // Laden anstossen.
        Favorite._loader = Favorite._loader.then(() =>
            countProgramGuide(filter).then((count) => {
                // Wert vermerken.
                this._count = Favorite._countCache[this._cacheKey] = count ?? 0

                // Oberfläche zur Aktualisierung auffordern.
                if (this.view) this.view.refreshUi()

                // Liste aktualisieren.
                if (this._refresh) this._refresh()
            })
        )

        return null
    }
}
