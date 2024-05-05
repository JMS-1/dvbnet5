import { AdminPage, IAdminPage } from './pages/admin'
import { DevicesPage, IDevicesPage } from './pages/devices'
import { EditPage, IEditPage } from './pages/edit'
import { FavoritesPage, IFavoritesPage } from './pages/favorites'
import { GuidePage, IGuidePage } from './pages/guide'
import { HelpPage, IHelpComponent, IHelpComponentProvider, IHelpPage } from './pages/help'
import { HomePage, IHomePage } from './pages/home'
import { IJobPage, JobPage } from './pages/jobs'
import { ILogPage, LogPage } from './pages/log'
import { IPage, Page } from './pages/page'
import { IPlanPage, PlanPage } from './pages/plan'
import { ISettingsPage, SettingsPage } from './pages/settings'

import { switchView } from '../lib/http/config'
import { IView } from '../lib/site'
import { getServerVersion, IInfoServiceContract } from '../web/IInfoServiceContract'
import { getUserProfile, IUserProfileContract } from '../web/IUserProfileContract'

// Schnittstelle der Anwendung.
export interface IApplication {
    // Die Überschrift der Anwendung als Ganzes.
    readonly title: string

    // Der aktuell verwendete Navigationsbereich.
    readonly page?: IPage | null

    // Getzt solange einer neuer Navigationsbereich initialisiert wird.
    readonly isBusy: boolean

    // Gesetzt während der VCR.NET Recording Service neu startet.
    readonly isRestarting: boolean

    // Das Präsentationsmodell der Einstiegsseite.
    readonly homePage: IHomePage

    // Das Präsentationsmodell der Hilfeseiten.
    readonly helpPage: IHelpPage

    // Das Präsentationsmodell des Aufzeichnungsplans.
    readonly planPage: IPlanPage

    // Das Präsentationsmodell der Pflegeseite für eine Aufzeichnung.
    readonly editPage: IEditPage

    // Das Präsentationsmodell der Programmzeitschrift.
    readonly guidePage: IGuidePage

    // Das Präsentationsmodell der Aufzeichnungsübersicht.
    readonly jobPage: IJobPage

    // Das Präsentationsmodell der Protokollansicht.
    readonly logPage: ILogPage

    // Das Präsentationsmodell für die Konfiguration.
    readonly adminPage: IAdminPage

    // Das Präsentationsmodell für die Einstellungen des Benutzers.
    readonly settingsPage: ISettingsPage

    // Das Präsentationsmodell für die gespeicherten Suchen.
    readonly favoritesPage: IFavoritesPage

    // Das Präsentationsmodell der Geräteübersicht.
    readonly devicesPage: IDevicesPage

    // Einstellungen des Benutzers.
    readonly profile: IUserProfileContract

    // Meldet die Verwaltung der Hilfeseiten - dies erfolgt primär im Kontext der Oberfläche.
    getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType>

    // Wechselt den Navigationsbereich.
    switchPage(name: string, sections?: string[]): void
}

// Die von der Oberfläche bereitzustellende Arbeitsumgebung für die Anwendung.
export interface IApplicationSite extends IView {
    // Wechselt zu einem anderen Navigationsbereich.
    goto(page: string | null): void

    // Meldet die Verwaltung der Hilfeseiten.
    getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType>
}

// Das Präsentationsmodell der Anwendung.
export class Application implements IApplication {
    // Das Präsentationsmodell der Einstiegsseite.
    readonly homePage: HomePage

    // Das Präsentationsmodell der Hilfeseiten.
    readonly helpPage: HelpPage

    // Das Präsentationsmodell des Aufzeichnungsplans.
    readonly planPage: PlanPage

    // Das Präsentationsmodell der Pflegeseite für eine Aufzeichnung.
    readonly editPage: EditPage

    // Das Präsentationsmodell der Programmzeitschrift.
    readonly guidePage: GuidePage

    // Das Präsentationsmodell der Aufzeichnungsübersicht.
    readonly jobPage: JobPage

    // Das Präsentationsmodell der Protokollansicht.
    readonly logPage: LogPage

    // Das Präsentationsmodell für die Konfiguration.
    readonly adminPage: AdminPage

    // Das Präsentationsmodell für die Einstellungen des Benutzers.
    readonly settingsPage: SettingsPage

    // Das Präsentationsmodell für die gespeicherten Suchen.
    readonly favoritesPage: FavoritesPage

    // Das Präsentationsmodell der Geräteübersicht.
    readonly devicesPage: DevicesPage

    // Die in der Anwendung bereitgestellten Navigationsbereiche.
    private _pageMapper: { [name: string]: Page } = {}

    // Gesetzt wenn der Dienst gerade neu startet.
    isRestarting = false

    // Version des VCR.NET Recording Service.
    version: IInfoServiceContract

    // Einstellungen des Benutzers.
    profile: IUserProfileContract

    // Der aktuelle Navigationsbereich.
    page?: IPage | null

    // Der interne Arbeitsstand der Anwendung.
    private _pendingPage?: IPage

    get isBusy(): boolean {
        return !!this._pendingPage
    }

    set isBusy(isBusy: boolean) {
        // Das geht nur intern!
        if (isBusy) throw 'isBusy darf nur intern gesetzt werden'

        // Keine echte Änderung.
        if (isBusy === this.isBusy) return

        // Zustand vermerken.
        this.page = this._pendingPage

        this._pendingPage = undefined

        // Oberfläche zur Aktualisierung auffordern.
        this.refreshUi()
    }

    // Erstellt ein neues Präsentationsmodell für die Anwendung.
    constructor(private _site: IApplicationSite) {
        // Navigationsbereiche einmalig anlegen - das ist hier am einfachsten in der Handhabe.
        this.adminPage = this.addPage(AdminPage)
        this.devicesPage = this.addPage(DevicesPage)
        this.editPage = this.addPage(EditPage)
        this.favoritesPage = this.addPage(FavoritesPage)
        this.guidePage = this.addPage(GuidePage)
        this.helpPage = this.addPage(HelpPage)
        this.homePage = this.addPage(HomePage)
        this.jobPage = this.addPage(JobPage)
        this.logPage = this.addPage(LogPage)
        this.planPage = this.addPage(PlanPage)
        this.settingsPage = this.addPage(SettingsPage)
    }

    // Erstellt einen Navigationsbereich und vermerkt ihn dann einmalig.
    private addPage<TPageType extends Page>(factory: { new (application: Application): TPageType }): TPageType {
        // Konkretes Präsentationmodell für den Bereich anlegen.
        const page = new factory(this)

        // Neue Instanz vermerken und melden.
        this._pageMapper[page.route] = page

        return page
    }

    // Den Navigationsbereich wechseln.
    gotoPage(name: string | null): void {
        // Tatsächlich macht das die Anwendung.
        this._site.goto(name)
    }

    switchPage(name: string, sections: string[]): void {
        // Melden, dass alle ausstehenden asynchronen Anfragen von nun an nicht mehr interessieren.
        switchView()

        // Den Singleton der gewünschten Seite ermitteln.
        const page = this._pageMapper[name] || this.homePage

        // Aktivieren.
        this._pendingPage = page

        // Anzeige aktualisieren lassen.
        this.refreshUi()

        // Benutzereinstellungen anfordern.
        getUserProfile()
            .then((profile) => {
                // Benutzereinstellungen übernehmen.
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                this.profile = profile!

                // Versionsinformationen anfordern.
                return getServerVersion()
            })
            .then((info) => {
                // Versionsinformationen aktualisieren.
                // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
                this.version = info!

                // Navigationsbereich starten.
                page.reset(sections || [])
            })
    }

    // Oberfläche zur Aktualisierung auffordern.
    private refreshUi(): void {
        if (this._site) this._site.refreshUi()
    }

    // Name der Anwendung für den Browser ermitteln.
    get title(): string {
        const title = 'VCR.NET Recording Service'

        // Nach Möglichkeit die Versionsinformationen einmischen.
        const version = this.version

        if (version) return `${title} ${version.version}`
        else return title
    }

    // Oberfläche nach der Verwaltung der Hilfeseiten fragen.
    getHelpComponentProvider<TComponentType extends IHelpComponent>(): IHelpComponentProvider<TComponentType> {
        return this._site && this._site.getHelpComponentProvider<TComponentType>()
    }

    // Der Dienst wird neu gestartet.
    restart(): void {
        // Zustand vermerken.
        this._pendingPage = undefined
        this.isRestarting = true
        this.page = null

        // Ein wenig warten - das Intervall istrein willkürlich.
        setTimeout(() => {
            // Zustand zurücksetzen.
            this.isRestarting = false

            // Einstiegsseite anfordern.
            this.gotoPage(null)

            // Oberfläche zur Aktualisierung auffordern.
            this.refreshUi()
        }, 10000)

        // Oberfläche zur Aktualisierung auffordern.
        this.refreshUi()
    }
}
