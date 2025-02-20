﻿import { IDeviceInfo, Info } from './devices/entry'
import { IPage, Page } from './page'

import { getPlanCurrent } from '../../web/IPlanCurrentContract'
import { Application } from '../app'

// Schnittstelle zur Anzeige der aktuellen Aktivitäten.
export interface IDevicesPage extends IPage {
    // Alle aktuellen Aktivitäten.
    readonly infos: IDeviceInfo[]
}

// Präsentationsmodell zur Anzeige und Manipulation der aktuellen Aktivitäten.
export class DevicesPage extends Page implements IDevicesPage {
    // Alle Aktivitäten.
    infos: Info[] = []

    // Erstellt ein neues Präsentationsmodell.
    constructor(application: Application) {
        super('current', application)

        // Der Anwender kann die Ansicht aktualisieren.
        this.navigation.refresh = true
    }

    // Beginnt mit der Anzeige der Ansicht.
    reset(sections: string[]): void {
        // Zurücksetzen
        this._refreshing = false

        // Liste anfordern.
        this.reload()
    }

    // Gesetzt während sich das Präsentationsmodell aktualisiert.
    private _refreshing = false

    // Fordert die Aktivitäten vom VCR.NET Recording Service neu an.
    reload(): void {
        getPlanCurrent().then((plan) => {
            // Aktionen des Anwenders einmal binden.
            const similiar = this.application.guidePage.findInGuide.bind(this.application.guidePage)
            const refresh = this.toggleDetails.bind(this)
            const reload = this.reload.bind(this)

            // Die aktuellen Aktivitäten umwandeln.
            this.infos = (plan || []).map((info) => new Info(info, refresh, reload, similiar))

            // Anwendung kann nun bedient werden.
            this.application.isBusy = false

            // Anzeige zur Aktualisierung auffordern.
            this.refreshUi()
        })
    }

    // Schaltet die Detailanzeige einer Aktivität um.
    private toggleDetails(info: Info, guide: boolean): void {
        // Das machen wir gerade schon.
        if (this._refreshing) return

        // Wir müssen hier Rekursionen vermeiden.
        this._refreshing = true

        // Aktuellen Stand auslesen.
        const flag = guide ? info.showGuide : info.showControl
        const state = flag.value

        // Alle anderen Detailansichten schliessen.
        this.infos.forEach((i) => (i.showControl.value = i.showGuide.value = false))

        // Neuen Stand übernehmen.
        flag.value = state

        // Wir können nun wieder normal arbeiten.
        this._refreshing = false

        // Oberfläche zur Aktualisierung auffordern.
        this.refreshUi()
    }

    // Die Überschreibt für die Ansicht des Präsentationsmodells.
    get title(): string {
        return 'Geräteübersicht'
    }
}
