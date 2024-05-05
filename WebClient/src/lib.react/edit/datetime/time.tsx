import * as React from 'react'

import { ITime } from '../../../lib/edit/datetime/time'
import { ComponentWithSite } from '../../reactUi'

// React.Js Komponente zur Eingabe einer Uhrzeit.
export class EditTime extends ComponentWithSite<ITime> {
    // Erstellt die Oberflächenelemente.
    render(): JSX.Element {
        return (
            <input
                className='jmslib-edittime jmslib-validatable'
                size={5}
                title={this.props.uvm.message}
                type='TEXT'
                value={this.props.uvm.rawValue}
                onChange={(ev) => (this.props.uvm.rawValue = (ev.target as HTMLInputElement).value)}
            />
        )
    }
}
