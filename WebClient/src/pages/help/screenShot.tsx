import * as React from 'react'
import { IPictogram, Pictogram } from '../../lib.react/command/pictogram'
import { IEmpty } from '../../lib.react/reactUi'

// React.Js Komponente für ein Bild in der Hilfe.
export class ScreenShot extends React.Component<IPictogram, IEmpty> {
    // Erzeugt die Oberflächenelemente.
    render(): JSX.Element {
        return (
            <span className='vcrnet-screenshot'>
                <Pictogram {...this.props} />
            </span>
        )
    }
}
