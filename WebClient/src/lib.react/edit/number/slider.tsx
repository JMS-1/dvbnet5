﻿import * as React from 'react'

import { INumberWithSlider } from '../../../lib/edit/number/slider'
import { ComponentWithSite } from '../../reactUi'

// React.Js Komponente zur Auswahl einer Zahl über einen Schieberegler.
export class EditNumberSlider extends ComponentWithSite<INumberWithSlider> {
    // Erstellt die Oberflächenelement - hier gibt es eine ganze Menge Kleinigkeiten im CSS zu beachten, esp. die Positionierung.
    render(): React.JSX.Element {
        return (
            <div className='jmslib-slider'>
                <div></div>
                <div>
                    <div
                        className={this.props.uvm.isDragging ? 'jmslib-slider-selected' : undefined}
                        style={{ left: `${100 * this.props.uvm.position}%` }}
                    ></div>
                </div>
                <div
                    draggable={false}
                    tabIndex={0}
                    onDragStart={() => false}
                    onKeyUp={(ev) => this.doKey(ev)}
                    onMouseDown={(ev) => (this.props.uvm.isDragging = ev.buttons === 1)}
                    onMouseLeave={(ev) => (this.props.uvm.isDragging = false)}
                    onMouseMove={(ev) => this.doMove(ev)}
                    onMouseUp={(ev) => (this.props.uvm.isDragging = false)}
                ></div>
            </div>
        )
    }

    // Überwacht Bewegungen mit der Maus und gibe diese an die Anwendungslogik weiter.
    private doMove(ev: React.MouseEvent<HTMLDivElement>): void {
        // Zurzeit sind Änderungen nicht gestattet.
        if (!this.props.uvm.isDragging) return

        // Der äußere Bereich des Reglers.
        const bounds = (ev.target as HTMLDivElement).getBoundingClientRect()

        // Die relative horizontale Mausposition.
        const absX = ev.clientX
        const relX = absX - bounds.left

        // Als relativen Wert zwischen 0 und 1 an die Anwendungslogik melden - hier ist noch ein Bug mit einem leichten Offset!
        this.props.uvm.position = relX / bounds.width
    }

    // Zur Feinsteuerung setzen wir auch die Pfeiltasten nach links und rechts um.
    private doKey(ev: React.KeyboardEvent<HTMLDivElement>): void {
        if (ev.keyCode === 37) this.props.uvm.delta(-1)
        else if (ev.keyCode === 39) this.props.uvm.delta(+1)
    }
}
