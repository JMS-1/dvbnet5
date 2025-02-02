import * as React from 'react'

import { ICommand } from '../../lib/command/command'
import { ComponentExWithSite, IComponent } from '../reactUi'

// Schnittstelle zur Anzeige einer Schaltfläche.
interface IButtonCommand extends IComponent<ICommand> {
    // Zusätzliche CSS Klassen.
    className?: string
}

// React.Js Komponente zur Anzeige einer Aktion über eine Schaltfläche.
export class ButtonCommand extends ComponentExWithSite<ICommand, IButtonCommand> {
    // Oberflächenelemente erzeugen.
    render(): React.JSX.Element | false {
        return (
            this.props.uvm.isVisible && (
                <a
                    className={`jmslib-command${this.props.className ? ` ${this.props.className}` : ''}${this.props.uvm.isDangerous ? ' jmslib-command-dangerous' : ''}${this.props.uvm.isEnabled ? '' : ' jmslib-command-disabled'}`}
                    title={this.props.uvm.message}
                    onClick={(ev) => this.props.uvm.execute()}
                >
                    {this.props.uvm.text}
                </a>
            )
        )
    }
}
