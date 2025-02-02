import * as React from 'react'

import { IDaySelector, ISelectableDay } from '../../../lib/edit/datetime/day'
import { ButtonCommand } from '../../command/button'
import { Pictogram } from '../../command/pictogram'
import { ComponentWithSite } from '../../reactUi'

// React.JS Komponente zur Auswahl eines Datums.
export class EditDay extends ComponentWithSite<IDaySelector> {
    // Anzeige erstellen.
    render(): React.JSX.Element {
        return (
            <div className='jmslib-editday jmslib-validatable' title={this.props.uvm.message}>
                <div>
                    <Pictogram name='prev' onClick={(ev) => this.props.uvm.monthBackward.execute()} />
                    <div>
                        <select
                            value={this.props.uvm.month}
                            onChange={(ev) => (this.props.uvm.month = (ev.target as HTMLSelectElement).value)}
                        >
                            {this.props.uvm.months.map((m) => (
                                <option key={m} value={m}>
                                    {m}
                                </option>
                            ))}
                        </select>
                        <select
                            value={this.props.uvm.year}
                            onChange={(ev) => (this.props.uvm.year = (ev.target as HTMLSelectElement).value)}
                        >
                            {this.props.uvm.years.map((m) => (
                                <option key={m} value={m}>
                                    {m}
                                </option>
                            ))}
                        </select>
                    </div>
                    <Pictogram name='next' onClick={(ev) => this.props.uvm.monthForward.execute()} />
                </div>
                <table>
                    <thead>
                        <tr>
                            {this.props.uvm.dayNames.map((n) => (
                                <td key={n}>{n}</td>
                            ))}
                        </tr>
                    </thead>
                    <tbody>{this.getRows(this.props.uvm.days)}</tbody>
                </table>
                <div>
                    <ButtonCommand uvm={this.props.uvm.today} />
                    <ButtonCommand uvm={this.props.uvm.reset} />
                </div>
            </div>
        )
    }

    // Ermittelt eine Woche mit auswählbaren Tagen.
    private getRow(days: ISelectableDay[], rowKey: number): React.JSX.Element | null {
        // Prüfen ob genug Tage zur Verfügung stehen.
        if (days.length !== 7) return null

        // Oberflächenelemente erstellen.
        return (
            <tr key={rowKey}>
                {days.map((day, index) => {
                    const classes: string[] = []

                    // Visualisierung der Sondertage vorbereiten.
                    if (day.isCurrentMonth) classes.push('jmslib-editday-month')
                    if (day.isCurrentDay) classes.push('jmslib-editday-selected')
                    if (day.isToday) classes.push('jmslib-editday-today')

                    // Oberflächenelemente für einen einzelnen Tag auswählbaren erstellen.
                    return (
                        <td key={`${index}`} className={classes.join(' ')} onClick={day.select}>
                            {day.display}
                        </td>
                    )
                })}
            </tr>
        )
    }

    // Teilt die zur Auswahl anzubietende Tage in Häppchen zu je einer Woche.
    private getRows(days: ISelectableDay[]): (React.JSX.Element | null)[] {
        const rows: (React.JSX.Element | null)[] = []

        // Tage in Schritten einer Woche zerlegen.
        for (let ix = 0; ix < days.length; ix += 7) rows.push(this.getRow(days.slice(ix, ix + 7), ix))

        return rows
    }
}
