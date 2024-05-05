import { ISection, Section } from './section'

import { IUiValue, IValueFromList, SingleListProperty, uiValue } from '../../../lib/edit/list'
import {
    getSecuritySettings,
    getWindowsGroups,
    setSecuritySettings,
} from '../../../web/admin/ISecuritySettingsContract'

// Schnittstelle zur Konfiguration der Benutzergruppen.
export interface IAdminSecurityPage extends ISection {
    // Die Gruppe der normalen Benutzer.
    readonly userGroups: IValueFromList<string>

    // Die Gruppe der Administratoren.
    readonly adminGroups: IValueFromList<string>
}

// Instanz zur Pflege der Konfiguration der Benutzergruppen.
export class SecuritySection extends Section implements IAdminSecurityPage {
    // Der eindeutige Name des Bereichs.
    static readonly route = 'security'

    // Alle bekannten Windows Kontogruppen - die werden nur ein einziges Mal angefordert.
    private static _windowsGroups: Promise<IUiValue<string>[]>

    // Die Gruppe der normalen Benutzer mit Auswahl.
    readonly userGroups = new SingleListProperty<string>({}, 'users', 'Benutzer')

    // Die Gruppe der Administratoren mit Auswahl.
    readonly adminGroups = new SingleListProperty<string>({}, 'admins', 'Administratoren')

    // Beginnt mit der Abfrage der aktuellen Einstellungen.
    protected loadAsync(): void {
        getSecuritySettings()
            .then((settings) => {
                // Daten an die Präsentationsmodelle binden.
                this.userGroups.data = settings
                this.adminGroups.data = settings

                // Windows Kontogruppen einmalig anfordern.
                if (!SecuritySection._windowsGroups)
                    // Immer die Leerauswahl ergänzen - damit werden automatisch alle Benutzer erfasst.
                    SecuritySection._windowsGroups = getWindowsGroups().then((names) =>
                        [uiValue('', '(Alle Benutzer)')].concat(names?.map((name) => uiValue(name)) ?? [])
                    )

                // Windows Kontogruppen direkt oder verzögert laden.
                return SecuritySection._windowsGroups
            })
            .then((groups) => {
                // Daten in den Präsentationsmodellen berücksichtigen.
                this.userGroups.allowedValues = groups
                this.adminGroups.allowedValues = groups

                // Bedienung durch den Anwender freischalten.
                this.page.application.isBusy = false

                // Aktualisierung der Oberfläche anfordern.
                this.refreshUi()
            })
    }

    // Beginnt mit der Aktualisierung der aktuellen Einstellungen.
    protected saveAsync(): Promise<boolean | undefined> {
        return setSecuritySettings(this.userGroups.data)
    }
}
