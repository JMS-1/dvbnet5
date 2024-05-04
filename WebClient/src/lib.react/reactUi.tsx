import * as React from 'react'

import { IConnectable, IView } from '../lib/site'

// Beschreibt eine React.Js Komponente für ein Präsentationsmodell.
export interface IComponent<TViewModelType> {
    // Das Präsentationsmodell (Ui View Model).
    uvm: TViewModelType

    // Kindelemente.
    children?: React.ReactNode
}

// Beschreibt einen nicht vorhandenen Zustand einer React.Js Komponente.
export interface IEmpty {}

// Implementierung einer React.Js Komponente für ein Präsentationsmodell.
export abstract class ComponentEx<
    TViewModelType,
    TConfigType extends IComponent<TViewModelType>,
> extends React.Component<TConfigType, IEmpty> {}

export abstract class Component<TViewModelType> extends ComponentEx<TViewModelType, IComponent<TViewModelType>> {}

// Implementierung einer React.Js Komponente für ein Präsentationsmodell mit Benachrichtigungen.
export abstract class ComponentExWithSite<
        TViewModelType extends IConnectable,
        TConfigType extends IComponent<TViewModelType>,
    >
    extends ComponentEx<TViewModelType, TConfigType>
    implements IView
{
    // Führt die Anmeldung auf Benachrichtigungen aus.
    // eslint-disable-next-line @typescript-eslint/naming-convention
    UNSAFE_componentWillMount(): void {
        this.props.uvm.view = this
    }

    // Meldet sich von Benachrichtigungen ab.
    componentWillUnmount(): void {
        this.props.uvm.view = undefined
    }

    // Fordert eine Aktualisierung der Anzeige an.
    refreshUi(): void {
        this.forceUpdate()
    }
}

export abstract class ComponentWithSite<TViewModelType extends IConnectable> extends ComponentExWithSite<
    TViewModelType,
    IComponent<TViewModelType>
> {}
