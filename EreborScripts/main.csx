﻿#load "ItemTypes.cs"
#load "common.csx"
#load "Scripts.cs"
#load "cooking.csx"
#load "looting.csx"
#load "PipkaDolAmroth.cs"
#load "MapRecorder.cs"
#load "commands.csx"

using System;
using System.Threading;
using UltimaRX.Proxy;
using UltimaRX.Packets;
using UltimaRX.Proxy.InjectionApi;
using UltimaRX.Packets.Parsers;
using UltimaRX.Gumps;
using static UltimaRX.Proxy.InjectionApi.Injection;
using static Scripts;

void Start()
{
    Program.Start(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("89.185.244.24"), 2593), 33334);
}
