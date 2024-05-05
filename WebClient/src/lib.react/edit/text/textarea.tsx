import * as React from 'react'

import { IString } from '../../../lib/edit/text/text'
import { ComponentExWithSite, IComponent } from '../../reactUi'

// Konfiguration zur Eingabe eines langen Textes.
interface IEditTextArea extends IComponent<IString> {
    // Zeilen zur Eingabe.
    rows: number

    // Spalten zur Eingabe.
    columns: number
}

// React.Js Komponente zur Eingabe eines langen Textes.
export class EditTextArea extends ComponentExWithSite<IString, IEditTextArea> {
    // Oberflächenelemente erstellen.
    render(): JSX.Element {
        return (
            <textarea
                className='jmslib-edittextarea'
                cols={this.props.columns}
                rows={this.props.rows}
                title={this.props.uvm.message}
                value={this.props.uvm.value ?? ''}
                onChange={(ev) => (this.props.uvm.value = (ev.target as HTMLTextAreaElement).value)}
            />
        )
    }
}
