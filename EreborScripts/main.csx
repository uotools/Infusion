﻿#r "..\UltimaRX\bin\Debug\UltimaRX.dll"
#r "..\UltimaRX.Proxy\bin\Debug\UltimaRX.Proxy.exe"
#load "constants.csx"
#load "item_types.csx"

using System;
using System.Threading;
using UltimaRX.Proxy;
using static UltimaRX.Proxy.Program;

Start(currentConnection);