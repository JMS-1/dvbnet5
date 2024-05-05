/* eslint-disable @typescript-eslint/no-non-null-assertion */

import * as React from 'react'

import { IMultiValueFromList } from '../../lib/edit/multiList'
import { ButtonCommand } from '../command/button'
import { ComponentExWithSite, IComponent } from '../reactUi'

// Schnittstelle zur Anzeige einer Liste von Schaltflächen.
interface IMultiSelectButton extends IComponent<IMultiValueFromList<unknown>> {
    // Gesetzt, wenn die Schaltflächen nicht separiert werden sollen.
    merge?: boolean
}

// React.Js Komponente für eine Mehrfachauswahl über einzelne Schaltflächen.
export class MultiSelectButton extends ComponentExWithSite<IMultiValueFromList<unknown>, IMultiSelectButton> {
    // Oberflächenelemente erstellen.
    render(): JSX.Element {
        return (
            <div className={`jmslib-editmultibuttonlist${this.props.merge ? ' jmslib-command-mergelist' : ''}`}>
                {this.props.uvm.allowedValues.map((v) => (
                    <ButtonCommand
                        key={v.display}
                        className={v.isSelected ? 'jmslib-command-checked' : ''}
                        uvm={v.toggle!}
                    />
                ))}
            </div>
        )
    }
}
