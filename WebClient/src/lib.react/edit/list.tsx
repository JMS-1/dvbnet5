﻿import * as React from 'react'

import { IValueFromList } from '../../lib/edit/list'
import { ComponentWithSite } from '../reactUi'

// Verwaltete eine Zeichenkette, die über eine einfache Auswahlliste festgelegt wird.
export class SingleSelect extends ComponentWithSite<IValueFromList<unknown>> {
    // Erstellt die Anzeige für die Komponente.
    render(): React.JSX.Element {
        return (
            <select
                className='jmslib-editlist jmslib-validatable'
                title={this.props.uvm.message}
                value={`${this.props.uvm.valueIndex}`}
                onChange={(ev) => (this.props.uvm.valueIndex = parseInt((ev.target as HTMLSelectElement).value))}
            >
                {this.props.uvm.allowedValues.map((av, index) => (
                    <option key={index} value={`${index}`}>
                        {av.display}
                    </option>
                ))}
            </select>
        )
    }
}
