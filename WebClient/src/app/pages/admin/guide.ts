import { ISection, Section } from './section'

import { Command, ICommand } from '../../../lib/command/command'
import { BooleanProperty, IFlag } from '../../../lib/edit/boolean/flag'
import { IValueFromList, SingleListProperty, uiValue } from '../../../lib/edit/list'
import { IMultiValueFromList, MultiListProperty } from '../../../lib/edit/multiList'
import { INumber, NumberProperty } from '../../../lib/edit/number/number'
import { getGuideSettings, IGuideSettingsContract, setGuideSettings } from '../../../web/admin/IGuideSettingsContract'
import { ProfileCache } from '../../../web/ProfileCache'
import { ProfileSourcesCache } from '../../../web/ProfileSourcesCache'
import { AdminPage } from '../admin'
import { ChannelProperty, IChannelSelector } from '../channel'

// Schnittstelle zur Pflege der Konfiguration der Programmzeitschrift.
export interface IAdminGuidePage extends ISection {
    // Gesetzt, wenn die automatische Aktualisierung der Programmzeitschrift aktiviert wurde.
    readonly isActive: IFlag

    // Die Liste der Stunden, an denen eine automatische Aktivierung stattfinden soll.
    readonly hours: IMultiValueFromList<number>

    // Alle Quellen, deren Programmzeitschrift ausgelesen werden soll.
    readonly sources: IMultiValueFromList<string>

    // Gesetzt, wenn auch die englische programmzeitschrift eingeschlossen werden soll.
    readonly ukTv: IFlag

    // Die Auswahl eines Geräte für die folgende Auswahl einer Quelle.
    readonly device: IValueFromList<string>

    // Die Auswahl einer Quelle des aktuell ausgewählten Gerätes.
    readonly source: IChannelSelector

    // Entfernt die ausgewählten Quellen aus der Liste der zu untersuchenden Quellen.
    readonly remove: ICommand

    // Fügt eine Quelle zur Liste der zu untersuchenden Quellen hinzu.
    readonly add: ICommand

    // Maximale Dauer für die Sammlung der Programmzeitschrift (in Minuten).
    readonly duration: INumber

    // Minimale Dauer zwischen zwei Sammlungen (in Minuten).
    readonly delay: INumber

    // Interval für die vorgezogene Sammlung (in Minuten).
    readonly latency: INumber
}

// Präsentationsmodell zur Pflege der Konfiguration der Aktualisierung der Programmzeitschrift.
export class GuideSection extends Section implements IAdminGuidePage {
    // Der eindeutige Name des Bereichs.
    static readonly route = 'guide'

    // Gesetzt, wenn die automatische Aktualisierung der Programmzeitschrift aktiviert wurde.
    readonly isActive = new BooleanProperty({} as { value?: boolean }, 'value', 'Aktualisierung aktivieren', () =>
        this.refreshUi()
    )

    // Die Liste der Stunden, an denen eine automatische Aktivierung stattfinden soll.
    readonly hours = new MultiListProperty(
        {} as { hours?: number },
        'hours',
        'Uhrzeiten',
        undefined,
        AdminPage.hoursOfDay
    )

    // Alle Quellen, deren Programmzeitschrift ausgelesen werden soll.
    readonly sources = new MultiListProperty(
        {} as { value?: string },
        'value',
        undefined,
        () => this.remove && this.remove.refreshUi()
    )

    // Gesetzt, wenn auch die englische programmzeitschrift eingeschlossen werden soll.
    readonly ukTv = new BooleanProperty(
        {} as { includeUK?: boolean },
        'includeUK',
        'Sendungsvorschau englischer Sender (FreeSat UK) abrufen'
    )

    // Entfernt die ausgewählten Quellen aus der Liste der zu untersuchenden Quellen.
    readonly remove: Command<unknown> = new Command(
        () => this.removeSources(),
        'Entfernen',
        () => (this.sources.value?.length ?? 0) > 0
    )

    // Die Auswahl eines Geräte für die folgende Auswahl einer Quelle.
    readonly device = new SingleListProperty(
        {} as { value?: string },
        'value',
        'Gerät',
        () => !this.page.application.isBusy && this.loadSources()
    )

    // Die Auswahl einer Quelle des aktuell ausgewählten Gerätes.
    readonly source

    // Fügt eine Quelle zur Liste der zu untersuchenden Quellen hinzu.
    readonly add = new Command(
        () => this.addSource(),
        'Quelle hinzufügen',
        () => !!this.source.value && this.sources.allowedValues.every((v) => v.value !== this.source.value)
    )

    // Maximale Dauer für die Sammlung der Programmzeitschrift (in Minuten).
    readonly duration = new NumberProperty(
        {} as { duration?: number },
        'duration',
        'Maximale Laufzeit einer Aktualisierung in Minuten',
        () => this.update.refreshUi()
    )
        .addRequiredValidator()
        .addMinValidator(5)
        .addMaxValidator(55)

    // Minimale Dauer zwischen zwei Sammlungen (in Minuten).
    readonly delay = new NumberProperty(
        {} as { minDelay?: number },
        'minDelay',
        'Wartezeit zwischen zwei Aktualisierungen in Stunden (optional)',
        () => this.update.refreshUi()
    )
        .addMinValidator(1)
        .addMaxValidator(23)

    // Interval für die vorgezogene Sammlung (in Minuten).
    readonly latency = new NumberProperty(
        {} as { joinHours?: number },
        'joinHours',
        'Latenzzeit für vorgezogene Aktualisierungen in Stunden (optional)',
        () => this.update.refreshUi()
    )
        .addMinValidator(1)
        .addMaxValidator(23)

    // Erstellt ein neues Präsentationsmodell.
    constructor(page: AdminPage) {
        super(page)

        // Auswahl der Quelle vorbereiten.
        this.source = new ChannelProperty(
            page.application.profile,
            {} as { value?: boolean },
            'value',
            this.page.application.profile.recentSources || [],
            () => this.refreshUi()
        )
    }

    // Forder die Konfiguration zur Pflege der Programmzeitschrift an.
    protected loadAsync(): void {
        // Neu initialisieren.
        this.add.reset()
        this.remove.reset()

        this.device.value = null
        this.source.allSources = []
        this.source.value = ''

        // Daten vom VCR.NET Recording Service abrufen.
        getGuideSettings()
            .then((settings) => {
                // Daten mit den Präsentationsmodellen verbinden.
                this.isActive.value = (settings?.duration ?? 0) > 0
                this.duration.data = settings
                this.latency.data = settings
                this.hours.data = settings
                this.delay.data = settings
                this.ukTv.data = settings

                // Die aktuelle Liste der Quellen laden.
                this.sources.allowedValues = settings?.sources.map((s) => uiValue(s)) ?? []

                // Liste der Geräteprofile anfordern.
                return ProfileCache.getAllProfiles()
            })
            .then((profiles) => {
                // Alle bekannten Geräteprofile.
                this.device.allowedValues = profiles.map((p) => uiValue(p.name))

                // Das erste Profil auswählen.
                if (this.device.allowedValues.length > 0) this.device.value = this.device.allowedValues[0].value

                // Die Liste der Quellen aktualisieren.
                this.loadSources()
            })
    }

    // Prüft, ob ein Speichern möglich ist.
    protected get isValid(): boolean {
        // Immer, wenn die automatische Aktualisierung deaktiviert ist.
        if (!this.isActive.value) return true

        // Alle Zahlen müssen fehlerfrei sein.
        if (this.duration.message) return false
        if (this.latency.message) return false
        if (this.delay.message) return false

        // Dann können wir auch speichern.
        return true
    }

    // Fordert die Liste der Quellen vom aktuellen ausgewählten Gerät an.
    private loadSources(): void {
        ProfileSourcesCache.getSources(this.device.value ?? '').then((sources) => {
            // Auswahlliste setzen.
            this.source.allSources = sources

            // Anwendung zur Benutzung freischalten.
            this.page.application.isBusy = false

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi()
        })
    }

    // Alle ausgewählten Quellen entfernen.
    private removeSources(): void {
        this.sources.allowedValues = this.sources.allowedValues.filter((v) => !v.isSelected)
    }

    // Neue Quelle zur Liste der zu berücksichtigenden Quellen hinzufügen.
    private addSource(): void {
        this.sources.allowedValues = this.sources.allowedValues.concat([uiValue(this.source.value ?? '')])

        // Die Auswahl setzen wir aber direkt wieder zurück.
        this.source.value = ''
    }

    // Die Konfiguration zur Aktualisierung an den VCR.NET Recording Service übertragen.
    protected saveAsync(): Promise<boolean | undefined> {
        // Die Auswahlliste der Quellen ist die Liste der zu berücksichtigenden Quellen.
        const settings = <IGuideSettingsContract>this.hours.data

        settings.sources = this.sources.allowedValues.map((v) => v.value)

        // Die Aktivierung der Aktualisierung wird über die Dauer gesteuert.
        if (!this.isActive.value) settings.duration = 0

        // Speicherung anfordern.
        return setGuideSettings(settings)
    }
}
