using System;
using System.Collections.Generic;

namespace Aislinn.Core.Models
{
    public class Chunk
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string ChunkType { get; set; }
        public string Name { get; set; }
        public double[] Vector { get; set; }
        public double ActivationLevel { get; set; }
        public Dictionary<string, ModelSlot> Slots { get; set; }
        public List<ActivationHistoryItem> ActivationHistory { get; set; }


        // public Chunk GetChunkAttr(string name) {
        //     //returns a chunk from an attribute, assuming the attribute is a chunkID
        //     //will need to pass a store reference on chunk creation to do this.
        // }

        public Chunk()
        {
            Slots = new Dictionary<string, ModelSlot>();
            ActivationHistory = new List<ActivationHistoryItem>();
        }
    }
}