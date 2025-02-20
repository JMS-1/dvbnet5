﻿import * as React from 'react'

import { ILogEntry } from '../../app/pages/log/entry'
import { ExternalLink } from '../../lib.react/command/externalLink'
import { Pictogram } from '../../lib.react/command/pictogram'
import { Component } from '../../lib.react/reactUi'

// React.Js Komponente zur Anzeige der Details eines Protokolleintrags.
export class LogDetails extends Component<ILogEntry> {
    // Oberflächenelemente erstellen.
    render(): React.JSX.Element {
        return (
            <form className='vcrnet-logentrydetails'>
                <fieldset>
                    <legend>Detailinformationen</legend>
                    <table className='vcrnet-tableIsForm'>
                        <tbody>
                            <tr>
                                <td>Beginn:</td>
                                <td>{this.props.uvm.start}</td>
                            </tr>
                            <tr>
                                <td>Ende:</td>
                                <td>{this.props.uvm.end}</td>
                            </tr>
                            <tr>
                                <td>Quelle:</td>
                                <td>{this.props.uvm.source}</td>
                            </tr>
                            <tr>
                                <td>Größe:</td>
                                <td>{this.props.uvm.size}</td>
                            </tr>
                            <tr>
                                <td>Primäre Datei:</td>
                                <td>{this.props.uvm.primary}</td>
                            </tr>
                            {this.props.uvm.hasFiles && (
                                <tr>
                                    <td>Datei ansehen:</td>
                                    <td>
                                        {this.props.uvm.files.map((f, index) => (
                                            <ExternalLink key={index} sameWindow={true} url={f}>
                                                <Pictogram name='recording' />
                                            </ExternalLink>
                                        ))}
                                    </td>
                                </tr>
                            )}
                            {this.props.uvm.hasHashes && (
                                <tr>
                                    <td>Demux starten:</td>
                                    <td>
                                        {this.props.uvm.fileHashes.map((f, index) => (
                                            <ExternalLink key={index} sameWindow={true} url={f}>
                                                <Pictogram name='recording' />
                                            </ExternalLink>
                                        ))}
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </fieldset>
            </form>
        )
    }
}
