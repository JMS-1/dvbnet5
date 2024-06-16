import * as React from 'react'

import { IString } from '../../../lib/edit/text/text'
import { ComponentExWithSite, IComponent } from '../../reactUi'

// Konfiguration einer einfachen Texteingabe.
interface IEditText extends IComponent<IString> {
    // Die Anzahl der darzustellenden Zeichen.
    chars: number

    // Optional ein Platzhaltertext.
    hint?: string

    // Gesetzt, wenn es sich um die Eingabe eines Kennworts handelt.
    password?: boolean
}

// Texteingabe für React.Js - die NoUi Schicht stellt den Wert und das Prüfergebnis zur Verfügung.
export class EditText extends ComponentExWithSite<IString, IEditText> {
    // Erstellt die Anzeige der Komponente.
    render(): JSX.Element {
        return (
            <input
                className='jmslib-edittext jmslib-validatable'
                placeholder={this.props.hint}
                size={this.props.chars}
                title={this.props.uvm.message}
                type={this.props.password ? 'PASSWORD' : 'TEXT'}
                value={this.props.uvm.value ?? ''}
                onChange={(ev) => (this.props.uvm.value = (ev.target as HTMLInputElement).value)}
            />
        )
    }
}
