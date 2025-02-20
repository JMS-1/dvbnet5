﻿import * as React from 'react'

import { IEmpty } from './reactUi'

// Blendet in einer Tabelle eine Sonderzeile mit Detailinformationen ein.
interface IDetailRow {
    // Optional die Anzahl der Spalten vor der Detailinformation.
    prefixColumns?: number

    // Die Anzahl der Spalten, die für die Anzeige der Detailinformationen zusammengefasst werden sollen.
    dataColumns: number

    // Kindelemente.
    children?: React.ReactNode
}

// React.Js Komponente zur Anzeige einer Detailzeile.
export class DetailRow extends React.Component<IDetailRow, IEmpty> {
    // Oberflächenelemente anlegen.
    render(): React.JSX.Element {
        return (
            <tr className='jmslib-details'>
                {this.props.prefixColumns && <td colSpan={this.props.prefixColumns}>&nbsp;</td>}
                <td colSpan={this.props.dataColumns}>{this.props.children}</td>
            </tr>
        )
    }
}
