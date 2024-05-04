import * as React from 'react'
import { Component } from '../lib.react/reactUi'
import { IHelpPage } from '../app/pages/help'
import { HelpComponent } from './help/helpComponent'

// React.Js Komponente zur Anzeige der Hilfeseite.
export class Help extends Component<IHelpPage> {
    // Erstellt die Anzeigeelemente der Oberfläche.
    render(): JSX.Element {
        // Ermittelt die Anzeige des gewählten Aspektes.
        var element = this.props.uvm.getHelpComponent<HelpComponent>()

        return <div className='vcrnet-faq'>{element && element.render(this.props.uvm)}</div>
    }
}
