import { DevicesSection } from './admin/devices'
import { DirectoriesSection } from './admin/directories'
import { GuideSection } from './admin/guide'
import { OtherSection } from './admin/other'
import { RulesSection } from './admin/rules'
import { ScanSection } from './admin/scan'
import { ISection, ISectionInfo, ISectionInfoFactory, Section } from './admin/section'
import { SecuritySection } from './admin/security'
import { IPage, Page } from './page'

import { Command } from '../../lib/command/command'
import { DateTimeUtils } from '../../lib/dateTimeUtils'
import { IUiValue, IValueFromList, SingleListProperty, uiValue } from '../../lib/edit/list'
import { Application } from '../app'

// Interne Verwaltungseinheit für Konfigurationsbereiche.
class SectionInfo implements ISectionInfoFactory {
    // Meldet den eindeutigen Namen des Konfigurationsbereichs.
    get route(): string {
        return this._factory.route
    }

    // Erstellt eine neue Verwaltung.
    constructor(private readonly _factory: { new (section: AdminPage): Section; route: string }) {}

    // Die Präsentationsinstanz des zugehörigen Konfigurationsbereichs.
    private _section: Section

    // Meldet die Präsentation des zugehörigen Konfigurationsbereichs - bei Bedarf wird eine neue erstellt.
    getOrCreate(adminPage: AdminPage): Section | undefined {
        // Beim ersten Aufruf eine neue Präsentationsinstanz anlegen.
        if (!this._section) this._section = new this._factory(adminPage)

        return this._section
    }
}

// Schnittstelle zur Anzeige der Administration.
export interface IAdminPage extends IPage {
    // Eine Auswahl für den aktuell anzuzeigenden Konfigurationsbereich.
    readonly sections: IValueFromList<ISectionInfo>

    // Erstellt eine Instanz des aktuellen Konfigurationsbereichs.
    getOrCreateCurrentSection(): ISection | undefined
}

// Das Präsentationsmodell für die Konfiguration des VCR.NET Recording Service.
export class AdminPage extends Page implements IAdminPage {
    // Einmalig berechnet die Liste aller Stunden des Tages.
    static readonly hoursOfDay: IUiValue<number>[] = Array(24).map((_d: number, i: number) =>
        uiValue(i, DateTimeUtils.formatNumber(i))
    )

    // Die Liste aller Konfigurationsbereiche in der Reihenfolge, in der sie dem Anwender präsentiert werden sollen.
    private readonly _sections: IUiValue<SectionInfo>[] = [
        uiValue(new SectionInfo(SecuritySection), 'Sicherheit'),
        uiValue(new SectionInfo(DirectoriesSection), 'Verzeichnisse'),
        uiValue(new SectionInfo(DevicesSection), 'Geräte'),
        uiValue(new SectionInfo(GuideSection), 'Programmzeitschrift'),
        uiValue(new SectionInfo(ScanSection), 'Quellen'),
        uiValue(new SectionInfo(RulesSection), 'Planungsregeln'),
        uiValue(new SectionInfo(OtherSection), 'Sonstiges'),
    ]

    // Präsentationsmodell zur Auswahl des aktuellen Konfigurationsbereichs.
    readonly sections = new SingleListProperty({}, 'value', undefined, undefined, this._sections)

    // Erstellt ein neues Präsentationsmodell für die Seite.
    constructor(application: Application) {
        super('admin', application)
    }

    // Bereitet die Seite für die Anzeige vor.
    reset(sections: string[]): void {
        // Den aktuellen Konfigurationsbereich ermittelt - im Zweifel verwenden wir den ersten der Liste.
        const allSections = this.sections.allowedValues
        const curSection = allSections.filter((v) => v.value.route === sections[0])[0] || allSections[0]

        // Auswahl übernehmen.
        this.sections.value = curSection.value

        // Den aktiven Konfigurationsbereich laden.
        curSection.value.getOrCreate(this)?.reset()
    }

    // Aktualisiert eine Teilkonfiguration.
    update(request: Promise<boolean | undefined>, command: Command<void>): Promise<void> {
        // Auf das Ende der asynchronen Ausführung warten.
        return request.then(
            (restartRequired) => {
                if (restartRequired === true) this.application.restart()
                else if (restartRequired === false) this.application.gotoPage(null)
                else command.message = 'Ausführung zurzeit nicht möglich'
            },
            (error) => {
                // Fehlermeldung eintragen.
                command.message = error.message

                // Weitere Fehlerbehandlung ermöglichen.
                return error
            }
        )
    }

    // Erstellt eine Instanz des aktuellen Konfigurationsbereichs.
    getOrCreateCurrentSection(): ISection | undefined {
        return this.sections.value?.getOrCreate(this)
    }

    // Überschrift melden.
    get title(): string {
        return 'Administration und Konfiguration'
    }
}
