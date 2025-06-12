using System;
using System.Collections.Generic;

namespace Aislinn.Core.Models
{

    public class Chunk
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        //Implementation type of the chunk, e.g. "Memory", "Procedure", "Goal", etc.
        public string ChunkType { get; set; }
        //A more specific implementation subtype, used as needed
        public string CognitiveCategory { get; set; }
        //user-defined semantic type that might be used in certain algoritms for heuristics
        public string SemanticType { get; set; }
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
        //we'll move this to a utility function later, good for now
        public void DebugConsole()
        {
            Console.WriteLine("==== Chunk Details ====");
            Console.WriteLine($"ID: {this.ID}");
            Console.WriteLine($"Name: {this.Name}");
            Console.WriteLine($"ChunkType: {this.ChunkType}");
            Console.WriteLine($"CognitiveCategory: {this.CognitiveCategory}");
            Console.WriteLine($"SemanticType: {this.SemanticType}");
            Console.WriteLine($"ActivationLevel: {this.ActivationLevel:F4}");

            if (this.Vector != null && this.Vector.Length > 0)
            {
                Console.WriteLine("Vector: [" + string.Join(", ", this.Vector.Select(v => v.ToString("F4"))) + "]");
            }
            else
            {
                Console.WriteLine("Vector: null or empty");
            }

            Console.WriteLine("\n-- Slots --");
            if (this.Slots != null && this.Slots.Any())
            {
                foreach (var kvp in this.Slots)
                {
                    var slot = kvp.Value;
                    Console.WriteLine($"Slot Key: {kvp.Key}");
                    Console.WriteLine($"  Name: {slot.Name}");
                    Console.WriteLine($"  SlotType: {slot.SlotType}");
                    Console.WriteLine($"  Value: {slot.Value ?? "null"}");
                }
            }
            else
            {
                Console.WriteLine("No slots defined.");
            }

            Console.WriteLine("\n-- Activation History --");
            if (this.ActivationHistory != null && this.ActivationHistory.Any())
            {
                foreach (var entry in this.ActivationHistory)
                {
                    Console.WriteLine($"  {entry}"); // Customize this based on ActivationHistoryItem's ToString()
                }
            }
            else
            {
                Console.WriteLine("No activation history.");
            }

            Console.WriteLine("========================\n");
        }
    }
}