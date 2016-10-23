﻿namespace UltimaRX.Packets.PacketDefinitions.Server
{
    public class RejectMoveItemRequestDefinition : PacketDefinition
    {
        public RejectMoveItemRequestDefinition() : base(Id, new StaticPacketLength(2))
        {
        }

        public new static int Id => 0x27;
    }
}