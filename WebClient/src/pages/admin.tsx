import * as React from 'react'

import { AdminDevices } from './admin/devices'
import { AdminDirectories } from './admin/directories'
import { AdminGuide } from './admin/guide'
import { AdminOther } from './admin/other'
import { AdminRules } from './admin/rules'
import { AdminSection } from './admin/section'
import { AdminSmtp } from './admin/smtp'
import { AdminSources } from './admin/sources'

import { AdminPage, IAdminPage } from '../app/pages/admin'
import { ISection } from '../app/pages/admin/section'
import { InternalLink } from '../lib.react/command/internalLink'
import { ComponentWithSite, IComponent, IEmpty } from '../lib.react/reactUi'

// Hilfsschnittstelle zur Signature des Konstruktors eines Ui View Models.
export interface IAdminSectionFactory<TSectionType extends ISection> {
    // Der eigentliche Konstruktor.
    new (page: AdminPage): ISection

    // Die statische Eigenschaft mit dem eindeutigen Namen.
    route: string
}

// Schnittstelle zum anlegen der React.Js Komponente für einen einzelnen Konfigurationsbereich.
interface IAdminUiSectionFactory<TSectionType extends ISection> {
    // Der eigentliche Konstruktor.
    new (props?: IComponent<TSectionType>, context?: IEmpty): AdminSection<TSectionType>

    // Das zugehörige Ui View Model.
    readonly uvm: IAdminSectionFactory<TSectionType>
}

// Nachschlageliste für die React.Js Komponenten der Konfigurationsbereiche.
interface IAdminSectionFactoryMap {
    // Ermittelt einen Konfigurationsbereich.
    [route: string]: IAdminUiSectionFactory<ISection>
}

// React.Js Komponente zur Anzeige der Konfiguration.
export class Admin extends ComponentWithSite<IAdminPage> {
    // Alle bekannten Konfigurationsbereiche.
    static _sections: IAdminSectionFactoryMap

    // Einen einzelnen Konfigurationsbereich ergänzen.
    static addSection<TSectionType extends ISection>(factory: IAdminUiSectionFactory<TSectionType>): void {
        Admin._sections[factory.uvm.route] = factory
    }

    // Oberflächenelemente erstellen.
    render(): JSX.Element {
        const section = this.props.uvm.sections.value

        return (
            <div className='vcrnet-admin'>
                <div>
                    {this.props.uvm.sections.allowedValues.map((si) => (
                        <div key={si.display} className={`${si.value === section ? 'jmslib-command-checked' : ''}`}>
                            <InternalLink view={`${this.props.uvm.route};${si.value.route}`}>{si.display}</InternalLink>
                        </div>
                    ))}
                </div>
                <div>{this.renderSection()}</div>
            </div>
        )
    }

    // React.Js Komponente zum aktuellen Konfigurationsbereich ermitteln.
    private renderSection(): React.ReactNode {
        // Einmalig erzeugen.
        if (!Admin._sections) {
            // Leer anlegen.
            Admin._sections = {}

            // Alle unterstützten Seiten anlegen.
            Admin.addSection(AdminDevices)
            Admin.addSection(AdminDirectories)
            Admin.addSection(AdminGuide)
            Admin.addSection(AdminSources)
            Admin.addSection(AdminRules)
            Admin.addSection(AdminSmtp)
            Admin.addSection(AdminOther)
        }

        // Oberlfächenkomponente ermitteln.
        const route = this.props.uvm.sections.value?.route
        const factory = route && Admin._sections[route]

        // Ui View Model ermitteln undReact.Js Komponente erstellen.
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        return factory && React.createElement(factory as any, { uvm: this.props.uvm.getOrCreateCurrentSection() })
    }
}
