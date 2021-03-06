﻿using System.Linq;
using FluentAssertions;
using Infusion.Packets;
using Infusion.Packets.Both;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Infusion.Tests.Packets.Both
{
    [TestClass]
    public class SendSkillsPacketTests
    {
        [TestMethod]
        public void Can_deserialize_single_skill_update()
        {
            var rawPacket = FakePackets.Instantiate(new byte[]
            {
                0x3A, 0x00, 0x0B, 0xFF, 0x00, 0x2C, 0x00, 0x0A, 0x00, 0x0A, 0x00,
            });

            var packet = new SendSkillsPacket();
            packet.Deserialize(rawPacket);

            packet.Values.Length.Should().Be(1);
            packet.Values[0].Should().Be(new SkillValue(Skill.Lumberjacking, 0x0A, 0x0A));
        }

        [TestMethod]
        public void Can_deserialize_initial_skill_update()
        {
            var rawPacket = FakePackets.Instantiate(new byte[]
            {
                0x3A, // packet
                0x01, 0x64, // size
                0x00, // type
                0x00, 0x01, 0x01, 0x2C, 0x01, 0x2F, 0x00, // ushort skill, ushort value, ushort unchangedValue, byte lock
                0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x0E, 0x01, 0x32, 0x01, 0x32, 0x00,
                0x00, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x11, 0x01, 0x2C, 0x01, 0x2C, 0x00,
                0x00, 0x12, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x13, 0x01, 0x2F, 0x01, 0x2F, 0x00,
                0x00, 0x14, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x15, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x16, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x17, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x18, 0x01, 0x2C, 0x01, 0x2C, 0x00,
                0x00, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x1A, 0x01, 0xF4, 0x01, 0xF4, 0x00,
                0x00, 0x1B, 0x01, 0x2C, 0x01, 0x2C, 0x00,
                0x00, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x1E, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x1F, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x21, 0x00, 0x64, 0x00, 0x64, 0x00,
                0x00, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x23, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x25, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x26, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x27, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x28, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x2A, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x2B, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x2D, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x2E, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x2F, 0x01, 0xF4, 0x01, 0xF4, 0x00,
                0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x32, 0x01, 0x2C, 0x01, 0x2D, 0x00,
                0x00, 0x00
            });

            var packet = new SendSkillsPacket();
            packet.Deserialize(rawPacket);

            packet.Values.Length.Should().Be(0x32);
            packet.Values.First().Should().Be(new SkillValue(Skill.Alchemy, 0x12C, 0x12F));
            packet.Values.Last().Should().Be(new SkillValue(Skill.Necromancy, 0x12C, 0x12D));
        }
    }
}
