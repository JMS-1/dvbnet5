import * as React from 'react'

import { IFlag } from '../../../lib/edit/boolean/flag'
import { ComponentWithSite } from '../../reactUi'

// React.Js Komponente zur visuellen Pflege eines Wahrheitswertes.
export class EditBoolean extends ComponentWithSite<IFlag> {
    // Erstellt die Anzeige der Komponente.
    render(): JSX.Element {
        return (
            <label className='jmslib-editflag jmslib-validatable' title={this.props.uvm.message}>
                <input
                    checked={!!this.props.uvm.value}
                    disabled={this.props.uvm.isReadonly}
                    type='CHECKBOX'
                    onChange={(ev) => (this.props.uvm.value = (ev.target as HTMLInputElement).checked)}
                />
                {this.props.uvm.text}
            </label>
        )
    }
}
