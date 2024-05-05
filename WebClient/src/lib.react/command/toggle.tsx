import * as React from 'react'

import { ButtonCommand } from './button'

import { IToggableFlag } from '../../lib/edit/boolean/flag'
import { Component } from '../reactUi'

// React.Js Komponente zur Pflege eines Wahrheitswertes über eine Schaltfläche.
export class ToggleCommand extends Component<IToggableFlag> {
    // Erzeugt die Oberflächenelemente.
    render(): JSX.Element {
        return (
            <ButtonCommand
                className={`jmslib-toggle${this.props.uvm.value ? ' jmslib-command-checked' : ''}`}
                uvm={this.props.uvm.toggle}
            />
        )
    }
}
