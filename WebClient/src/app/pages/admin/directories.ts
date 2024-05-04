import { ISection, Section } from './section'

import { Command, ICommand } from '../../../lib/command/command'
import { IValueFromList, SelectSingleFromList, uiValue } from '../../../lib/edit/list'
import { IMultiValueFromList, SelectMultipleFromList } from '../../../lib/edit/multiList'
import { IString, String } from '../../../lib/edit/text/text'
import {
    browseDirectories,
    getDirectorySettings,
    IDirectorySettingsContract,
    setDirectorySettings,
    validateDirectory,
} from '../../../web/admin/IDirectorySettingsContract'

// Schnittstelle zur Pflege der erlaubten Aufzeichnungsverzeichnisse.
export interface IAdminDirectoriesPage extends ISection {
    // Die aktuelle Liste der Aufzeichnungsverzeichnisse.
    readonly directories: IMultiValueFromList<string>

    // Eingabe eines Netzwerklaufwerks.
    readonly share: IString

    // Meldet, ob die verzeichnisauswahl angezeigt werden soll.
    readonly showBrowse: boolean

    // Die aktuelle Verzeichnisauswahl.
    readonly browse: IValueFromList<string>

    // Befehl um in der Verzeichnisauswahl zum übergeordneten Verzeichnis zu wechseln.
    readonly parent: ICommand

    // Befehl zur Eintragung des netzwerklaufwerks (nach Prüfung) oder des ausgewählten Verzeichnisses in die Verzeichnisliste.
    readonly add: ICommand

    // Befehl zum Entfernen der ausgewählten Verzeichnisse aus der Verzeichnisliste.
    readonly remove: ICommand

    // Das aktuelle Muster für die Namen von Aufzeichnungsdateien.
    readonly pattern: IString
}

// Präsentationsmodell zur Pflege der Konfiguration der Aufzeichnungsverzeichnisse.
export class DirectoriesSection extends Section implements IAdminDirectoriesPage {
    // Der eindeutige Name des Bereichs.
    static readonly route = 'directories'

    // Die aktuelle Liste der Aufzeichnungsverzeichnisse.
    readonly directories = new SelectMultipleFromList<string>(
        {},
        'value',
        undefined,
        () => this.remove && this.remove.refreshUi()
    )

    // Das aktuelle Muster für die Namen von Aufzeichnungsdateien.
    readonly pattern = new String({}, 'pattern', 'Muster für Dateinamen', () =>
        this.update.refreshUi()
    ).addRequiredValidator()

    // Befehl zum Entfernen der ausgewählten Verzeichnisse aus der Verzeichnisliste.
    readonly remove: Command<unknown> = new Command(
        () => this.removeDirectories(),
        'Verzeichnisse entfernen',
        () => (this.directories.value?.length ?? 0) > 0
    )

    // Fehlermeldung zur letzten Pfüfung des Netzwerkverzeichnisses.
    private _shareValidation?: string

    // Eingabe eines Netzwerklaufwerks.
    readonly share = new String({}, 'value', 'Netzwerk-Share', () => this.refreshUi()).addValidator(
        (v) => this._shareValidation || ''
    )

    // Gesetzt wenn die Verzeichnisauswahl angezeigt werden soll.
    get showBrowse(): boolean {
        return (this.share.value || '').trim().length < 1
    }

    // Die aktuelle Verzeichnisauswahl.
    readonly browse = new SelectSingleFromList<string>({}, 'value', 'Server-Verzeichnis', () => this.doBrowse())

    // Befehl um in der Verzeichnisauswahl zum übergeordneten Verzeichnis zu wechseln.
    readonly parent = new Command(
        () => this.doBrowseUp(),
        'Übergeordnetes Verzeichnis',
        () => !!this.browse.value && this.showBrowse
    )

    // Befehl zum Entfernen der ausgewählten Verzeichnisse aus der Verzeichnisliste.
    readonly add = new Command(
        () => this.onAdd(),
        'Verzeichnis hinzufügen',
        () => !!this.browse.value || !this.showBrowse
    )

    // Gesetzt während die Liste der Verzeichnisse aktualisiert wird.
    private _disableBrowse = false

    // Fordert die Konfiguration an.
    protected loadAsync(): void {
        this.add.reset()
        this.remove.reset()
        this.parent.reset()

        this._shareValidation = undefined
        this.share.value = null

        // Konfiguration anfordern.
        getDirectorySettings().then((settings) => {
            // Liste der erlaubten Verzeichnisse laden.
            this.directories.allowedValues = settings?.directories.map((d) => uiValue(d)) ?? []

            // Pflege des Dateinamenmusters vorbereiten.
            this.pattern.data = settings

            // Wurzelverzeichnisse laden.
            browseDirectories('', true).then((dirs) => this.setDirectories(dirs!))
        })
    }

    // Aktualisiert die Liste der auswählbaren Verzeichnisse.
    private setDirectories(directories: string[]): void {
        // Aktualisierungen vermeiden.
        this._disableBrowse = true

        // Auswahlliste vorbereiten und mit dem ersten Verzeichnis initialisieren.
        this.browse.allowedValues = (directories || []).map((d) => uiValue(d, d || '<Bitte auswählen>'))
        this.browse.value = this.browse.allowedValues[0].value

        // Alles wie wie üblich.
        this._disableBrowse = false

        // Nun müssen wir die Aktualisierung der Oberfläche anfordern.
        this.refreshUi()

        // Beim ersten Aufruf können wir die Anwendung nun zur Bedienung freigeben.
        this.page.application.isBusy = false
    }

    // Die Auswahl des Verzeichnisses hat sich verändert.
    private doBrowse(): void {
        // Wir laden gerade die Liste.
        if (this._disableBrowse) return

        // Alle Unterverzeichnisse ermitteln.
        const folder = this.browse.value
        if (folder) browseDirectories(folder, true).then((dirs) => this.setDirectories(dirs!))
    }

    // Das übergeordnete Verzeichnis soll angezeigt werden.
    private doBrowseUp(): void {
        // In eine höhere Ansicht wechseln.
        const folder = this.browse.allowedValues[0].value
        if (folder) browseDirectories(folder, false).then((dirs) => this.setDirectories(dirs!))
    }

    // Ausgewählte Verzeichnisse aus der Liste entfernen.
    private removeDirectories(): void {
        this.directories.allowedValues = this.directories.allowedValues.filter((v) => !v.isSelected)
    }

    // Wenn das Dateimuster gültig ist, kann die Konfiguration abgespeichert werden - selbst eine leere Verzeichnisliste ist in Ordnung.
    protected get isValid(): boolean {
        return !this.pattern.message
    }

    // Sendet die veränderte Konfiguration an den VCR.NET Recording Service.
    protected saveAsync(): Promise<boolean | undefined> {
        // Die aktuell erlaubten Verzeichnisse werden als Verzeichnisliste übernommen.
        const settings: IDirectorySettingsContract = this.pattern.data

        settings.directories = this.directories.allowedValues.map((v) => v.value)

        // Neue Konfiguration senden.
        return setDirectorySettings(settings)
    }

    // Ergänzt ein Verzeichnis.
    private onAdd(): Promise<void> | null {
        // Es erfolgt eine direkte Auswahl über eine Verzeichnisliste.
        if (this.showBrowse) {
            // Sicherheitshalber prüfen wir auf eine echte Auswahl.
            const selected = this.browse.value
            if (selected) this.addDirectory(selected)

            // Das geht synchron.
            return null
        }

        // Der Anwender hat ein Netzwerkverzeichnis ausgewählt.
        const share = (this.share.value ?? '').trim()

        // Prüfergebnis zurücksetzen.
        this._shareValidation = undefined
        this.share.validate()

        // Oberfläche zur eingeschränkten Aktualisierung der Anzeige auffordern.
        this.share.refreshUi()

        // Verzeichnis durch den VCR.NET Recording Service prüfen lassen.
        return validateDirectory(share).then((ok) => {
            // Gültige Verzeichnisse werden direkt in die Liste übernommen.
            if (ok) {
                // Das brauchen wir jetzt nicht mehr.
                this.share.value = null

                // Verzeichnisliste ergänzen.
                this.addDirectory(share)
            } else {
                // Ungültige Verzeichnisse setzen die Fehlerbedingung.
                this._shareValidation = 'Ungültiges Verzeichnis'
                this.share.validate()

                // Oberfläche zur eingeschränkten Aktualisierung der Anzeige auffordern.
                this.share.refreshUi()
            }
        })
    }

    // Übernimmt ein Verzeichnis in die Liste der Verzeichnisse.
    private addDirectory(folder: string): void {
        // Als Verzeichnis aufbereiten.
        folder = folder.trim()

        if (folder.length > 0) if (folder[folder.length - 1] != '\\') folder += '\\'

        // Nur bisher unbekannte Verzeichnisse eintragen.
        if (!this.directories.allowedValues.some((v) => v.value === folder))
            this.directories.allowedValues = this.directories.allowedValues.concat([uiValue(folder)])
    }
}
