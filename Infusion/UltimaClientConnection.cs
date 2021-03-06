﻿using System;
using System.IO;
using Infusion.Diagnostic;
using Infusion.IO;
using Infusion.Packets;
using PacketLogParser = Infusion.Parsers.PacketLogParser;

namespace Infusion
{
    internal sealed class UltimaClientConnection
    {
        private readonly IDiagnosticPullStream diagnosticPullStream;
        private readonly IDiagnosticPushStream diagnosticPushStream;

        public UltimaClientConnection() : this(
            UltimaClientConnectionStatus.Initial, NullDiagnosticPullStream.Instance,
            NullDiagnosticPushStream.Instance)
        {
        }

        public UltimaClientConnection(UltimaClientConnectionStatus status)
            : this(status, NullDiagnosticPullStream.Instance, NullDiagnosticPushStream.Instance)
        {
        }

        public UltimaClientConnection(UltimaClientConnectionStatus status, IDiagnosticPullStream diagnosticPullStream,
            IDiagnosticPushStream diagnosticPushStream)
        {
            this.diagnosticPullStream = diagnosticPullStream;
            this.diagnosticPushStream = diagnosticPushStream;
            Status = status;
        }

        public UltimaClientConnectionStatus Status { get; private set; }

        public event EventHandler<Packet> PacketReceived;

        public void ReceiveBatch(IPullStream inputStream)
        {
            diagnosticPullStream.BaseStream = inputStream;

            switch (Status)
            {
                case UltimaClientConnectionStatus.Initial:
                    ReceiveSeed(diagnosticPullStream);
                    Status = UltimaClientConnectionStatus.ServerLogin;
                    break;
                case UltimaClientConnectionStatus.PreGameLogin:
                    ReceiveSeed(diagnosticPullStream);
                    Status = UltimaClientConnectionStatus.GameLogin;
                    break;
            }

            foreach (var packet in PacketLogParser.ParseBatch(diagnosticPullStream))
            {
                OnPacketReceived(packet);
                switch (Status)
                {
                    case UltimaClientConnectionStatus.ServerLogin:
                        if (packet.Id == PacketDefinitions.SelectServerRequest.Id)
                            Status = UltimaClientConnectionStatus.PreGameLogin;
                        break;
                    case UltimaClientConnectionStatus.GameLogin:
                        if (packet.Id == PacketDefinitions.GameServerLoginRequest.Id)
                            Status = UltimaClientConnectionStatus.Game;
                        break;
                }
            }
        }

        private void ReceiveSeed(IPullStream inputStream)
        {
            var payload = new byte[4];
            inputStream.Read(payload, 0, 4);
            var packet = new Packet(PacketDefinitions.LoginSeed.Id, payload);
            OnPacketReceived(packet);
        }

        private void OnPacketReceived(Packet packet)
        {
            diagnosticPullStream.FinishPacket(packet);
            PacketReceived?.Invoke(this, packet);
        }

        public void Send(Packet packet, Stream outputStream)
        {
            diagnosticPushStream.DumpPacket(packet);

            switch (Status)
            {
                case UltimaClientConnectionStatus.Initial:
                case UltimaClientConnectionStatus.ServerLogin:
                case UltimaClientConnectionStatus.PreGameLogin:
                    diagnosticPushStream.BaseStream = new StreamToPushStreamAdapter(outputStream);
                    diagnosticPushStream.Write(packet.Payload, 0, packet.Length);
                    break;
                case UltimaClientConnectionStatus.Game:
                    diagnosticPushStream.BaseStream = new StreamToPushStreamAdapter(outputStream);
                    var huffmanStream = new HuffmanStream(new PushStreamToStreamAdapter(diagnosticPushStream));
                    huffmanStream.Write(packet.Payload, 0, packet.Length);
                    break;
                default:
                    throw new NotImplementedException($"Sending packets while in {Status} Status.");
            }

            diagnosticPushStream.Finish();
        }
    }
}