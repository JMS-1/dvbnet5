import { ISection, Section } from './section'

import { IString, StringProperty } from '../../../lib/edit/text/text'
import { getSmtpSettings, ISmtpSettingsContract, setSmtpSettings } from '../../../web/admin/ISmtpSettingsContract'

// Schnittstelle zur Pflege der Einstellungen zum E-Mail Versand.
export interface IAdminSmtpPage extends ISection {
    /* Zu verwendender SMTP Server. */
    readonly relay: IString

    /* Benutzername zur Anmeldung am SMTP Server. */
    readonly username: IString

    /* Zugehöriges Kennwort. */
    readonly password: IString

    /* Empfänger für alle Benachrichtigungen. */
    readonly recipient: IString
}

// Präsentationsmodell zur Pflege sonstiger Konfigurationswerte.
export class SmtpSection extends Section implements IAdminSmtpPage {
    // Der eindeutige Name des Bereichs.
    static readonly route = 'smtp'

    // Zu verwendender SMTP Server
    readonly relay = new StringProperty({} as Pick<ISmtpSettingsContract, 'relay'>, 'relay', 'Name des SMTP Servers')

    // Benutzername zur Anmeldung am SMTP Server
    readonly username = new StringProperty(
        {} as Pick<ISmtpSettingsContract, 'username'>,
        'username',
        'Benutzername zur Anmeldung am SMTP Server'
    )

    // Zugehöriges Kennwort
    readonly password = new StringProperty(
        {} as Pick<ISmtpSettingsContract, 'password'>,
        'password',
        'Zugehöriges Kennwort'
    )

    // Empfänger für alle Benachrichtigungen
    readonly recipient = new StringProperty(
        {} as Pick<ISmtpSettingsContract, 'recipient'>,
        'recipient',
        'Empfänger für alle Benachrichtigungen'
    )
    // Fordert die Konfigurationswerte vom VCR.NET Recording Service an.
    protected loadAsync(): void {
        getSmtpSettings().then((settings) => {
            // Alle Präsentationsmodelle verbinden.
            this.password.data = settings
            this.recipient.data = settings
            this.relay.data = settings
            this.username.data = settings

            // Die Anwendung kann nun verwendet werden.
            this.page.application.isBusy = false

            // Anzeige aktualisieren lassen.
            this.refreshUi()
        })
    }

    // Die Beschriftung der Schaltfläche zum Speichern.
    protected readonly saveCaption = 'Ändern'

    // Beginnt die asynchrone Speicherung der Konfiguration.
    protected saveAsync(): Promise<boolean | undefined> {
        return setSmtpSettings(this.relay.data)
    }
}
