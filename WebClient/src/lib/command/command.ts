﻿import { IDisplay } from '../localizable'
import { IConnectable, IView } from '../site'

// Schnittstelle zur Anzeige einer Aktion.
export interface ICommand extends IDisplay, IConnectable {
    // Gesetzt, wenn die Aktion überhaupt angezeigt werden soll.
    readonly isVisible: boolean

    // Gesetzt, wenn die Aktion zurzeit ausgeführt werden kann.
    readonly isEnabled: boolean

    // Gesetzt, wenn die Aktion eine kritische Änderung bedeutet.
    readonly isDangerous: boolean

    // Fehlermeldung zur letzten Ausführung der Aktion.
    readonly message: string

    // Führt die Aktion aus.
    execute(): void
}

// Ui View Model zur Anzeige einer Aktion.
export class Command<TResponseType> implements ICommand {
    // Gesetzt während die Aktion ausgeführt wird.
    private _busy = false

    // Die zugehörige Anzeige.
    view: IView

    // Setzt den Befehl auf den Initialzustand zurück.
    reset(): void {
        this._message = ''
        this._busy = false

        // Anzeige aktualisieren.
        this.refreshUi()
    }

    // Fehlermeldung zur letzen Ausführung.
    private _message = ''

    // Meldet die Fehlermeldung zur letzten Ausführung.
    get message(): string {
        return this._message
    }

    // Setzt die Fehlermeldung zu einer Ausführung.
    set message(newMessage: string) {
        // Die Meldung hat sich nicht verändert.
        if (newMessage === this._message) return

        // Neue Meldung merken.
        this._message = newMessage

        // Anzeige benachrichtigen.
        this.refreshUi()
    }

    // Erstellt eine neue Repräsentation.
    constructor(
        private readonly _begin: () => Promise<TResponseType> | void | null,
        public readonly text: string | null = null,
        private readonly _test?: () => boolean
    ) {}

    // Gesetzt, wenn es sich um eine kritische Änderung handelt.
    private _dangerous = false

    // Meldet, ob es sich um eine kritische Änderung handelt.
    get isDangerous(): boolean {
        return this._dangerous
    }

    // Legt fest, ob es sich um eine kritische Änderung handelt.
    set isDangerous(newValue: boolean) {
        // Nur Aktualisieren, wenn auch tatsächlich eine Umschaltung erfolgt ist.
        if (newValue === this._dangerous) return

        // Neuen Wert übernehmen.
        this._dangerous = newValue

        // Oberfläche geeignet aktualisieren.
        this.refreshUi()
    }

    // Gesetzt, wenn die Aktion sichtbar sein soll.
    private _visible = true

    // Meldet, ob die Aktion sichtbar sein soll.
    get isVisible(): boolean {
        return this._visible
    }

    // Legt fest, ob die Aktion sichtbar sei soll.
    set isVisible(newValue: boolean) {
        // Nur Aktualisieren, wenn auch tatsächlich eine Umschaltung erfolgt ist.
        if (newValue === this._visible) return

        // Änderung vermerken.
        this._visible = newValue

        // Oberfläche aktualisieren.
        this.refreshUi()
    }

    // Prüft, ob die Aktion ausgeführt werden darf.
    get isEnabled(): boolean {
        // Insbesondere ist dies nich möglich, wenn noch eine Ausführung aktiv ist oder die Aktion gar nicht angezeigt wird.
        if (this._busy) return false
        else if (!this.isVisible) return false
        else if (this._test) return this._test()
        else return true
    }

    // Ändert den Ausführungszustand der Aktion.
    private setBusy(newVal: boolean): void {
        // Oberfläche nur bei Änderungen aktualisieren.
        if (this._busy === newVal) return

        // Änderung vermerken.
        this._busy = newVal

        // Anzeige aktualisieren.
        this.refreshUi()
    }

    // Oberfläche zur Aktualisierung auffordern.
    refreshUi(): void {
        if (this.view) this.view.refreshUi()
    }

    // Befehl ausführen.
    execute(): void {
        // Das ist im Mopment nicht möglich, zum Beispiel weil die letzte Ausführung noch nicht abgeschlossen ist.
        if (!this.isEnabled) return

        // Fehlermeldung zurücksetzen.
        this.message = ''

        // Gegen erneutes Aufrufen sperren.
        this.setBusy(true)

        // Aktion starten.
        const begin = this._begin()

        // Auf das Ende der Aktion warten und Aktion wieder freigeben.
        if (begin)
            begin.then(
                () => this.setBusy(false),
                (e) => {
                    this.message = e.message
                    this.setBusy(false)
                }
            )
        else this.setBusy(false)
    }
}
