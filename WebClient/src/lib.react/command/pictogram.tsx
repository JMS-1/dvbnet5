import * as React from 'react'

import { IEmpty } from '../reactUi'

// Konfiguration zur Anzeige eines Symbols.
export interface IPictogram {
    // Der Name einer Symboldatei.
    name: string

    // Eine alternative Beschreibung für das Symbol.
    description?: string

    // Aktion bei Auswahl des Symbols.
    onClick?(ev: React.MouseEvent<HTMLImageElement>): void
}

// React.Js Komponente zur Anzeige eines Symbols.
export class Pictogram extends React.Component<IPictogram, IEmpty> {
    // Globale Festlegung für das Verzeichnis aller Symboldateien.
    static imageRoot: string

    // Erstellt die Oberflächenelemente.
    render(): JSX.Element {
        return (
            <img
                alt={this.props.description}
                className='jmslib-pict'
                src={`${Pictogram.imageRoot}${this.props.name}.png`}
                onClick={this.props.onClick}
            />
        )
    }
}
