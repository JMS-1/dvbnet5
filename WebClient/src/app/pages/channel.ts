import { IProperty, Property } from '../../lib/edit/edit'
import { IValueFromList, SingleListProperty, uiValue } from '../../lib/edit/list'
import { IConnectable } from '../../lib/site'
import { IUserProfileContract } from '../../web/IUserProfileContract'
import { SourceEntry } from '../../web/ProfileSourcesCache'

// Die Einschränkung auf die Verschlüsselung.
export enum encryptionFilter {
    // Keine Einschränkung.
    All,

    // Nur verschlüsselte Sender.
    PayTv,

    // Nur unverschlüsselte Sender.
    FreeTv,
}

// Die Arten von zu berücksichtigenden Quellen.
export enum typeFilter {
    // Alle Quellen.
    All,

    // Nur Radiosender.
    Radio,

    // Nur Fernsehsender.
    Tv,
}

// Schnitstelle zur Pflege der Senderauswahl.
export interface IChannelSelector extends IProperty<string>, IConnectable {
    // Die Vorauswahl der Quellen vor allem nach dem ersten Zeichen des Namens.
    readonly section: IValueFromList<string>

    // Die Vorauswahl der Quellen über die Art (Fernsehen oder Radio).
    readonly type: IValueFromList<typeFilter>

    // Die Vorauswahl der Quellen über die Verschlüsselung.
    readonly encryption: IValueFromList<encryptionFilter>

    // Die komplette Liste aller verfügbaren Quellen.
    readonly sourceName: IValueFromList<string>

    // Gesetzt, wenn die zusätzlichen Filter angezeigt werden sollen.
    readonly showFilter: boolean
}

// Stellt die Logik zur Auswahl eines Senders zur Verfügung.
export class ChannelEditor extends Property<string> implements IChannelSelector {
    // Die Auswahl der Verschlüsselung.
    private static readonly _encryptions = [
        uiValue(encryptionFilter.All, 'Alle Quellen'),
        uiValue(encryptionFilter.PayTv, 'Nur verschlüsselte Quellen'),
        uiValue(encryptionFilter.FreeTv, 'Nur unverschlüsselte Quellen'),
    ]

    // Prüft ob eine Quelle der aktuellen Einschränkung der Verschlüsselung entspricht.
    private applyEncryptionFilter(source: SourceEntry): boolean {
        // Wenn wir nur die bevorzugten Sender anzeigen gibt es keine Einschränkung.
        if (!this.showFilter) return true

        switch (this.encryption.value) {
            case encryptionFilter.All:
                return true
            case encryptionFilter.PayTv:
                return source.isEncrypted
            case encryptionFilter.FreeTv:
                return !source.isEncrypted
            default:
                return false
        }
    }

    // Alle Auswahlmöglichkeiten der Verschlüsselung.
    readonly encryption: IValueFromList<encryptionFilter>

    // Die Auswahlmöglichkeiten zur Art der Quelle.
    private static readonly _types = [
        uiValue(typeFilter.All, 'Alle Quellen'),
        uiValue(typeFilter.Radio, 'Nur Radio'),
        uiValue(typeFilter.Tv, 'Nur Fernsehen'),
    ]

    // Prüft, ob eine Quelle der aktuell ausgewählten Art entspricht.
    private applyTypeFilter(source: SourceEntry): boolean {
        // Wenn wir nur die bevorzugten Sender anzeigen gibt es keine Einschränkung.
        if (!this.showFilter) return true

        switch (this.type.value) {
            case typeFilter.All:
                return true
            case typeFilter.Radio:
                return !source.isTelevision
            case typeFilter.Tv:
                return source.isTelevision
            default:
                return false
        }
    }

    // Alle Auswahlmöglichkeiten für die Art der Quelle.
    readonly type: IValueFromList<typeFilter>

    // Alle möglichen Einschränkungen auf die Namen der Quellen.
    private static readonly _sections = [
        uiValue('(Zuletzt verwendet)'),
        uiValue('A B C'),
        uiValue('D E F'),
        uiValue('G H I'),
        uiValue('J K L'),
        uiValue('M N O'),
        uiValue('P Q R'),
        uiValue('S T U'),
        uiValue('V W X'),
        uiValue('Y Z'),
        uiValue('0 1 2 3 4 5 6 7 8 9'),
        uiValue('(Andere)'),
        uiValue('(Alle Quellen)'),
    ]

    // Prüft, ob der Name einer Quelle der aktuellen Auswahl entspricht.
    private applySectionFilter(source: SourceEntry): boolean {
        const first = source.firstNameCharacter

        switch (this.section.valueIndex) {
            case 0:
                return this._favorites[source.name]
            case 1:
                return first >= 'A' && first <= 'C'
            case 2:
                return first >= 'D' && first <= 'F'
            case 3:
                return first >= 'G' && first <= 'I'
            case 4:
                return first >= 'J' && first <= 'L'
            case 5:
                return first >= 'M' && first <= 'O'
            case 6:
                return first >= 'P' && first <= 'R'
            case 7:
                return first >= 'S' && first <= 'U'
            case 8:
                return first >= 'V' && first <= 'X'
            case 9:
                return first >= 'Y' && first <= 'Z'
            case 10:
                return first >= '0' && first <= '9'
            case 11:
                return !((first >= 'A' && first <= 'Z') || (first >= '0' && first <= '9'))
            case 12:
                return true
            default:
                return false
        }
    }

    // Alle Auswahlmöglichkeiten zum Namen der Quellen.
    readonly section = new SingleListProperty(
        { value: 0 },
        'value',
        undefined,
        () => this.refreshFilter(),
        ChannelEditor._sections
    )

    // Alle aktuell bezüglich aller Einschränkungen relevanten Quellen.
    readonly sourceName = new SingleListProperty<string>(this, 'value').addValidator((n) => {
        const source = n.value

        // Wenn eine Quelle ausgewählt wurde, dann muss sie auch von dem aktuellen Gerät empfangen werden können.
        if ((source || '').trim().length > 0)
            if (!this.allSources.some((s) => s.name === source))
                return 'Die Quelle wird von dem ausgewählten Gerät nicht empfangen.'
    })

    // Die bevorzugten Quellen des Anwenders - hier in einem Dictionary zur Prüfung optimiert.
    private _favorites: { [source: string]: boolean } = {}

    // Gesetzt, wenn die zusätzlichen Filter angezeigt werden sollen.
    get showFilter(): boolean {
        return this.section.valueIndex !== 0
    }

    // Erstellt eine neue Logik zur Senderauswahl.
    constructor(
        profile: IUserProfileContract,
        data: unknown,
        prop: string,
        favoriteSources: string[],
        onChange: () => void
    ) {
        super(data, prop, 'Quelle', onChange)

        // Voreinstellungen vorbereiten.
        this.encryption = new SingleListProperty(
            { value: ChannelEditor.lookupEncryption(profile) },
            'value',
            undefined,
            () => this.refreshFilter(),
            ChannelEditor._encryptions
        )
        this.type = new SingleListProperty(
            { value: ChannelEditor.lookupType(profile) },
            'value',
            undefined,
            () => this.refreshFilter(),
            ChannelEditor._types
        )

        // Initialen Filter vorbereiten.
        if (favoriteSources.length < 1) this.section.valueIndex = this.section.allowedValues.length - 1
        else {
            // Übernimmt die lineare Liste aller bevorzugten Sender zur schnelleren Auswahl in ein Dictionary.
            favoriteSources.forEach((s) => (this._favorites[s] = true))

            this.section.valueIndex = 0
        }
    }

    private static lookupType(profile: IUserProfileContract): typeFilter {
        switch (profile && profile.typeFilter) {
            case 'R':
                return typeFilter.Radio
            case 'T':
                return typeFilter.Tv
            default:
                return typeFilter.All
        }
    }

    private static lookupEncryption(profile: IUserProfileContract): encryptionFilter {
        switch (profile && profile.encryptionFilter) {
            case 'F':
                return encryptionFilter.FreeTv
            case 'P':
                return encryptionFilter.PayTv
            default:
                return encryptionFilter.All
        }
    }

    // Ermittelt die Liste der relevanten Quellen neu.
    private refreshFilter(): void {
        // Alle Quellen bezüglich der aktiven Filter untersuchen.
        const sourceNames = this.allSources
            .filter((s) => {
                if (!this.applyEncryptionFilter(s)) return false
                if (!this.applyTypeFilter(s)) return false
                if (!this.applySectionFilter(s)) return false

                return true
            })
            .map((s) => s.name)

        // Aktuelle Quelle zusätzliche in die Liste einmischen, so dass immer eine korrekte Auswahl existiert.
        const source = this.value || ''

        if (source.trim().length > 0)
            if (sourceNames.indexOf(source) < 0) {
                const cmp = source.toLocaleUpperCase()
                let insertAt = -1

                for (let i = 0; i < sourceNames.length; i++)
                    if (cmp.localeCompare(sourceNames[i].toLocaleUpperCase()) < 0) {
                        insertAt = i

                        break
                    }

                // Bereits gewählte Quelle an der korrekten Position in der Liste eintragen.
                if (insertAt < 0) sourceNames.push(source)
                else sourceNames.splice(insertAt, 0, source)
            }

        // Der erste Eintrag erlaubt es immer auch einfach mal keinen Sender auszuwählen.
        this.sourceName.allowedValues = [uiValue('', '(Keine Quelle)')].concat(sourceNames.map((s) => uiValue(s)))

        // Anzeige aktualisieren.
        this.refresh()
    }

    // Sämtliche bekannten Quellen.
    private _allSources: SourceEntry[] = []

    get allSources(): SourceEntry[] {
        return this._allSources
    }

    set allSources(sources: SourceEntry[]) {
        // Falls wir auf der gleichen Liste arbeiten müssen wir gar nichts machen.
        if (this._allSources === sources) return

        // Die Liste der Quellen wurde verändert.
        this._allSources = sources

        this.refreshFilter()
    }
}
