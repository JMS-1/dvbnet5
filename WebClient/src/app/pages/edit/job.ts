import { IJobScheduleEditor, JobScheduleEditor } from './jobSchedule'

import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IUiValue, IValueFromList, SingleListProperty } from '../../../lib/edit/list'
import { IEditJobContract } from '../../../web/IEditJobContract'
import { IPage } from '../page'

// Schnittstelle zur Pflege eines Auftrags.
export interface IJobEditor extends IJobScheduleEditor {
    // Das Aufzeichnungsverzeichnis.
    readonly folder: IValueFromList<string>

    // Das zu verwendende DVB Gerät.
    readonly device: IValueFromList<string>

    // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
    readonly deviceLock: IFlag
}

// Bietet die Daten eines Auftrags zur Pflege an.
export class JobEditor extends JobScheduleEditor<IEditJobContract> implements IJobEditor {
    // Erstellt ein neues Präsentationsmodell.
    constructor(
        page: IPage,
        model: IEditJobContract,
        devices: IUiValue<string>[],
        favoriteSources: string[],
        folders: IUiValue<string>[],
        onChange: () => void
    ) {
        super(page, model, favoriteSources, onChange)

        // Pflegekomponenten erstellen
        this.deviceLock = new BooleanProperty(
            this.model,
            'useProfileForRecording',
            '(auf diesem Gerät aufzeichnen)',
            onChange
        )
        this.folder = new SingleListProperty(this.model, 'recordingDirectory', 'Verzeichnis', onChange, folders)
        this.device = new SingleListProperty(
            this.model,
            'profile',
            'DVB.NET Geräteprofil',
            onChange,
            devices
        ).addRequiredValidator()

        // Zusätzliche Prüfungen einrichten.
        this.name.addRequiredValidator('Ein Auftrag muss einen Namen haben.')

        // Initiale Prüfung.
        this.name.validate()
        this.device.validate()
        this.folder.validate()
        this.deviceLock.validate()
    }

    // Das Aufzeichnungsverzeichnis.
    readonly folder

    // Das zu verwendende DVB Gerät.
    readonly device

    // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
    readonly deviceLock

    // Gesetzt, wenn die Einstellungen des Auftrags gültig sind.
    isValid(): boolean {
        // Erst einmal die Basisklasse fragen.
        if (!super.isValid()) return false

        // Dann alle unsere eigenen Präsentationsmodelle.
        if (this.device.message) return false
        if (this.folder.message) return false
        if (this.deviceLock.message) return false

        return true
    }
}
