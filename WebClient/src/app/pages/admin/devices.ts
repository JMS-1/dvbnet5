﻿import { Device, IDevice } from './device'
import { ISection, Section } from './section'

import { IValueFromList, SingleListProperty, uiValue } from '../../../lib/edit/list'
import * as contract from '../../../web/admin/IProfileSettingsContract'

// Schnittstelle zur Konfiguration der Geräteprofile.
export interface IAdminDevicesPage extends ISection {
    // Erlaubt die Auswahl des bevorzugten Gerätes.
    readonly defaultDevice: IValueFromList<string>

    // Alle bekannten Geräte.
    readonly devices: IDevice[]
}

// Präsentationsmodell zur Konfiguration der Geräteprofile.
export class DevicesSection extends Section implements IAdminDevicesPage {
    // Der eindeutige Name des Bereichs.
    static readonly route = 'devices'

    // Präsentationsmodell zur Pflege des bevorzugten Gerätes.
    readonly defaultDevice = new SingleListProperty(
        {} as Pick<contract.IProfileSettingsContract, 'defaultProfile'>,
        'defaultProfile',
        'Bevorzugtes Gerät (zum Beispiel für neue Aufzeichnungen)',
        () => this.refresh()
    )
        .addRequiredValidator()
        .addValidator((list) => this.validateDefaultDevice(list.value))

    // Alle bekannten Gerät.
    devices: Device[] = []

    // Fordert die Konfiguration der Geräteprofile an.
    protected loadAsync(): void {
        contract.getProfileSettings().then((settings) => {
            // Präsentationsmodelle aus den Rohdaten erstellen.
            this.devices =
                settings?.systemProfiles.map(
                    (p) =>
                        new Device(
                            p,
                            () => this.refresh(),
                            () => this.defaultDevice.value ?? ''
                        )
                ) ?? []

            // Liste der Geräte ermitteln und in die Auswahl für das bevorzugte Gerät übernehmen.
            this.defaultDevice.allowedValues = settings?.systemProfiles.map((p) => uiValue(p.name)) ?? []
            this.defaultDevice.data = settings

            // Initiale Prüfung durchführen.
            this.refresh()

            // Anwendung freischalten.
            this.page.application.isBusy = false
        })
    }

    // Prüft, ob das bevorzugte Gerät auch verwendet werden darf.
    private validateDefaultDevice(defaultDevice: string | null): string | undefined {
        if (this.devices.filter((d) => d.name === defaultDevice).some((d) => !d.active.value))
            return 'Dieses Gerät ist nicht für Aufzeichnungen vorgesehen'
    }

    // Aktualisiert die Anzeige.
    refresh(): void {
        // Alle Eingaben gemeinsam prüfen.
        this.devices.forEach((d) => d.active.validate())
        this.defaultDevice.validate()

        // Oberfläche zur Aktualisierung auffordern.
        this.refreshUi()
    }

    // Beschriftung der Schaltfläche zur Aktualisierung der Konfiguration.
    protected readonly saveCaption = 'Ändern und neu Starten'

    // Gesetzt, wenn ein Speichern möglich ist.
    protected get isValid(): boolean {
        return !this.defaultDevice.message && this.devices.every((d) => d.isValid)
    }

    // Sendet die Konfiguration zur asynchronen Aktualisierung an den VCR.NET Recording Service.
    protected saveAsync(): Promise<boolean | undefined> {
        return contract.setProfileSettings(this.defaultDevice.data)
    }
}
