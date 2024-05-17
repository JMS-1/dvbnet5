import * as React from 'react'

import { HelpComponent } from './help/helpComponent'

import { IHelpPage } from '../app/pages/help'
import { Component } from '../lib.react/reactUi'

// React.Js Komponente zur Anzeige der Hilfeseite.
export class Help extends Component<IHelpPage> {
    // Erstellt die Anzeigeelemente der Oberfläche.
    render(): JSX.Element {
        // Ermittelt die Anzeige des gewählten Aspektes.
        const element = this.props.uvm.getHelpComponent<HelpComponent>()

        return <div className='vcrnet-faq'>{element?.render(this.props.uvm)}</div>
    }
}
