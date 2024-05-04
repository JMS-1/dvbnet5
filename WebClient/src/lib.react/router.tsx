import * as React from 'react'
import { Component, IComponent, IEmpty } from './reactUi'

// Schnittstelle eines Präsentationsmodell, das als Ziel einer Navigation dienen kann.
export interface IRoutablePage {
    // Der eindeutige Name des Navigationsziels.
    route: string
}

// Schnittstelle zum Erzeugen einer React.Js Komponente für ein Navigationsziel.
export interface IPageFactory<TPageType extends IRoutablePage> {
    // Ermittelt zu dem Präsentationsmodell Navigationsziel eine geeignet React.Js Komponente.
    [route: string]: {
        new (props?: IComponent<any>, context?: IEmpty): Component<TPageType>
    }
}

// Basisklasse zur Implementierung eines Navigationsverteilers.
export abstract class Router<TPageType extends IRoutablePage> extends Component<TPageType> {
    // Alle bekannten Navigationsziele und deren React.Js Komponentenerzeuger.
    private static _pages: IPageFactory<any>

    protected abstract getPages(page: TPageType): IPageFactory<TPageType>

    // Erstellt die Oberflächenelemente.
    render(): JSX.Element {
        // Die Navigationszeiele werden einmalig und vor allem nicht statisch zur Laufzeit geladen - das vermeidet unnötige Abhängigkeiten.
        if (!Router._pages) if (this.props.uvm) Router._pages = this.getPages(this.props.uvm)

        // Erzeuger für das aktuelle Navigationsziel ermitteln.
        var factory = Router._pages && Router._pages[this.props.uvm.route]

        // React.Js Komponente für das Navigationsziel anlegen.
        return (
            <div className='jmslib-router'>
                {factory && React.createElement(factory as any, { uvm: this.props.uvm })}
            </div>
        )
    }
}
