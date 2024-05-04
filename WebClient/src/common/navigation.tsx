import * as React from 'react'
import { InternalLink } from '../lib.react/command/internalLink'
import { IPage } from '../app/pages/page'
import { Component } from '../lib.react/reactUi'

// React.Js Komponente zur Anzeige der Navigationsleiste.
export class Navigation extends Component<IPage> {
    // Erstellt die Anzeige.
    render(): React.ReactNode {
        var page = this.props.uvm

        if (!page || !page.navigation) return null

        var application = page.application

        return (
            <div className='vcrnet-navigation vcrnet-bar'>
                {page.navigation.refresh && (
                    <InternalLink view={() => page.reload()} pict='refresh'>
                        Aktualisieren
                    </InternalLink>
                )}
                <InternalLink view={application.homePage.route} pict='home'>
                    Startseite
                </InternalLink>
                {page.navigation.favorites && (
                    <InternalLink view={application.favoritesPage.route} pict='fav'>
                        Favoriten
                    </InternalLink>
                )}
                {page.navigation.guide && (
                    <InternalLink view={application.guidePage.route} pict='guide'>
                        Programmzeitschrift
                    </InternalLink>
                )}
                {page.navigation.plan && (
                    <InternalLink view={application.planPage.route} pict='plan'>
                        Aufzeichnungsplan
                    </InternalLink>
                )}
                {page.navigation.new && (
                    <InternalLink view={application.editPage.route} pict='new'>
                        Neue Aufzeichnung
                    </InternalLink>
                )}
                {page.navigation.current && (
                    <InternalLink view={application.devicesPage.route} pict='devices'>
                        Geräte
                    </InternalLink>
                )}
            </div>
        )
    }
}
