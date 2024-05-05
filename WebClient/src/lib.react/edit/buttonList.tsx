/* eslint-disable @typescript-eslint/no-non-null-assertion */

import * as React from 'react'

import { IValueFromList } from '../../lib/edit/list'
import { ButtonCommand } from '../command/button'
import { ComponentExWithSite, IComponent } from '../reactUi'

// Schnittstelle zur Anzeige einer Liste von Schaltflächen.
interface ISingleSelectButton extends IComponent<IValueFromList<unknown>> {
    // Gesetzt, wenn die Schaltflächen nicht separiert werden sollen.
    merge?: boolean
}

// React.Js Komponente zur Anzeige einer Liste von Schaltflächen.
export class SingleSelectButton extends ComponentExWithSite<IValueFromList<unknown>, ISingleSelectButton> {
    // Erstellt die Anzeige für die Komponente.
    render(): JSX.Element {
        const value = this.props.uvm.value

        return (
            <div className={`jmslib-editbuttonlist${this.props.merge ? ' jmslib-command-mergelist' : ''}`}>
                {this.props.uvm.allowedValues.map((av) => (
                    <ButtonCommand
                        key={av.display}
                        className={av.isSelected ? 'jmslib-command-checked' : ''}
                        uvm={av.select!}
                    />
                ))}
            </div>
        )
    }
}
