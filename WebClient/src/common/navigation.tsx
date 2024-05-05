import * as React from 'react'

import { IPage } from '../app/pages/page'
import { InternalLink } from '../lib.react/command/internalLink'
import { Component } from '../lib.react/reactUi'

// React.Js Komponente zur Anzeige der Navigationsleiste.
export class Navigation extends Component<IPage> {
    // Erstellt die Anzeige.
    render(): React.ReactNode {
        const page = this.props.uvm

        if (!page || !page.navigation) return null

        const application = page.application

        return (
            <div className='vcrnet-navigation vcrnet-bar'>
                {page.navigation.refresh && (
                    <InternalLink pict='refresh' view={() => page.reload()}>
                        Aktualisieren
                    </InternalLink>
                )}
                <InternalLink pict='home' view={application.homePage.route}>
                    Startseite
                </InternalLink>
                {page.navigation.favorites && (
                    <InternalLink pict='fav' view={application.favoritesPage.route}>
                        Favoriten
                    </InternalLink>
                )}
                {page.navigation.guide && (
                    <InternalLink pict='guide' view={application.guidePage.route}>
                        Programmzeitschrift
                    </InternalLink>
                )}
                {page.navigation.plan && (
                    <InternalLink pict='plan' view={application.planPage.route}>
                        Aufzeichnungsplan
                    </InternalLink>
                )}
                {page.navigation.new && (
                    <InternalLink pict='new' view={application.editPage.route}>
                        Neue Aufzeichnung
                    </InternalLink>
                )}
                {page.navigation.current && (
                    <InternalLink pict='devices' view={application.devicesPage.route}>
                        Geräte
                    </InternalLink>
                )}
            </div>
        )
    }
}
