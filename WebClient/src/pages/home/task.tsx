import * as React from 'react'
import { ButtonCommand } from '../../lib.react/command/button'
import { ComponentWithSite } from '../../lib.react/reactUi'
import { ICommand } from '../../lib/command/command'

// React.Js Komponente zum manuellen Starten einer Sonderaufgabe.
export class Task extends ComponentWithSite<ICommand> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        return (
            <li className='vcrnet-home-task'>
                <fieldset>
                    {this.props.children}
                    <div>
                        <ButtonCommand uvm={this.props.uvm} />
                    </div>
                </fieldset>
            </li>
        )
    }
}
