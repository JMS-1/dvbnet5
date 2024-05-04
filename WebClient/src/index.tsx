import './index.scss'

import * as React from 'react'
import { createRoot } from 'react-dom/client'
import { Pictogram } from './lib.react/command/pictogram'
import { Main } from './common/main'

// Bilbliothek konfigurieren.
Pictogram.imageRoot = `content/images/`

// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
createRoot(document.querySelector('body > client-root')!).render(<Main />)
