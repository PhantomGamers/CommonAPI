﻿using CommonAPI.Systems;
using NebulaAPI;

namespace CommonAPI.Nebula
{
    public class StarSystemData
    {
        public int starIndex { get; set; }
        public byte[] binaryData { get; set; }

        public StarSystemData() { }
        public StarSystemData(int starIndex, byte[] data)
        {
            this.starIndex = starIndex;
            binaryData = data;
        }
    }
    
    [RegisterPacketProcessor]
    public class StarSystemDataProcessor : BasePacketProcessor<StarSystemData>
    {
        public override void ProcessPacket(StarSystemData packet, INebulaConnection conn)
        {
            if (IsHost) return;

            StarData star = GameMain.galaxy.StarById(packet.starIndex + 1);
            using IReaderProvider p = NebulaModAPI.GetBinaryReader(packet.binaryData);

            for (int i = 1; i < CustomStarSystem.registry.data.Count; i++)
            {
                StarSystemStorage system = CustomStarSystem.systems[i];
                system.GetSystem(star).Import(p.BinaryReader);
            }
        }
    }
}