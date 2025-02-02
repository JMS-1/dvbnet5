import * as React from 'react'

import { IPlanPage } from '../../app/pages/plan'
import { IPlanException } from '../../app/pages/plan/exception'
import { HelpLink } from '../../common/helpLink'
import { ButtonCommand } from '../../lib.react/command/button'
import { EditNumberSlider } from '../../lib.react/edit/number/slider'
import { ComponentExWithSite, IComponent } from '../../lib.react/reactUi'

// Schnittstelle zur Pflege einer Ausnahmeregel.
interface IPlanExceptionStatic extends IComponent<IPlanException> {
    // Die aktuell angezeigte Seite.
    page: IPlanPage
}

// React.Js Komponente zur Pflege einer einzelnen Ausnahmeregel.
export class PlanException extends ComponentExWithSite<IPlanException, IPlanExceptionStatic> {
    // Erstellt die Oberflächenelemente zur Pflege.
    render(): React.JSX.Element {
        return (
            <fieldset className='vcrnet-planexception'>
                <legend>
                    Ausnahmeregel bearbeiten
                    <HelpLink page={this.props.page} topic='repeatingschedules' />
                </legend>
                <table className='vcrnet-tableIsForm'>
                    <tbody>
                        <tr>
                            <td>Start</td>
                            <td>{this.props.uvm.currentStart}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Ende</td>
                            <td>{this.props.uvm.currentEnd}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td
                                className={
                                    this.props.uvm.currentDuration <= 0 ? 'vcrnet-planexception-discard' : undefined
                                }
                            >
                                Dauer
                            </td>
                            <td>{`${this.props.uvm.currentDuration} Minute${this.props.uvm.currentDuration === 1 ? '' : 'n'}`}</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>Startverschiebung</td>
                            <td>{`${this.props.uvm.startSlider.value} Minute${Math.abs(this.props.uvm.startSlider.value ?? 0) === 1 ? '' : 'n'}`}</td>
                            <td>
                                <EditNumberSlider uvm={this.props.uvm.startSlider} />
                            </td>
                        </tr>
                        <tr>
                            <td>Laufzeitanpassung</td>
                            <td>{`${this.props.uvm.durationSlider.value} Minute${Math.abs(this.props.uvm.durationSlider.value ?? 0) === 1 ? '' : 'n'}`}</td>
                            <td>
                                <EditNumberSlider uvm={this.props.uvm.durationSlider} />
                            </td>
                        </tr>
                    </tbody>
                </table>
                <div>
                    <ButtonCommand uvm={this.props.uvm.originalTime} />
                    <ButtonCommand uvm={this.props.uvm.skip} />
                    <ButtonCommand uvm={this.props.uvm.update} />
                </div>
            </fieldset>
        )
    }
}
