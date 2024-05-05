import { IPage } from '../app/pages/page'
import { IPageFactory, Router } from '../lib.react/router'
import { Admin } from '../pages/admin'
import { Devices } from '../pages/devices'
import { Edit } from '../pages/edit'
import { Favorites } from '../pages/favorites'
import { Guide } from '../pages/guide'
import { Help } from '../pages/help'
import { Home } from '../pages/home'
import { Jobs } from '../pages/jobs'
import { Log } from '../pages/log'
import { Plan } from '../pages/plan'
import { Settings } from '../pages/settings'

// Konkretisierte React.Js Komponente zur Anzeige unterschiedlicher Navigationsbereiche.
export class View extends Router<IPage> {
    // Anmeldung der Navigationsbereiche für die Basisklasse.
    protected getPages(page: IPage): IPageFactory<IPage> {
        return {
            [page.application.logPage.route]: Log,
            [page.application.jobPage.route]: Jobs,
            [page.application.homePage.route]: Home,
            [page.application.planPage.route]: Plan,
            [page.application.editPage.route]: Edit,
            [page.application.helpPage.route]: Help,
            [page.application.guidePage.route]: Guide,
            [page.application.adminPage.route]: Admin,
            [page.application.devicesPage.route]: Devices,
            [page.application.settingsPage.route]: Settings,
            [page.application.favoritesPage.route]: Favorites,
        }
    }
}
