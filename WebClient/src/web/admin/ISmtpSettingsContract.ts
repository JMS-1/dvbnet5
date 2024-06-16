import { ISettingsContract } from '../ISettingsContract'
import { doUrlCall } from '../VCRServer'

// Repräsentiert die Klasse OtherSettings
export interface ISmtpSettingsContract extends ISettingsContract {
    /* Zu verwendender SMTP Server. */
    relay: string

    /* Benutzername zur Anmeldung am SMTP Server. */
    username: string

    /* Zugehöriges Kennwort. */
    password: string

    /* Empfänger für alle Benachrichtigungen. */
    recipient: string
}

export function getSmtpSettings(): Promise<ISmtpSettingsContract | undefined> {
    return doUrlCall('configuration/smtp')
}

export function setSmtpSettings(data: ISmtpSettingsContract): Promise<boolean | undefined> {
    return doUrlCall('configuration/smtp', 'PUT', data)
}
