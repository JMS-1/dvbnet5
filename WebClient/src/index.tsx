import './index.scss'

import * as React from 'react'
import { createRoot } from 'react-dom/client'

import { Main } from './common/main'
import { Pictogram } from './lib.react/command/pictogram'

// Bilbliothek konfigurieren.
Pictogram.imageRoot = 'images/'

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
createRoot(document.querySelector('body > vcrnet-spa')!).render(<Main />)
