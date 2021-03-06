﻿using Infusion.IO;

namespace Infusion.Packets.Server
{
    internal sealed class SendGumpMenuDialogPacket : MaterializedPacket
    {
        private Packet rawPacket;
        public GumpTypeId GumpTypeId { get; private set; }
        public GumpInstanceId GumpId { get; private set; }
        public uint X { get; private set; }
        public uint Y { get; private set; }
        public string Commands { get; private set; }
        public string[] TextLines { get; private set; }

        public override Packet RawPacket => rawPacket;

        public override void Deserialize(Packet rawPacket)
        {
            this.rawPacket = rawPacket;

            var reader = new ArrayPacketReader(rawPacket.Payload);
            reader.Skip(3);

            GumpId = new GumpInstanceId(reader.ReadUInt());
            GumpTypeId = new GumpTypeId(reader.ReadUInt());
            X = reader.ReadUInt();
            Y = reader.ReadUInt();

            var commandSectionLength = reader.ReadUShort();
            Commands = reader.ReadString(commandSectionLength);

            var textLinesCount = reader.ReadUShort();
            TextLines = new string[textLinesCount];
            for (var i = 0; i < textLinesCount; i++)
            {
                var textLength = reader.ReadUShort();
                TextLines[i] = reader.ReadUnicodeString(textLength);
            }
        }
    }
}