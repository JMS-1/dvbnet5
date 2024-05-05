import * as React from 'react'

import { INumber } from '../../../lib/edit/number/number'
import { ComponentExWithSite, IComponent } from '../../reactUi'

// Konfiguration der Anzeige einer Zahl.
interface IEditNumber extends IComponent<INumber> {
    // Die Anzahl der Zeichen im Texteingabefeld für die Zahl.
    chars: number
}

// React.Js Komponente zur Eingabe einer Zahl über ein Textfeld.
export class EditNumber extends ComponentExWithSite<INumber, IEditNumber> {
    // Oberflächenelemente erzeugen.
    render(): JSX.Element {
        return (
            <input
                className='jmslib-editnumber jmslib-validatable'
                size={this.props.chars}
                title={this.props.uvm.message}
                type='TEXT'
                value={this.props.uvm.rawValue}
                onChange={(ev) => (this.props.uvm.rawValue = (ev.target as HTMLInputElement).value)}
            />
        )
    }
}
