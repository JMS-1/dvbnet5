﻿import { Command, ICommand } from '../../../lib/command/command'
import { IConnectable, IView } from '../../../lib/site'
import { AdminPage, IAdminPage } from '../admin'

// Gemeinsame Schnittstelle für alle Konfigurationsbereiche der Administration.
export interface ISection extends IConnectable {
    // Die zugehörige Konfigurationsseite.
    readonly page: IAdminPage

    // Der Befehl zum Speichern der Daten des Konfigurationsbereichs.
    readonly update: ICommand
}

// Basisklasse für einen Konfigurationsbereich.
export abstract class Section implements ISection {
    // Der Befehl zum Speichern der Daten des Konfigurationsbereichs.
    private _update: Command<void>

    // Meldet den Befehl zum Speichern, eventuell nach dem dieser einmalig erzeugt wurde.
    get update(): Command<void> {
        // Beim ersten Aufruf muss der Befehl erzeugt werden.
        if (!this._update)
            this._update = new Command(
                () => this.save(),
                this.saveCaption,
                () => this.isValid
            )

        return this._update
    }

    // Erstellt einen neuen Konfigurationsbereich.
    constructor(public readonly page: AdminPage) {}

    // Das aktuell zur Anzeige veränderte Oberflächenelement.
    view: IView

    // Benachrichtigt das aktuelle Oberflächenelement über Veränderungen in der Anzeige.
    protected refreshUi(): void {
        if (this.view) this.view.refreshUi()
    }

    // Fordert die zugehörigen Konfigurationsdaten neu an.
    protected abstract loadAsync(): void

    // Beginnt mit der eventuell erneuten Darstellung des Konfigurationsbreichs in der Oberfläche.
    reset(): void {
        // Befehl zurücksetzen.
        if (this._update) this._update.reset()

        // Konfigurationsdaten anfordern.
        this.loadAsync()
    }

    // Meldet die Beschriftung des Befehls zum Speichern der Daten des Konfigurationsbereichs.
    protected readonly saveCaption: string = 'Ändern'

    // Meldet ob die Daten des Konfigurationsbereichs gültig sind - nur dann ist der Befehl zum Speichern aktiv.
    protected get isValid(): boolean {
        return true
    }

    // Sendet die aktuellen Daten des Konfigurationsbereichs an den VCR.NET Recording Service zur Übernahme derselben.
    protected abstract saveAsync(): Promise<boolean | undefined>

    // Beginnt mit der Speicherung der aktuellen Daten des Konfigurationsbereichs im VCR.NET Recording Service.
    private save(): Promise<void> {
        return this.page.update(this.saveAsync(), this.update)
    }
}

// Beschreibt einen Konfigurationsbereich.
export interface ISectionInfo {
    // Der eindeutige Name des Konfigurationsbereichs.
    readonly route: string
}

// Beschreibt einen Konfigurationsbereich.
export interface ISectionInfoFactory extends ISectionInfo {
    // Das Präsentationsmodell des Konfigurationsbereichs.
    getOrCreate(adminPage: AdminPage): ISection | undefined
}
