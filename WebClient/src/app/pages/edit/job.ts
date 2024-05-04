import { IFlag, Flag } from '../../../lib/edit/boolean/flag'
import { IValueFromList, IUiValue, SelectSingleFromList } from '../../../lib/edit/list'
import { IEditJobContract } from '../../../web/IEditJobContract'
import { IPage } from '../page'
import { IJobScheduleEditor, JobScheduleEditor } from './jobSchedule'

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
        this.deviceLock = new Flag(this.model, 'lockedToDevice', '(auf diesem Gerät aufzeichnen)', onChange)
        this.folder = new SelectSingleFromList(this.model, 'directory', 'Verzeichnis', onChange, folders)
        this.device = new SelectSingleFromList(
            this.model,
            'device',
            'DVB.NET Geräteprofil',
            onChange,
            devices
        ).addRequiredValidator()

        // Zusätzliche Prüfungen einrichten.
        this.name.addRequiredValidator(`Ein Auftrag muss einen Namen haben.`)

        // Initiale Prüfung.
        this.name.validate()
        this.device.validate()
        this.folder.validate()
        this.deviceLock.validate()
    }

    // Das Aufzeichnungsverzeichnis.
    readonly folder: SelectSingleFromList<string>

    // Das zu verwendende DVB Gerät.
    readonly device: SelectSingleFromList<string>

    // Gesetzt, wenn die Aufzeichnung immer auf dem Gerät stattfinden soll.
    readonly deviceLock: Flag

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
