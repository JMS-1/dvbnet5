import { ISourceInformationContract } from './ISourceInformationContract'
import { doUrlCall } from './VCRServer'

// Repräsentiert die Klasse ProfileSource
export interface IProfileSourceContract extends ISourceInformationContract {
    // Gesetzt, wenn es sich um einen Fernseh- und nicht einen Radiosender handelt.
    tvNotRadio: boolean
}

export function getProfileSources(device: string): Promise<IProfileSourceContract[] | undefined> {
    return doUrlCall(`profile/${device}`)
}
