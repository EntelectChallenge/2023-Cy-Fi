using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Enums;
using Domain.Models;

namespace Domain.Configs
{
    public class EngineConfig
    {
        public string RunnerUrl { get; set; }
        public string RunnerPort { get; set; }
        public int BotCount { get; set; }
        public int MaxTicks { get; set; }
        public int TickRate { get; set; }
        public int ProcessTick { get; set; }
        public int WorldSeed { get; set; }
        public Seeds Seeds { get; set; }
    }
    
    public class Seeds
    {
        public List<int> PlayerSeeds { get; set; }
        public int MaxSeed { get; set; }
        public int MinSeed { get; set; }
    }
}