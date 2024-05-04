import * as React from 'react'
import { IString } from '../../../lib/edit/text/text'
import { IComponent, ComponentExWithSite } from '../../reactUi'

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
                rows={this.props.rows}
                cols={this.props.columns}
                value={this.props.uvm.value ?? ''}
                title={this.props.uvm.message}
                className='jmslib-edittextarea'
                onChange={(ev) => (this.props.uvm.value = (ev.target as HTMLTextAreaElement).value)}
            />
        )
    }
}
