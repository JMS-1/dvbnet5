﻿import * as React from 'react'

import { IMultiValueFromList } from '../../lib/edit/multiList'
import { ComponentExWithSite, IComponent } from '../reactUi'

// Konfiguration zur Auswahl mehrerer Elemente aus einer Liste erlaubter Elemente.
export interface ISelectMultipleFromList extends IComponent<IMultiValueFromList<unknown>> {
    // Die Größe der Liste.
    items: number
}

// React.Js Komponente zur Auswahl von mehreren Elementen aus eine Liste von Elementen.
export class MultiSelect extends ComponentExWithSite<IMultiValueFromList<unknown>, ISelectMultipleFromList> {
    // Erstellt die Oberflächenelemente.
    render(): React.JSX.Element {
        return (
            <select
                className='jmslib-editmultilist'
                multiple={true}
                size={this.props.items}
                value={this.props.uvm.allowedValues
                    .map((v, index) => (v.isSelected ? index : -1))
                    .filter((i) => i >= 0)
                    .map((i) => `${i}`)}
                onChange={(ev) => this.onChange(ev)}
            >
                {this.props.uvm.allowedValues.map((v, index) => (
                    <option key={index} value={`${index}`}>
                        {v.display}
                    </option>
                ))}
            </select>
        )
    }

    // Die Veränderung der Auswahl ist in dieser Implementierung etwas aufwändiger, da wir den Algorithmus des Browsers unverändert übernehmen wollen.
    private onChange(ev: React.FormEvent<HTMLSelectElement>): void {
        // Alle Oberflächenelemente zur Auswahlliste.
        const options = (ev.currentTarget as HTMLSelectElement).children

        // Die zugehörige Auswahlliste.
        const values = this.props.uvm.allowedValues

        // Auswahl einfach wie vom Browser gewünscht übertragen.
        for (let i = 0; i < options.length; i++)
            this.props.uvm.allowedValues[i].isSelected = (options[i] as HTMLOptionElement).selected
    }
}
