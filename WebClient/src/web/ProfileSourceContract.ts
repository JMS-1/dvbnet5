import { SourceInformationContract } from './SourceInformationContract'
import { doUrlCall } from './vcrserver'

// Repräsentiert die Klasse ProfileSource
export interface ProfileSourceContract extends SourceInformationContract {
    // Gesetzt, wenn es sich um einen Fernseh- und nicht einen Radiosender handelt.
    tvNotRadio: boolean
}

export function getProfileSources(device: string): Promise<ProfileSourceContract[] | undefined> {
    return doUrlCall(`profile/${device}`)
}
