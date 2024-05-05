import { ISection, Section } from './section'

import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IValueFromList, SingleListProperty, uiValue } from '../../../lib/edit/list'
import { INumber, NumberProperty } from '../../../lib/edit/number/number'
import { getOtherSettings, setOtherSettings } from '../../../web/admin/IOtherSettingsContract'

// Die Art des zu verwendenden Schlafzustands.
export enum hibernationMode {
    // Kein automatischer Übergang in den Schlafzustand.
    Disabled,

    // Für den Schlafzustand Stand-By verwenden.
    StandBy,

    // Für den Schlafzustand Hibernate verwenden.
    Hibernate,
}

// Schnittstelle zur Pflege sonstiger Konfigurationswerte.
export interface IAdminOtherPage extends ISection {
    // Der TCP/IP Port des Web Clients.
    readonly port: INumber

    // Gesetzt, wenn auch eine sichere Verbindung (SSL / HTTPS) unterstützt werden soll.
    readonly ssl: IFlag

    // Der sichere (SSL) TCP/IP Port des Web Clients.
    readonly securePort: INumber

    // Gesetzt, wenn neben der integrierten Windows Sicherheit (NTLM Challenge/Response) auch die Standard Autorisierung (Basic) verwendet werden kann.
    readonly basicAuth: IFlag

    // Die Zeit zum vorzeitigen Aufwachen für eine Aufzeichnung oder Sonderaufgabe (in Sekunden).
    readonly preSleep: INumber

    // Die minimale Verweildauer im Schalfzustand (in Minuten).
    readonly minSleep: INumber

    // Gesetzt um die minimale Verweildauer im Schlafzustand zu unterdrücken.
    readonly ignoreMinSleep: IFlag

    // Die Verweildauer eines Protokolleintrags vor der automatischen Löscung (in Wochen).
    readonly logKeep: INumber

    // Die Verweildauer eines Auftrags im Archiv vor der automatischen Löschung (in Wochen).
    readonly jobKeep: INumber

    // Gesetzt, wenn die Systemzeit einer HDTV Aufzeichnung nicht automatisch ermittelt werden soll.
    readonly noH264PCR: IFlag

    // Gesetzt, wenn die Systemzeit einer SDTV Aufzeichnung nicht automatisch ermittelt werden soll.
    readonly noMPEG2PCR: IFlag

    // Die Art des automatischen Schlafzustands.
    readonly hibernation: IValueFromList<hibernationMode>

    // Die Art der Protokollierung.
    readonly logging: IValueFromList<string>
}

// Präsentationsmodell zur Pflege sonstiger Konfigurationswerte.
export class OtherSection extends Section implements IAdminOtherPage {
    // Der eindeutige Name des Bereichs.
    static readonly route = 'other'

    // Die einzelnen Arten der Protokollierung als Auswahlliste für den Anwender.
    private static readonly _logging = [
        uiValue('Errors', 'Nur Fehler'),
        uiValue('Security', 'Nur Sicherheitsprobleme'),
        uiValue('Schedules', 'Aufzeichnungen'),
        uiValue('Full', 'Vollständig'),
    ]

    // Die einzelnen Arten des Schlafzustands als Auswahlliste für den Anwender.
    private static readonly _hibernation = [
        uiValue(hibernationMode.Disabled, 'Nicht verwenden'),
        uiValue(hibernationMode.StandBy, 'StandBy / Suspend (S3)'),
        uiValue(hibernationMode.Hibernate, 'Hibernate (S4)'),
    ]

    // Der TCP/IP Port des Web Clients.
    readonly port = new NumberProperty({}, 'webPort', 'TCP/IP Port für den Web Server', () => this.update.refreshUi())
        .addRequiredValidator()
        .addMinValidator(1)
        .addMaxValidator(0xffff)

    // Gesetzt, wenn auch eine sichere Verbindung (SSL / HTTPS) unterstützt werden soll.
    readonly ssl = new BooleanProperty({}, 'ssl', 'Sichere Verbindung zusätzlich anbieten')

    // Der sichere (SSL) TCP/IP Port des Web Clients.
    readonly securePort = new NumberProperty({}, 'sslPort', 'TCP/IP Port für den sicheren Zugang', () =>
        this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(1)
        .addMaxValidator(0xffff)

    // Gesetzt, wenn neben der integrierten Windows Sicherheit (NTLM Challenge/Response) auch die Standard Autorisierung (Basic) verwendet werden kann.
    readonly basicAuth = new BooleanProperty(
        {},
        'basicAuth',
        'Benutzererkennung über Basic (RFC 2617) zusätzlich erlauben (nicht empfohlen)'
    )

    // Die Zeit zum vorzeitigen Aufwachen für eine Aufzeichnung oder Sonderaufgabe (in Sekunden).
    readonly preSleep = new NumberProperty(
        {},
        'hibernationDelay',
        'Vorlaufzeit für das Aufwachen aus dem Schlafzustand in Sekunden',
        () => this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(0)
        .addMaxValidator(600)

    // Die minimale Verweildauer im Schalfzustand (in Minuten).
    readonly minSleep = new NumberProperty(
        {},
        'forcedHibernationDelay',
        'Minimale Pause nach einem erzwungenen Schlafzustand in Minuten',
        () => this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(5)
        .addMaxValidator(60)

    // Gesetzt um die minimale Verweildauer im Schlafzustand zu unterdrücken.
    readonly ignoreMinSleep = new BooleanProperty(
        {},
        'suppressHibernationDelay',
        'Pause für erzwungenen Schlafzustand ignorieren'
    )

    // Die Verweildauer eines Protokolleintrags vor der automatischen Löscung (in Wochen).
    readonly logKeep = new NumberProperty({}, 'protocol', 'Aufbewahrungsdauer für Protokolle in Wochen', () =>
        this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(1)
        .addMaxValidator(13)

    // Die Verweildauer eines Auftrags im Archiv vor der automatischen Löschung (in Wochen).
    readonly jobKeep = new NumberProperty(
        {},
        'archive',
        'Aufbewahrungsdauer von archivierten Aufzeichnungen in Wochen',
        () => this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(1)
        .addMaxValidator(13)

    // Gesetzt, wenn die Systemzeit einer HDTV Aufzeichnung nicht automatisch ermittelt werden soll.
    readonly noH264PCR = new BooleanProperty(
        {},
        'noH264PCR',
        'Systemzeit (PCR) in Aufzeichnungsdateien nicht aus einem H.264 Bildsignal ableiten'
    )

    // Gesetzt, wenn die Systemzeit einer SDTV Aufzeichnung nicht automatisch ermittelt werden soll.
    readonly noMPEG2PCR = new BooleanProperty(
        {},
        'noMPEG2PCR',
        'Systemzeit (PCR) in Aufzeichnungsdateien nicht aus einem MPEG2 Bildsignal ableiten'
    )

    // Die Art des automatischen Schlafzustands.
    readonly hibernation = new SingleListProperty(
        this,
        'hibernationMode',
        'Art des von VCR.NET ausgelösten Schlafzustands',
        undefined,
        OtherSection._hibernation
    )

    // Die Art der Protokollierung.
    readonly logging = new SingleListProperty(
        {},
        'logging',
        'Umfang der Protokollierung in das Windows Ereignisprotokoll',
        undefined,
        OtherSection._logging
    )

    // Fordert die Konfigurationswerte vom VCR.NET Recording Service an.
    protected loadAsync(): void {
        getOtherSettings().then((settings) => {
            // Alle Präsentationsmodelle verbinden.
            this.ignoreMinSleep.data = settings
            this.noMPEG2PCR.data = settings
            this.securePort.data = settings
            this.basicAuth.data = settings
            this.noH264PCR.data = settings
            this.minSleep.data = settings
            this.preSleep.data = settings
            this.jobKeep.data = settings
            this.logKeep.data = settings
            this.logging.data = settings
            this.port.data = settings
            this.ssl.data = settings

            // Die Anwendung kann nun verwendet werden.
            this.page.application.isBusy = false

            // Anzeige aktualisieren lassen.
            this.refreshUi()
        })
    }

    // Die Beschriftung der Schaltfläche zum Speichern.
    protected readonly saveCaption = 'Ändern und eventuell neu Starten'

    // Gesetzt, wenn ein Speichern möglich ist.
    protected get isValid(): boolean {
        // Alle Zahlen müssen fehlerfrei eingegeben worden sein - dazu gehört immer auch ein Wertebereich.
        if (this.port.message) return false
        if (this.securePort.message) return false
        if (this.hibernation.message) return false
        if (this.preSleep.message) return false
        if (this.minSleep.message) return false
        if (this.logKeep.message) return false
        if (this.jobKeep.message) return false
        if (this.logging.message) return false

        // Ja, das geht.
        return true
    }

    // Beginnt die asynchrone Speicherung der Konfiguration.
    protected saveAsync(): Promise<boolean | undefined> {
        return setOtherSettings(this.port.data)
    }
}
