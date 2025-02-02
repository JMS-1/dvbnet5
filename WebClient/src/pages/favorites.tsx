import * as React from 'react'

import { Favorite } from './favorites/entry'

import { IFavoritesPage } from '../app/pages/favorites'
import { InlineHelp } from '../common/inlineHelp'
import { Pictogram } from '../lib.react/command/pictogram'
import { SingleSelectButton } from '../lib.react/edit/buttonList'
import { ComponentWithSite } from '../lib.react/reactUi'

// React.Js Komponente zur Anzeige der gespeicherten Suchen.
export class Favorites extends ComponentWithSite<IFavoritesPage> {
    // Oberflächenelemente anzeigen.
    render(): React.JSX.Element {
        return (
            <div className='vcrnet-favorites'>
                {this.getHelp()}
                <SingleSelectButton merge={true} uvm={this.props.uvm.onlyWithCount} />
                <table className='vcrnet-table'>
                    <thead>
                        <tr>
                            <td>Sendungen</td>
                            <td>Suchbedingung</td>
                        </tr>
                    </thead>
                    <tbody>
                        {this.props.uvm.favorites.map((f, index) => (
                            <Favorite key={index} uvm={f} />
                        ))}
                    </tbody>
                </table>
            </div>
        )
    }

    // Erlaäuterungen anzeigen.
    private getHelp(): React.JSX.Element {
        return (
            <InlineHelp title='Erläuterungen zur Bedienung'>
                Die Auswertung der passenden Sendungen erfolgt pro Favorit einmalig und wird verzögert im Hintergrund
                ausgeführt. Solange diese Berechnung noch nicht abgeschlossen ist, wird als Anzahl der Sendungen in der
                ersten Spalte ein Bindestrich dargestellt. Unabhängig davon ist es durch Auswahl der Anzahl jederzeit
                möglich, den Favoriten als Suche in der Programmzeitschrift anzuzeigen.
                <br />
                <br />
                Ein Favorit kann durch das Symbol <Pictogram name='delete' /> rechts neben der Beschreibung der
                Suchbedingung jederzeit gelöscht werden. Dieser Vorgang wird unmittelbar und unwiederbringlich ohne
                weitere Rückfrage ausgeführt.
            </InlineHelp>
        )
    }
}
