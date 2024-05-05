import { IJobEditor, JobEditor } from './edit/job'
import { IScheduleEditor, ScheduleEditor } from './edit/schedule'
import { IPage, Page } from './page'

import { Command, ICommand } from '../../lib/command/command'
import { DateTimeUtils } from '../../lib/dateTimeUtils'
import { IUiValue, uiValue } from '../../lib/edit/list'
import { IEditJobContract } from '../../web/IEditJobContract'
import { deleteSchedule, IEditScheduleContract } from '../../web/IEditScheduleContract'
import { createScheduleFromGuide, updateSchedule } from '../../web/IJobScheduleDataContract'
import { IJobScheduleInfoContract } from '../../web/IJobScheduleInfoContract'
import { ProfileCache } from '../../web/ProfileCache'
import { ProfileSourcesCache } from '../../web/ProfileSourcesCache'
import { RecordingDirectoryCache } from '../../web/RecordingDirectoryCache'
import { Application } from '../app'

// Schnittstelle zur Pflege einer einzelnen Aufzeichnung.
export interface IEditPage extends IPage {
    // Die Daten des zugehörigen Auftrags.
    readonly job?: IJobEditor

    // Die Daten der Aufzeichnung.
    readonly schedule?: IScheduleEditor

    // Befehl zum Speichern der Aufzeichnung.
    readonly save: ICommand

    // Befehl zum Löschen der Aufzeichnung.
    readonly del: ICommand
}

// Das Präsentationsmodell zur Pflege einer Aufzeichnung.
export class EditPage extends Page implements IEditPage {
    // Die Originaldaten der Aufzeichnung.
    private _jobScheduleInfo?: IJobScheduleInfoContract

    // Die Daten des zugehörigen Auftrags.
    job?: JobEditor

    // Die Daten der Aufzeichnung.
    schedule?: ScheduleEditor

    // Befehl zum Speichern der Aufzeichnung.
    readonly save = new Command(
        () => this.onSave(),
        'Übernehmen',
        () => !!this.isValid
    )

    // Befehl zum Löschen der Aufzeichnung.
    readonly del = new Command(() => this.onDelete(), 'Löschen')

    // Meldet, ob alle Eingaben konsistent sind.
    private get isValid() {
        return this.job?.isValid() && this.schedule?.isValid()
    }

    // Gesetzt, wenn die Pflegeseite aus der Programmzeitschrift aufgerufen wurde.
    private _fromGuide: boolean

    // Erstellt ein neues Präsentationsmodell.
    constructor(application: Application) {
        super('edit', application)

        // Eine neue Aufzeichnung kann von hier aus nicht mehr direkt angelegt werden.
        this.navigation.new = false
    }

    // Initialisiert das Präsentationsmodell.
    reset(sections: string[]): void {
        // Zustand zurücksetzen.
        this.del.reset()
        this.save.reset()
        this.del.isDangerous = false

        this.job = undefined
        this.schedule = undefined
        this._jobScheduleInfo = undefined

        this._fromGuide = false

        // Die Auswahlliste der Aufzeichnungsverzeichnisse.
        const folderSelection = [uiValue('', '(Voreinstellung verwenden)')]

        // Die Auswahlliste der Geräte.
        let profileSelection: IUiValue<string>[]

        // Zuerst die Liste der Aufzeichnungsverzeichnisse abfragen.
        RecordingDirectoryCache.getPromise()
            .then((folders) => {
                // Die möglichen Verzeichnisse anhängen.
                folderSelection.push(...folders.map((f) => uiValue(f)))

                // Geräteprofile anfordern.
                return ProfileCache.getAllProfiles()
            })
            .then((profiles) => {
                // Auswahl für den Anwender vorbereiten.
                profileSelection = profiles.map((p) => uiValue(p.name))

                // Auf das Neuanlegen prüfen.
                if (sections.length > 0) {
                    // Auf existierende Aufzeichnung prüfen - wir gehen hier einfach mal von der Notation id= in der URL aus.
                    const id = sections[0].substr(3)
                    const epgId = (sections[1] || 'epgid=').substr(6)

                    // Bei neuen Aufzeichnungen brauchen wir auch kein Löschen.
                    this.del.isVisible = id !== '*'

                    // Einsprung aus der Programmzeitschrift.
                    this._fromGuide = !!epgId

                    // Aufzeichnung asynchron abrufen - entweder existiert eine solche oder sie wird aus der Programmzeitschrift neu angelegt.
                    return createScheduleFromGuide(id, epgId)
                }

                // Löschen geht sicher nicht.
                this.del.isVisible = false

                // Leere Aufzeichnung angelegen.
                const newJob = <IEditJobContract>{
                    allLanguages: this.application.profile.languages,
                    dolbyDigital: this.application.profile.dolby,
                    dvbSubtitles: this.application.profile.subtitles,
                    name: '',
                    profile: profiles[0] && profiles[0].name,
                    recordingDirectory: '',
                    source: '',
                    useProfileForRecording: false,
                    videotext: this.application.profile.videotext,
                }

                // Beschreibung der Aufzeichnung vorbereiten.
                const info = <IJobScheduleInfoContract>{
                    job: newJob,
                    jobIdentifier: '',
                    schedule: this.createEmptySchedule(),
                    scheduleIdentifier: '',
                }

                // Die neue Aufzeichnung können wir auch direkt synchron bearbeiten.
                return info
            })
            .then((info) => info && this.setJobSchedule(info, profileSelection, folderSelection))
    }

    // Erstellt eine neue leere Aufzeichnung.
    private createEmptySchedule(): IEditScheduleContract {
        const now = new Date(Date.now())

        return <IEditScheduleContract>{
            allLanguages: !!this.application.profile.languages,
            dolbyDigital: !!this.application.profile.dolby,
            duration: 120,
            dvbSubtitles: !!this.application.profile.subtitles,
            exceptions: [],
            firstStartISO: new Date(
                now.getFullYear(),
                now.getMonth(),
                now.getDate(),
                now.getHours(),
                now.getMinutes()
            ).toISOString(),
            lastDayISO: '',
            name: '',
            repeatPatternJSON: 0,
            source: '',
            videotext: !!this.application.profile.videotext,
        }
    }

    // Die Daten einer existierenden Aufzeichnung stehen bereit.
    private setJobSchedule(
        info: IJobScheduleInfoContract,
        profiles: IUiValue<string>[],
        folders: IUiValue<string>[]
    ): void {
        // Liste der zuletzt verwendeten Quellen abrufen.
        const favorites = this.application.profile.recentSources || []

        // Leere Aufzeichnung anlegen.
        if (!info.schedule) info.schedule = this.createEmptySchedule()

        // Präsentationsmodelle anlegen.
        this._jobScheduleInfo = info
        this.job = new JobEditor(this, info.job, profiles, favorites, folders, () => {
            this.schedule?.source.sourceName.validate()
            this.onChanged()
        })
        this.schedule = new ScheduleEditor(
            this,
            info.schedule,
            favorites,
            () => this.onChanged(),
            () => (this.job?.source.value || '').trim().length > 0
        )

        // Quellen für das aktuelle Geräteprofil laden und die Seite für den Anwender freigeben.
        this.loadSources().then(() => (this.application.isBusy = false))
    }

    // Die aktuelle Liste der Quellen zum ausgewählten Gerät anfordern.
    private loadSources(): Promise<void> {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        const profile = this.job?.device.value

        // Das kann man ruhig öfter mal machen, da das Ergebnis nach dem ersten asynchronen Abruf gespeichert wird.
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        return ProfileSourcesCache.getSources(profile!).then((sources) => {
            // Nur übernehmen, wenn das Gerät noch passt.
            if (this.job?.device.value === profile) {
                // Diese Zuweisung ist optimiert und macht einfach nichts, wenn exakt die selbe Liste eingetragen werden soll.
                if (this.job) this.job.source.allSources = sources
                if (this.schedule) this.schedule.source.allSources = sources
            }
        })
    }

    // Meldet die Überschrift zur Anzeige des Präsentationsmodells.
    get title(): string {
        return this.del.isVisible ? 'Aufzeichnung bearbeiten' : 'Neue Aufzeichnung anlegen'
    }

    // Wird bei Änderungen ausgelöst.
    private onChanged(): void {
        // Eventuell sind wir noch in einer Startphase.
        if (!this.job) return

        // Quellen neu anfordern - da passiert im Allgemeinen nicht wirklich viel, trotzdem optimieren wir das ein bißchen.
        let requireRefresh: unknown = true

        this.loadSources().then(() => (requireRefresh = this.refreshUi()))

        // Im asynchronen Fall jetzt schon einmal aktualisieren - irgendwann später gibt es dann noch eine zweite Aktualisierung.
        if (requireRefresh) this.refreshUi()
    }

    // Beginnt mit der asynchronen Aktualisierung der Daten der Aufzeichnung.
    private onSave(): Promise<void> {
        // Kopie der Aufzeichnungsdaten anlegen.
        const schedule = { ...this._jobScheduleInfo?.schedule } as IEditScheduleContract

        // Dauer unter Berücksichtigung der Zeitumstellung anpassen.
        schedule.duration = DateTimeUtils.getRealDurationInMinutes(schedule.firstStartISO, schedule.duration)

        // Asynchrone Operation anfordern.
        return updateSchedule(
            this._jobScheduleInfo?.jobIdentifier ?? '',
            this._jobScheduleInfo?.scheduleIdentifier ?? '',
            {
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion, @typescript-eslint/no-non-null-asserted-optional-chain
                job: this._jobScheduleInfo?.job!,
                schedule,
            }
        ).then(() => {
            // Je nach Wunsch des Anwenders entweder zruück zur Programmzeitschrift oder dem Aufzeichnungsplan aufrufen.
            if (this._fromGuide && this.application.profile.backToGuide)
                this.application.gotoPage(this.application.guidePage.route)
            else this.application.gotoPage(this.application.planPage.route)
        })
    }

    // Startet einen asynchronen Löschvorgang für die Aufzeichnung.
    private onDelete(): Promise<void> | void {
        // Beim zweiten Aufruf wird der asynchrone Befehl an den VCR.NET Recording Service geschickt.
        if (this.del.isDangerous)
            return deleteSchedule(
                this._jobScheduleInfo?.jobIdentifier ?? '',
                this._jobScheduleInfo?.scheduleIdentifier ?? ''
            ).then(() => this.application.gotoPage(this.application.planPage.route))

        // Beim ersten Versuch wird einfach nur ein Feedback angezeigt.
        this.del.isDangerous = true
    }
}
