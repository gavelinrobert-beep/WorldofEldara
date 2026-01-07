import React, { useState } from 'react';
import { MapPin, Skull, Users, BookOpen, Star, Zap, Shield, Swords } from 'lucide-react';

// Types
interface Region {
  id: string;
  name: string;
  levelRange: string;
  tier: string;
  faction: string;
  position: { x: number; y: number };
  description: string;
  color: string;
}

interface QuestArc {
  id: string;
  name: string;
  regionId: string;
  questLine: string;
  gatingItem: string;
  levelRange: string;
}

interface Hub {
  id: string;
  name: string;
  regionId: string;
  type: string;
  position: { x: number; y: number };
}

interface Monster {
  id: string;
  name: string;
  regionId: string;
  level: string;
  type: string;
  family: string;
  mechanics: string[];
}

interface Encounter {
  id: string;
  name: string;
  regionId: string;
  bossType: string;
  level: number;
  mechanics: string[];
}

// Data
const regions: Region[] = [
  {
    id: 'worldroot-expanse',
    name: 'Worldroot Expanse',
    levelRange: '1-15',
    tier: 'Starter',
    faction: 'Verdant Circles (Sylvaen)',
    position: { x: 15, y: 40 },
    description: 'Ancient forest with visible Worldroot manifestations. Maximum Worldroot density.',
    color: '#2d5016'
  },
  {
    id: 'verdaniels-continuity-reach',
    name: "Verdaniel's Continuity Reach",
    levelRange: '15-20',
    tier: 'Starter',
    faction: 'Ascendant League (High Elves)',
    position: { x: 50, y: 15 },
    description: 'Temporal Steppes where time flows inconsistently. Reality-unstable.',
    color: '#4a5568'
  },
  {
    id: 'vaels-shattered-totem-wilds',
    name: "Vael's Shattered Totem Wilds",
    levelRange: '20-35',
    tier: 'Mid',
    faction: 'Totem Clans (Therakai)',
    position: { x: 75, y: 45 },
    description: 'Untamed wilderness split between Wildborn and Pathbound. Totem Spirit shrines throughout.',
    color: '#744210'
  },
  {
    id: 'korraths-warfront-marches',
    name: "Korrath's Warfront Marches",
    levelRange: '25-35',
    tier: 'Mid',
    faction: 'Contested Territory',
    position: { x: 50, y: 55 },
    description: 'War-torn battlefields where factions clash. Damaged Worldroot.',
    color: '#7f1d1d'
  },
  {
    id: 'nereths-silent-fens',
    name: "Nereth's Silent Fens",
    levelRange: '30-40',
    tier: 'Mid-High',
    faction: 'Void Compact (Outcasts)',
    position: { x: 65, y: 75 },
    description: 'Death-corrupted marshlands. Blackwake Haven operates here. Absent Worldroot.',
    color: '#1e293b'
  },
  {
    id: 'astraes-lost-observatories',
    name: "Astrae's Lost Observatories",
    levelRange: '40-50',
    tier: 'High',
    faction: 'Seekers of Astrae',
    position: { x: 80, y: 20 },
    description: 'Ancient observatories with temporal anomalies. Time loops and future echoes.',
    color: '#312e81'
  },
  {
    id: 'the-null-scars',
    name: 'The Null Scars',
    levelRange: '45-60',
    tier: 'High-Endgame',
    faction: 'Dominion Warhost (Gronnak)',
    position: { x: 50, y: 85 },
    description: 'Reality scar where gods die. Krag\'Thuun stands here. Null corruption visible.',
    color: '#18181b'
  }
];

const questArcs: QuestArc[] = [
  {
    id: 'bleeding-root',
    name: 'The Bleeding Root',
    regionId: 'worldroot-expanse',
    questLine: 'Seal corrupted Worldroot breach',
    gatingItem: 'Root Sigil',
    levelRange: '1-15'
  },
  {
    id: 'echoes-severance',
    name: 'Echoes of the Severance',
    regionId: 'verdaniels-continuity-reach',
    questLine: 'Recover High Elf memory archive',
    gatingItem: 'Seed Sigil',
    levelRange: '15-20'
  },
  {
    id: 'war-instinct',
    name: 'The War of Instinct',
    regionId: 'vaels-shattered-totem-wilds',
    questLine: 'Unite or influence Therakai conflict',
    gatingItem: 'Totem Shard',
    levelRange: '20-30'
  },
  {
    id: 'gods-silence',
    name: "The God of War's Silence",
    regionId: 'korraths-warfront-marches',
    questLine: 'Prove worth in factional warfare',
    gatingItem: 'Banner Commendation',
    levelRange: '25-35'
  },
  {
    id: 'deaths-veil',
    name: "Beyond Death's Veil",
    regionId: 'nereths-silent-fens',
    questLine: 'Survive Void exposure in Blackwake Haven',
    gatingItem: 'Veil Token',
    levelRange: '30-40'
  },
  {
    id: 'missing-god',
    name: 'The Missing God',
    regionId: 'astraes-lost-observatories',
    questLine: 'Solve temporal puzzles and witness Astrae\'s capture',
    gatingItem: 'Star Prism',
    levelRange: '40-50'
  },
  {
    id: 'into-scar',
    name: 'Into the Scar',
    regionId: 'the-null-scars',
    questLine: 'Survive reality-scar exposure',
    gatingItem: 'Null Anchor Kit',
    levelRange: '50-60'
  }
];

const hubs: Hub[] = [
  { id: 'thornveil', name: 'Thornveil Enclave', regionId: 'worldroot-expanse', type: 'Main Hub', position: { x: 20, y: 35 } },
  { id: 'spire', name: 'Spire of Echoes', regionId: 'verdaniels-continuity-reach', type: 'Main Hub', position: { x: 55, y: 20 } },
  { id: 'vharos', name: 'Vharos (Split City)', regionId: 'vaels-shattered-totem-wilds', type: 'PvP Hub', position: { x: 70, y: 50 } },
  { id: 'banner', name: "Banner's Stand", regionId: 'korraths-warfront-marches', type: 'Neutral Hub', position: { x: 45, y: 60 } },
  { id: 'blackwake', name: 'Blackwake Haven', regionId: 'nereths-silent-fens', type: 'Pirate Hub', position: { x: 60, y: 80 } },
  { id: 'stellar', name: 'The Stellar Vault', regionId: 'astraes-lost-observatories', type: 'Research Hub', position: { x: 85, y: 15 } },
  { id: 'kragthuun', name: 'Krag\'Thuun', regionId: 'the-null-scars', type: 'Faction Hub', position: { x: 55, y: 90 } }
];

const encounters: Encounter[] = [
  {
    id: 'bleeding-root-boss',
    name: 'The Bleeding Root',
    regionId: 'worldroot-expanse',
    bossType: 'Dungeon Boss',
    level: 15,
    mechanics: ['Corruption Pools', 'Memory Echoes', 'Root Prison', 'Corruption Nova']
  },
  {
    id: 'temporal-construct',
    name: 'Temporal Construct',
    regionId: 'verdaniels-continuity-reach',
    bossType: 'Dungeon Boss',
    level: 20,
    mechanics: ['Reality Editing', 'Temporal Fracture', 'Time Bolt', 'Reality Collapse']
  },
  {
    id: 'vaels-echo',
    name: "Vael's Echo",
    regionId: 'vaels-shattered-totem-wilds',
    bossType: 'Raid Boss',
    level: 35,
    mechanics: ['Primal Rage', 'Strategic Stance', 'Totem Storm', 'Instinct/Choice Phases']
  },
  {
    id: 'silent-altar',
    name: "Korrath's Silent Altar",
    regionId: 'korraths-warfront-marches',
    bossType: 'World Raid',
    level: 35,
    mechanics: ['Divine Blessing', 'War Drums', 'Korrath\'s Silence', 'Null Corruption']
  },
  {
    id: 'nereths-avatar',
    name: "Nereth's Broken Avatar",
    regionId: 'nereths-silent-fens',
    bossType: 'Dungeon Boss',
    level: 40,
    mechanics: ['Death Touch', 'Life/Death Confusion', 'Reality Fracture', 'Null Corruption']
  },
  {
    id: 'astrae-trapped',
    name: 'Astrae, The Trapped God',
    regionId: 'astraes-lost-observatories',
    bossType: 'Raid Boss',
    level: 50,
    mechanics: ['Time Reversal', 'Triple Form', 'Causality Cascade', 'Temporal Loop']
  },
  {
    id: 'gharak',
    name: 'Witch-King Supreme Gharak',
    regionId: 'the-null-scars',
    bossType: 'World Raid Boss',
    level: 60,
    mechanics: ['Multi-God Arsenal', 'Null Transformation', 'Whisper of Null', 'Existence Unravel']
  }
];

const monsters: Monster[] = [
  {
    id: 'corrupted-wolf',
    name: 'Diseased Wolf',
    regionId: 'worldroot-expanse',
    level: '3-5',
    type: 'Trash',
    family: 'Corrupted Wildlife',
    mechanics: ['Pack Tactics', 'Bleed Effect']
  },
  {
    id: 'root-horror',
    name: 'Root Horror',
    regionId: 'worldroot-expanse',
    level: '8-10',
    type: 'Elite',
    family: 'Root Horrors',
    mechanics: ['Root Grasp', 'Strangling Vines', 'Summon Sprouts']
  },
  {
    id: 'phase-imp',
    name: 'Phase Imp',
    regionId: 'verdaniels-continuity-reach',
    level: '15-16',
    type: 'Trash',
    family: 'Temporal Echoes',
    mechanics: ['Blink', 'Time Skip']
  },
  {
    id: 'reality-render',
    name: 'Reality Render',
    regionId: 'verdaniels-continuity-reach',
    level: '19-20',
    type: 'Elite',
    family: 'Reality Predators',
    mechanics: ['Reality Tear', 'Time Fracture', 'Existence Unravel']
  },
  {
    id: 'wildborn-berserker',
    name: 'Wildborn Berserker',
    regionId: 'vaels-shattered-totem-wilds',
    level: '24-28',
    type: 'Elite',
    family: 'Wildborn Therakai',
    mechanics: ['Rage', 'Reckless Charge', 'Ignore Pain']
  },
  {
    id: 'primal-nightmare',
    name: 'Primal Nightmare',
    regionId: 'vaels-shattered-totem-wilds',
    level: '30-32',
    type: 'Elite',
    family: 'Corrupted Totems',
    mechanics: ['Nightmare Form', 'Instinct Overwhelm', 'Reality Tear']
  },
  {
    id: 'siege-golem',
    name: 'Siege Golem',
    regionId: 'korraths-warfront-marches',
    level: '31-34',
    type: 'Elite',
    family: 'War-Touched Creatures',
    mechanics: ['Siege Blast', 'Fortified', 'Ground Pound']
  },
  {
    id: 'war-spirit',
    name: 'War Spirit',
    regionId: 'korraths-warfront-marches',
    level: '32-34',
    type: 'Elite',
    family: 'Divine Remnants',
    mechanics: ['Divine Strike', 'War Cry', 'Intangible']
  },
  {
    id: 'marsh-revenant',
    name: 'Marsh Revenant',
    regionId: 'nereths-silent-fens',
    level: '33-36',
    type: 'Elite',
    family: 'Undead Abominations',
    mechanics: ['Death Touch', 'Summon Drowned', 'Life Drain']
  },
  {
    id: 'reality-eater',
    name: 'Reality Eater',
    regionId: 'nereths-silent-fens',
    level: '37-38',
    type: 'Elite',
    family: 'Void-Touched Beasts',
    mechanics: ['Consume Reality', 'Existence Fray', 'Phase Swarm']
  },
  {
    id: 'time-keeper',
    name: 'Time Keeper',
    regionId: 'astraes-lost-observatories',
    level: '43-45',
    type: 'Elite',
    family: 'Temporal Guardians',
    mechanics: ['Time Stop', 'Temporal Loop', 'Accelerate']
  },
  {
    id: 'causality-breaker',
    name: 'Causality Breaker',
    regionId: 'astraes-lost-observatories',
    level: '47-48',
    type: 'Elite',
    family: 'Paradox Entities',
    mechanics: ['Reverse Time', 'Break Causality', 'Timeline Fracture']
  },
  {
    id: 'witch-king-adept',
    name: 'Witch-King Adept',
    regionId: 'the-null-scars',
    level: '49-52',
    type: 'Elite',
    family: 'Dominion Warhost',
    mechanics: ['God-Fragment Power', 'Dominion Curse', 'Life Drain']
  },
  {
    id: 'void-herald',
    name: 'Void Herald',
    regionId: 'the-null-scars',
    level: '58-60',
    type: 'Elite',
    family: 'Null Entities',
    mechanics: ['Herald\'s Call', 'Reality Collapse', 'Null Gaze']
  }
];

// Main Component
const EldaraWorldMap: React.FC = () => {
  const [selectedRegion, setSelectedRegion] = useState<Region | null>(null);
  const [activeTab, setActiveTab] = useState<'overview' | 'quests' | 'encounters' | 'monsters'>('overview');

  const getRegionData = (regionId: string) => {
    return {
      quests: questArcs.filter(q => q.regionId === regionId),
      hubs: hubs.filter(h => h.regionId === regionId),
      encounters: encounters.filter(e => e.regionId === regionId),
      monsters: monsters.filter(m => m.regionId === regionId)
    };
  };

  return (
    <div className="w-full h-screen bg-gray-900 text-white p-4 flex flex-col">
      <div className="mb-4">
        <h1 className="text-3xl font-bold text-center mb-2">World of Eldara - Interactive Map</h1>
        <p className="text-center text-gray-400">Explore the seven regions, quest arcs, encounters, and monsters</p>
      </div>

      <div className="flex-1 flex gap-4 overflow-hidden">
        {/* Map Section */}
        <div className="flex-1 bg-gray-800 rounded-lg p-4 relative overflow-hidden">
          <div className="relative w-full h-full">
            {/* SVG Map */}
            <svg className="w-full h-full" viewBox="0 0 100 100" preserveAspectRatio="xMidYMid meet">
              {/* Background */}
              <rect width="100" height="100" fill="#1a1a1a" />
              
              {/* Connection Lines */}
              <line x1="15" y1="40" x2="50" y2="15" stroke="#4a5568" strokeWidth="0.3" strokeDasharray="1,1" />
              <line x1="15" y1="40" x2="50" y2="55" stroke="#4a5568" strokeWidth="0.3" strokeDasharray="1,1" />
              <line x1="50" y1="15" x2="75" y2="45" stroke="#4a5568" strokeWidth="0.3" strokeDasharray="1,1" />
              <line x1="75" y1="45" x2="50" y2="55" stroke="#4a5568" strokeWidth="0.3" strokeDasharray="1,1" />
              <line x1="50" y1="55" x2="65" y2="75" stroke="#4a5568" strokeWidth="0.3" strokeDasharray="1,1" />
              <line x1="65" y1="75" x2="50" y2="85" stroke="#4a5568" strokeWidth="0.3" strokeDasharray="1,1" />
              <line x1="75" y1="45" x2="80" y2="20" stroke="#4a5568" strokeWidth="0.3" strokeDasharray="1,1" />
              
              {/* Regions */}
              {regions.map((region) => (
                <g key={region.id}>
                  <circle
                    cx={region.position.x}
                    cy={region.position.y}
                    r="6"
                    fill={region.color}
                    stroke={selectedRegion?.id === region.id ? '#fbbf24' : '#fff'}
                    strokeWidth={selectedRegion?.id === region.id ? '0.5' : '0.2'}
                    className="cursor-pointer transition-all hover:stroke-amber-400"
                    onClick={() => setSelectedRegion(region)}
                  />
                  <text
                    x={region.position.x}
                    y={region.position.y - 8}
                    fontSize="2"
                    fill="white"
                    textAnchor="middle"
                    className="pointer-events-none"
                  >
                    {region.name.split(' ')[0]}
                  </text>
                  <text
                    x={region.position.x}
                    y={region.position.y + 10}
                    fontSize="1.5"
                    fill="#9ca3af"
                    textAnchor="middle"
                    className="pointer-events-none"
                  >
                    Lv {region.levelRange}
                  </text>
                </g>
              ))}

              {/* Hubs */}
              {hubs.map((hub) => (
                <g key={hub.id}>
                  <circle
                    cx={hub.position.x}
                    cy={hub.position.y}
                    r="2"
                    fill="#3b82f6"
                    stroke="#fff"
                    strokeWidth="0.2"
                    className="cursor-pointer hover:fill-blue-400"
                  />
                </g>
              ))}
            </svg>
          </div>
        </div>

        {/* Details Panel */}
        <div className="w-96 bg-gray-800 rounded-lg p-4 flex flex-col">
          {selectedRegion ? (
            <>
              <div className="mb-4">
                <h2 className="text-2xl font-bold mb-2">{selectedRegion.name}</h2>
                <div className="space-y-1 text-sm">
                  <p><span className="text-gray-400">Level Range:</span> {selectedRegion.levelRange}</p>
                  <p><span className="text-gray-400">Tier:</span> {selectedRegion.tier}</p>
                  <p><span className="text-gray-400">Faction:</span> {selectedRegion.faction}</p>
                  <p className="text-gray-300 mt-2">{selectedRegion.description}</p>
                </div>
              </div>

              {/* Tabs */}
              <div className="flex gap-2 mb-4 border-b border-gray-700">
                <button
                  className={`px-3 py-2 text-sm ${activeTab === 'overview' ? 'text-amber-400 border-b-2 border-amber-400' : 'text-gray-400'}`}
                  onClick={() => setActiveTab('overview')}
                >
                  <MapPin className="inline w-4 h-4 mr-1" />
                  Overview
                </button>
                <button
                  className={`px-3 py-2 text-sm ${activeTab === 'quests' ? 'text-amber-400 border-b-2 border-amber-400' : 'text-gray-400'}`}
                  onClick={() => setActiveTab('quests')}
                >
                  <BookOpen className="inline w-4 h-4 mr-1" />
                  Quests
                </button>
                <button
                  className={`px-3 py-2 text-sm ${activeTab === 'encounters' ? 'text-amber-400 border-b-2 border-amber-400' : 'text-gray-400'}`}
                  onClick={() => setActiveTab('encounters')}
                >
                  <Skull className="inline w-4 h-4 mr-1" />
                  Bosses
                </button>
                <button
                  className={`px-3 py-2 text-sm ${activeTab === 'monsters' ? 'text-amber-400 border-b-2 border-amber-400' : 'text-gray-400'}`}
                  onClick={() => setActiveTab('monsters')}
                >
                  <Swords className="inline w-4 h-4 mr-1" />
                  Monsters
                </button>
              </div>

              {/* Tab Content */}
              <div className="flex-1 overflow-y-auto">
                {activeTab === 'overview' && (
                  <div className="space-y-4">
                    <div>
                      <h3 className="text-lg font-semibold mb-2 flex items-center">
                        <Users className="w-5 h-5 mr-2" />
                        Hubs
                      </h3>
                      {getRegionData(selectedRegion.id).hubs.map(hub => (
                        <div key={hub.id} className="bg-gray-700 p-2 rounded mb-2">
                          <p className="font-medium">{hub.name}</p>
                          <p className="text-xs text-gray-400">{hub.type}</p>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {activeTab === 'quests' && (
                  <div className="space-y-3">
                    {getRegionData(selectedRegion.id).quests.map(quest => (
                      <div key={quest.id} className="bg-gray-700 p-3 rounded">
                        <h4 className="font-semibold text-amber-400 mb-1">{quest.name}</h4>
                        <p className="text-sm mb-2">{quest.questLine}</p>
                        <div className="flex items-center gap-2 text-xs">
                          <span className="bg-green-700 px-2 py-1 rounded">Lv {quest.levelRange}</span>
                          <span className="bg-purple-700 px-2 py-1 rounded flex items-center">
                            <Star className="w-3 h-3 mr-1" />
                            {quest.gatingItem}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                )}

                {activeTab === 'encounters' && (
                  <div className="space-y-3">
                    {getRegionData(selectedRegion.id).encounters.map(encounter => (
                      <div key={encounter.id} className="bg-gray-700 p-3 rounded">
                        <h4 className="font-semibold text-red-400 mb-1">{encounter.name}</h4>
                        <p className="text-xs text-gray-400 mb-2">{encounter.bossType} - Level {encounter.level}</p>
                        <div className="space-y-1">
                          <p className="text-xs font-semibold text-gray-300">Mechanics:</p>
                          {encounter.mechanics.map((mechanic, idx) => (
                            <div key={idx} className="flex items-center text-xs">
                              <Zap className="w-3 h-3 mr-1 text-yellow-500" />
                              {mechanic}
                            </div>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                )}

                {activeTab === 'monsters' && (
                  <div className="space-y-3">
                    {getRegionData(selectedRegion.id).monsters.map(monster => (
                      <div key={monster.id} className="bg-gray-700 p-3 rounded">
                        <h4 className="font-semibold mb-1">{monster.name}</h4>
                        <div className="flex gap-2 mb-2">
                          <span className="text-xs bg-blue-700 px-2 py-1 rounded">Lv {monster.level}</span>
                          <span className={`text-xs px-2 py-1 rounded ${
                            monster.type === 'Elite' ? 'bg-yellow-700' : 'bg-gray-600'
                          }`}>
                            {monster.type}
                          </span>
                        </div>
                        <p className="text-xs text-gray-400 mb-1">{monster.family}</p>
                        <div className="space-y-1">
                          {monster.mechanics.map((mechanic, idx) => (
                            <div key={idx} className="flex items-center text-xs text-gray-300">
                              <Shield className="w-3 h-3 mr-1 text-cyan-500" />
                              {mechanic}
                            </div>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </>
          ) : (
            <div className="flex-1 flex items-center justify-center text-center text-gray-400">
              <div>
                <MapPin className="w-16 h-16 mx-auto mb-4 opacity-50" />
                <p className="text-lg">Select a region on the map</p>
                <p className="text-sm mt-2">Click any region circle to view details</p>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Legend */}
      <div className="mt-4 bg-gray-800 rounded-lg p-3 flex gap-6 text-sm">
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 rounded-full bg-green-700"></div>
          <span>Starter Zone</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 rounded-full bg-amber-700"></div>
          <span>Mid Tier</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-4 h-4 rounded-full bg-red-700"></div>
          <span>High Tier</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-blue-500"></div>
          <span>Hub</span>
        </div>
      </div>
    </div>
  );
};

export default EldaraWorldMap;
