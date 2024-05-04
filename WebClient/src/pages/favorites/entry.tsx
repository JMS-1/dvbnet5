import * as React from 'react'
import { IFavorite } from '../../app/pages/favorites/entry'
import { InternalLink } from '../../lib.react/command/internalLink'
import { Pictogram } from '../../lib.react/command/pictogram'
import { ComponentWithSite } from '../../lib.react/reactUi'

// React.Js Komponente zur Anzeige und Pflege einer gespeicherten Suche.
export class Favorite extends ComponentWithSite<IFavorite> {
    // Oberflächenelemente anlegen.
    render(): JSX.Element {
        return (
            <tr className='vcrnet-favorite'>
                <td>
                    <InternalLink
                        description='In der Programmzeitschrift anzeigen'
                        view={() => this.props.uvm.show.execute()}
                    >
                        {this.props.uvm.count === null ? `-` : `${this.props.uvm.count}`}
                    </InternalLink>
                </td>
                <td>
                    {this.props.uvm.title} <Pictogram onClick={() => this.props.uvm.remove.execute()} name='delete' />
                </td>
            </tr>
        )
    }
}
