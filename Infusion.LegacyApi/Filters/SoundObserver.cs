﻿using System.Collections.Generic;
using Infusion.LegacyApi.Events;
using Infusion.Packets;
using Infusion.Packets.Server;

namespace Infusion.LegacyApi.Filters
{
    internal sealed class SoundObserver : ISoundFilter
    {
        private readonly IEventJournalSource eventJournalSource;
        private ISet<SoundId> filteredSounds = null;

        public SoundObserver(IServerPacketSubject serverPacketHandler, IEventJournalSource eventJournalSource)
        {
            this.eventJournalSource = eventJournalSource;
            serverPacketHandler.RegisterFilter(FilterBlockedSounds);
        }

        private Packet? FilterBlockedSounds(Packet rawPacket)
        {
            var sounds = filteredSounds;

            if (sounds != null)
            {
                if (rawPacket.Id == PacketDefinitions.PlaySoundEffect.Id)
                {
                    PlaySoundEffectPacket packet =
                        PacketDefinitionRegistry.Materialize<PlaySoundEffectPacket>(rawPacket);

                    var ev = new SoundEffectPlayedEvent(packet.Id, packet.Location);
                    eventJournalSource.Publish(ev);

                    if (sounds.Contains(packet.Id))
                        return null;
                }
            }

            return rawPacket;
        }

        public void SetFilteredSounds(IEnumerable<SoundId> sounds)
        {
            filteredSounds = new HashSet<SoundId>(sounds);
        }

        public void Disable()
        {
            filteredSounds = null;
        }
    }
}
