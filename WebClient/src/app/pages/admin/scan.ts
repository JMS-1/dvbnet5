import { ISection, Section } from './section'

import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IValueFromList, SingleListProperty, uiValue } from '../../../lib/edit/list'
import { IMultiValueFromList, MultiListProperty } from '../../../lib/edit/multiList'
import { INumber, NumberProperty } from '../../../lib/edit/number/number'
import * as contract from '../../../web/admin/ISourceScanSettingsContract'
import { AdminPage } from '../admin'

// Die Art der Aktualisierung der Quellenlisten.
export enum scanConfigMode {
    // Eine Aktualisierung ist nicht möglich.
    Disabled,

    // Die Aktualisierung wird manuell gestartet.
    Manual,

    // Die Aktualisierung wird nach einem Zeitplan durchgeführt.
    Automatic,
}

// Schnittstelle zur Konfiguration des Sendersuchlaufs.
export interface IAdminScanPage extends ISection {
    // Die Art der Aktualisierung.
    readonly mode: IValueFromList<scanConfigMode>

    // Gesetzt, wenn die Konfiguration überhaupt angezeigt werden soll.
    readonly showConfiguration: boolean

    // Gesetzt, wenn die Einstellungen der automatischen Aktualisierung angezeigt werden sollen.
    readonly configureAutomatic: boolean

    // Die maximale Dauer eines Suchlaufs (in Minuten).
    readonly duration: INumber

    // Die Stunden, an denen eine Aktualisierung ausgeführt werden soll.
    readonly hours: IMultiValueFromList<number>

    // Gesetzt, wenn das Ergebnis der Aktualisierung mit der aktuellen Liste der Quellen zusammengeführt werden soll.
    readonly merge: IFlag

    // Die minimale zeit zwischen zwei automatischen Aktualisierungen (in Tagen).
    readonly gapDays: INumber

    // Die Zeit für eine vorgezogene Aktualisierung (in Tagen).
    readonly latency: INumber
}

// Präsentationsmodell zur Pflege der Konfiguration des Sendersuchlaufs.
export class ScanSection extends Section implements IAdminScanPage {
    // Der eindeutige Name des Bereichs.
    static readonly route = 'scan'

    // Die Anzeigewerte für die einzelnen Arten der Aktualisierung.
    private static readonly _scanModes = [
        uiValue(scanConfigMode.Disabled, 'Aktualisierung deaktivieren'),
        uiValue(scanConfigMode.Manual, 'Manuell aktualisieren'),
        uiValue(scanConfigMode.Automatic, 'Aktualisieren nach Zeitplan'),
    ]

    // Die Art der Aktualisierung.
    readonly mode = new SingleListProperty(
        {} as { value?: scanConfigMode },
        'value',
        undefined,
        () => this.refreshUi(),
        ScanSection._scanModes
    )

    // Die Stunden, an denen eine Aktualisierung ausgeführt werden soll.
    readonly hours = new MultiListProperty(
        {} as Pick<contract.ISourceScanSettingsContract, 'hours'>,
        'hours',
        'Uhrzeiten',
        undefined,
        AdminPage.hoursOfDay
    )

    // Die maximale Dauer eines Suchlaufs (in Minuten).
    readonly duration = new NumberProperty(
        {} as Pick<contract.ISourceScanSettingsContract, 'duration'>,
        'duration',
        'Maximale Laufzeit für einen Sendersuchlauf in Minuten',
        () => this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(5)
        .addMaxValidator(55)

    // Gesetzt, wenn das Ergebnis der Aktualisierung mit der aktuellen Liste der Quellen zusammengeführt werden soll.
    readonly merge = new BooleanProperty(
        {} as Pick<contract.ISourceScanSettingsContract, 'mergeLists'>,
        'mergeLists',
        'Senderliste nach dem Suchlauf mit der vorherigen zusammenführen (empfohlen)'
    )

    // Die minimale zeit zwischen zwei automatischen Aktualisierungen (in Tagen).
    readonly gapDays = new NumberProperty(
        {} as Pick<contract.ISourceScanSettingsContract, 'interval'>,
        'interval',
        'Minimale Anzahl von Tagen zwischen zwei Suchläufen',
        () => this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(1)
        .addMaxValidator(28)

    // Die Zeit für eine vorgezogene Aktualisierung (in Tagen).
    readonly latency = new NumberProperty(
        {} as Pick<contract.ISourceScanSettingsContract, 'threshold'>,
        'threshold',
        'Latenzzeit für vorgezogene Aktualisierungen in Tagen (optional)',
        () => this.update.refreshUi()
    )
        .addMinValidator(1)
        .addMaxValidator(14)

    // Gesetzt, wenn die Konfiguration überhaupt angezeigt werden soll.
    get showConfiguration(): boolean {
        return this.mode.value !== scanConfigMode.Disabled
    }

    // Gesetzt, wenn die Einstellungen der automatischen Aktualisierung angezeigt werden sollen.
    get configureAutomatic(): boolean {
        return this.mode.value === scanConfigMode.Automatic
    }

    // Forder die aktuelle Konfiguration vom VCR.NET Recordings Service an.
    protected loadAsync(): void {
        contract.getSourceScanSettings().then((settings) => {
            // Präsentationsmodell mit den Daten verbinden.
            this.duration.data = settings
            this.gapDays.data = settings
            this.latency.data = settings
            this.hours.data = settings
            this.merge.data = settings

            // Die Art erfordert eine Sonderbehandlung.
            if (settings?.interval === null) this.mode.value = scanConfigMode.Disabled
            else if ((settings?.interval ?? 0) < 0) {
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                settings!.interval = 0

                this.mode.value = scanConfigMode.Manual
            } else this.mode.value = scanConfigMode.Automatic

            // Anwendung zur Bedienung freischalten.
            this.page.application.isBusy = false

            // Anzeige zur Aktualisierung auffordern.
            this.refreshUi()
        })
    }

    // Prüft alle Einstellungen auf Konsistenz.
    protected get isValid(): boolean {
        // Nicht notwendig wenn alles deaktiviert ist.
        if (!this.showConfiguration) return true

        // Zumindest die Grundeinstellungen für die manuelle Aktualisierung prüfen.
        if (this.duration.message) return false

        // Zusätzlich eventuell auch noch die Einstellungen der automatischen Aktualisierung.
        if (!this.configureAutomatic) return true
        if (this.gapDays.message) return false
        if (this.latency.message) return false

        // Speicherung ist möglich.
        return true
    }

    // Fordert den VCR.NET Recording Service zur Aktualisierung der Konfiguration an.
    protected saveAsync(): Promise<boolean | undefined> {
        // Die Art wird in die Konfigurationsdaten zurückgespiegelt.
        const settings = <contract.ISourceScanSettingsContract>this.hours.data

        if (!this.showConfiguration) settings.interval = 0
        else if (!this.configureAutomatic) settings.interval = -1

        // Änderung der Konfiguration asynchron starten.
        return contract.setSourceScanSettings(settings)
    }
}
