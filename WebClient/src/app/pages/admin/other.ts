import { ISection, Section } from './section'

import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IValueFromList, SingleListProperty, uiValue } from '../../../lib/edit/list'
import { INumber, NumberProperty } from '../../../lib/edit/number/number'
import { getOtherSettings, IOtherSettingsContract, setOtherSettings } from '../../../web/admin/IOtherSettingsContract'

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
    // Die Verweildauer eines Protokolleintrags vor der automatischen Löscung (in Wochen).
    readonly logKeep: INumber

    // Die Verweildauer eines Auftrags im Archiv vor der automatischen Löschung (in Wochen).
    readonly jobKeep: INumber

    // Gesetzt, wenn die Systemzeit einer HDTV Aufzeichnung nicht automatisch ermittelt werden soll.
    readonly noH264PCR: IFlag

    // Gesetzt, wenn die Systemzeit einer SDTV Aufzeichnung nicht automatisch ermittelt werden soll.
    readonly noMPEG2PCR: IFlag

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

    // Die Verweildauer eines Protokolleintrags vor der automatischen Löscung (in Wochen).
    readonly logKeep = new NumberProperty(
        {} as Pick<IOtherSettingsContract, 'protocolTime'>,
        'protocolTime',
        'Aufbewahrungsdauer für Protokolle in Wochen',
        () => this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(1)
        .addMaxValidator(13)

    // Die Verweildauer eines Auftrags im Archiv vor der automatischen Löschung (in Wochen).
    readonly jobKeep = new NumberProperty(
        {} as Pick<IOtherSettingsContract, 'archiveTime'>,
        'archiveTime',
        'Aufbewahrungsdauer von archivierten Aufzeichnungen in Wochen',
        () => this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(1)
        .addMaxValidator(13)

    // Gesetzt, wenn die Systemzeit einer HDTV Aufzeichnung nicht automatisch ermittelt werden soll.
    readonly noH264PCR = new BooleanProperty(
        {} as Pick<IOtherSettingsContract, 'disablePCRFromH264'>,
        'disablePCRFromH264',
        'Systemzeit (PCR) in Aufzeichnungsdateien nicht aus einem H.264 Bildsignal ableiten'
    )

    // Gesetzt, wenn die Systemzeit einer SDTV Aufzeichnung nicht automatisch ermittelt werden soll.
    readonly noMPEG2PCR = new BooleanProperty(
        {} as Pick<IOtherSettingsContract, 'disablePCRFromMPEG2'>,
        'disablePCRFromMPEG2',
        'Systemzeit (PCR) in Aufzeichnungsdateien nicht aus einem MPEG2 Bildsignal ableiten'
    )

    // Die Art der Protokollierung.
    readonly logging = new SingleListProperty(
        {} as Pick<IOtherSettingsContract, 'logging'>,
        'logging',
        'Umfang der Protokollierung in das Windows Ereignisprotokoll',
        undefined,
        OtherSection._logging
    )

    // Fordert die Konfigurationswerte vom VCR.NET Recording Service an.
    protected loadAsync(): void {
        getOtherSettings().then((settings) => {
            // Alle Präsentationsmodelle verbinden.
            this.noMPEG2PCR.data = settings
            this.noH264PCR.data = settings
            this.jobKeep.data = settings
            this.logKeep.data = settings
            this.logging.data = settings

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
        if (this.logKeep.message) return false
        if (this.jobKeep.message) return false
        if (this.logging.message) return false

        // Ja, das geht.
        return true
    }

    // Beginnt die asynchrone Speicherung der Konfiguration.
    protected saveAsync(): Promise<boolean | undefined> {
        return setOtherSettings(this.logging.data)
    }
}
