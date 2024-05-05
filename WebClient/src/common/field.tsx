import * as React from 'react'

import { HelpLink } from './helpLink'

import { IPage } from '../app/pages/page'
import { IEmpty } from '../lib.react/reactUi'

// Die Konfiguration eines Eingabefeldes.
interface IFieldStatic {
    // Die übergeordnete Seite.
    page: IPage

    // Der Anzeigename des Feldes.
    label: string

    // Optional der Verweise auf eine Hilfeseite.
    help?: string

    // Kindelemente.
    children?: React.ReactNode
}

// Beschreibt ein Eingabefeld.
export class Field extends React.Component<IFieldStatic, IEmpty> {
    // Erzeugt die Anzeige eines Eingabefeldes.
    render(): JSX.Element {
        return (
            <div className='vcrnet-editfield'>
                <div>
                    {this.props.label}
                    {this.props.help && <HelpLink page={this.props.page} topic={this.props.help} />}
                </div>
                <div>{this.props.children}</div>
            </div>
        )
    }
}
