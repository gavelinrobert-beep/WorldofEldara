import React, { useState } from 'react';

interface Region {
  id: string;
  name: string;
  levelRange: string;
  minLevel: number;
  maxLevel: number;
  hub: string;
  traversalUnlock: string;
  loreHook: string;
  color: string;
  position: { x: number; y: number };
  size: number;
  tier: string;
  progressionOrder: number;
}

const regions: Region[] = [
  {
    id: 'worldroot',
    name: 'Worldroot',
    levelRange: '1-10',
    minLevel: 1,
    maxLevel: 10,
    hub: 'Thornveil Enclave',
    traversalUnlock: 'Root bridges',
    loreHook: 'The planetary nervous system strains under accumulated memory. Wells overflow with forgotten histories.',
    color: '#2d5016',
    position: { x: 200, y: 300 },
    size: 80,
    tier: 'Starter',
    progressionOrder: 1
  },
  {
    id: 'verdaniel',
    name: 'Verdaniel',
    levelRange: '15-35',
    minLevel: 15,
    maxLevel: 35,
    hub: 'Memory Wastes',
    traversalUnlock: 'Seed Sigil',
    loreHook: 'Ancient seed vaults hold the memories of extinct civilizations, waiting to bloom again.',
    color: '#4a7c2f',
    position: { x: 350, y: 250 },
    size: 90,
    tier: 'Mid-Level',
    progressionOrder: 2
  },
  {
    id: 'vael',
    name: 'Vael',
    levelRange: '25-45',
    minLevel: 25,
    maxLevel: 45,
    hub: 'Vharos Warfront',
    traversalUnlock: 'Totem Shard/Wind lanes',
    loreHook: 'Fractured totem spirits cry out across the wilderness. The shattered god\'s fragments seek unity.',
    color: '#8b4513',
    position: { x: 500, y: 300 },
    size: 90,
    tier: 'Mid-Level',
    progressionOrder: 3
  },
  {
    id: 'korrath',
    name: 'Korrath',
    levelRange: '35-60',
    minLevel: 35,
    maxLevel: 60,
    hub: 'Scarred Highlands',
    traversalUnlock: 'Banner Commendation',
    loreHook: 'The warfront never sleeps. God-eaters march beneath banners soaked in divine blood.',
    color: '#8b0000',
    position: { x: 400, y: 450 },
    size: 95,
    tier: 'High-Level',
    progressionOrder: 4
  },
  {
    id: 'nereth',
    name: 'Nereth',
    levelRange: '35-60',
    minLevel: 35,
    maxLevel: 60,
    hub: 'Temporal Maze',
    traversalUnlock: 'Veil Token',
    loreHook: 'The veil between life and death tears. A broken god of passage leaves undeath in its wake.',
    color: '#2f2f4f',
    position: { x: 250, y: 450 },
    size: 95,
    tier: 'High-Level',
    progressionOrder: 5
  },
  {
    id: 'astrae',
    name: 'Astrae',
    levelRange: '50-65',
    minLevel: 50,
    maxLevel: 65,
    hub: 'The Isle of Giants',
    traversalUnlock: 'Star Prism',
    loreHook: 'Missing observatories drift through frozen time. The god of knowledge is trapped, watching eternally.',
    color: '#4169e1',
    position: { x: 600, y: 200 },
    size: 85,
    tier: 'Endgame',
    progressionOrder: 6
  },
  {
    id: 'null-scars',
    name: 'Null Scars',
    levelRange: '60+',
    minLevel: 60,
    maxLevel: 70,
    hub: 'The Null Threshold',
    traversalUnlock: 'Null Anchor Kit',
    loreHook: 'Where reality meets erasure. Anchors prevent the void from consuming memory itself.',
    color: '#1a1a2e',
    position: { x: 700, y: 350 },
    size: 100,
    tier: 'Endgame',
    progressionOrder: 7
  }
];

const progressionPaths = [
  { from: 'worldroot', to: 'verdaniel' },
  { from: 'worldroot', to: 'vael' },
  { from: 'verdaniel', to: 'korrath' },
  { from: 'verdaniel', to: 'nereth' },
  { from: 'vael', to: 'korrath' },
  { from: 'vael', to: 'nereth' },
  { from: 'korrath', to: 'astrae' },
  { from: 'nereth', to: 'astrae' },
  { from: 'astrae', to: 'null-scars' }
];

interface StarterZone {
  race: string;
  zone: string;
  region: string;
  faction: string;
  color: string;
  position: { x: number; y: number };
  cardinal: string;
}

// Starter zones represent specific starting locations for each race.
// Note: Some zones are sub-locations or adjacent areas not shown as main progression
// regions on the map (e.g., Central Territories, combined region boundaries).
// This aligns with world_map_design.md canonical mappings.
const starterZones: StarterZone[] = [
  { 
    race: 'Sylvaen', 
    zone: 'Thornveil Enclave', 
    region: 'Worldroot',
    faction: 'Verdant Circles',
    color: '#4a7c2f',
    position: { x: 120, y: 300 },
    cardinal: 'West'
  },
  { 
    race: 'High Elves (Aelthar)', 
    zone: 'Temporal Steppes', 
    region: 'Verdaniel',
    faction: 'Ascendant League',
    color: '#6b4fa8',
    position: { x: 350, y: 150 },
    cardinal: 'North'
  },
  { 
    race: 'Humans', 
    zone: 'Borderkeep', 
    region: 'Central Territories',
    faction: 'United Kingdoms',
    color: '#4169e1',
    position: { x: 300, y: 280 },
    cardinal: 'Central'
  },
  { 
    race: 'Therakai – Wildborn', 
    zone: 'The Untamed Reaches', 
    region: 'Vael',
    faction: 'Wildborn Clans',
    color: '#d2691e',
    position: { x: 500, y: 260 },
    cardinal: 'East'
  },
  { 
    race: 'Therakai – Pathbound', 
    zone: 'The Carved Valleys', 
    region: 'Vael',
    faction: 'Pathbound Clans',
    color: '#cd853f',
    position: { x: 520, y: 320 },
    cardinal: 'East'
  },
  { 
    race: 'Gronnak', 
    zone: 'The Scarred Highlands', 
    region: 'Korrath/Null Scars',
    faction: 'Dominion Warhost',
    color: '#8b0000',
    position: { x: 450, y: 480 },
    cardinal: 'South'
  },
  { 
    race: 'Void-Touched', 
    zone: 'Blackwake Haven', 
    region: 'Nereth',
    faction: 'Void Compact',
    color: '#483d8b',
    position: { x: 620, y: 420 },
    cardinal: 'Southeast'
  }
];

type TierFilter = 'Starter' | 'Tier 1-2' | 'Tier 3-4' | 'Endgame';

const EldaraWorldMap: React.FC = () => {
  const [selectedRegion, setSelectedRegion] = useState<string | null>(null);
  const [hoveredRegion, setHoveredRegion] = useState<string | null>(null);
  const [hoveredStarter, setHoveredStarter] = useState<string | null>(null);
  const [activeTierFilters, setActiveTierFilters] = useState<Set<TierFilter>>(
    new Set(['Starter', 'Tier 1-2', 'Tier 3-4', 'Endgame'])
  );

  const getTierFromRegion = (region: Region): TierFilter => {
    if (region.tier === 'Starter') return 'Starter';
    if (region.minLevel >= 1 && region.maxLevel <= 35) return 'Tier 1-2';
    if (region.minLevel >= 25 && region.maxLevel <= 60) return 'Tier 3-4';
    return 'Endgame';
  };

  const isRegionVisible = (region: Region): boolean => {
    const tier = getTierFromRegion(region);
    return activeTierFilters.has(tier);
  };

  const toggleTierFilter = (tier: TierFilter) => {
    const newFilters = new Set(activeTierFilters);
    if (newFilters.has(tier)) {
      newFilters.delete(tier);
    } else {
      newFilters.add(tier);
    }
    setActiveTierFilters(newFilters);
  };

  const getRegionOpacity = (regionId: string): number => {
    const region = regions.find(r => r.id === regionId);
    if (!region) return 0.3;
    
    const isVisible = isRegionVisible(region);
    if (!isVisible) return 0.2;
    
    if (!selectedRegion) return 1;
    return regionId === selectedRegion ? 1 : 0.3;
  };

  const getRegionSaturation = (regionId: string): string => {
    const region = regions.find(r => r.id === regionId);
    if (!region) return 'saturate(30%)';
    
    const isVisible = isRegionVisible(region);
    if (!isVisible) return 'saturate(20%)';
    
    if (!selectedRegion) return 'saturate(100%)';
    return regionId === selectedRegion ? 'saturate(100%)' : 'saturate(30%)';
  };

  const handleRegionClick = (regionId: string) => {
    setSelectedRegion(selectedRegion === regionId ? null : regionId);
  };

  return (
    <div className="eldara-world-map" style={{ 
      width: '100%', 
      maxWidth: '1400px', 
      margin: '0 auto',
      padding: '20px',
      backgroundColor: '#0a0e1a',
      borderRadius: '8px',
      fontFamily: 'system-ui, -apple-system, sans-serif',
      display: 'grid',
      gridTemplateColumns: '1fr 300px',
      gap: '20px'
    }}>
      {/* Main Map Area */}
      <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
        <div style={{ textAlign: 'center' }}>
          <h2 style={{ color: '#e0e0e0', marginBottom: '8px', fontSize: '24px' }}>
            World of Eldara - Progression Map
          </h2>
          <p style={{ color: '#a0a0a0', fontSize: '14px' }}>
            Click a region to focus. Hover for details. Use filters to highlight tiers.
          </p>
        </div>

        {/* Tier Filter Controls */}
        <div style={{
          padding: '16px',
          backgroundColor: '#1a2030',
          border: '2px solid #4a5060',
          borderRadius: '6px',
        }}>
          <h3 style={{ 
            margin: '0 0 12px 0', 
            color: '#ffd700', 
            fontSize: '16px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            <span>🎯</span> Tier Filters
          </h3>
          <div style={{ 
            display: 'flex', 
            gap: '8px',
            flexWrap: 'wrap'
          }}>
            {(['Starter', 'Tier 1-2', 'Tier 3-4', 'Endgame'] as TierFilter[]).map((tier) => {
              const isActive = activeTierFilters.has(tier);
              return (
                <button
                  key={tier}
                  onClick={() => toggleTierFilter(tier)}
                  style={{
                    padding: '8px 16px',
                    backgroundColor: isActive ? '#4a7c2f' : '#2a3040',
                    color: isActive ? '#ffffff' : '#888888',
                    border: isActive ? '2px solid #6a9c4f' : '2px solid #3a4050',
                    borderRadius: '20px',
                    fontSize: '13px',
                    fontWeight: isActive ? 'bold' : 'normal',
                    cursor: 'pointer',
                    transition: 'all 0.2s ease',
                    outline: 'none'
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.transform = 'translateY(-2px)';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.transform = 'translateY(0)';
                  }}
                >
                  {tier}
                </button>
              );
            })}
          </div>
          <p style={{ 
            marginTop: '8px', 
            fontSize: '11px', 
            color: '#888888',
            fontStyle: 'italic' 
          }}>
            Click to toggle visibility. Non-selected categories will dim.
          </p>
        </div>

        {/* Spatial Hint */}
        <div style={{
          padding: '12px',
          backgroundColor: '#1a2030',
          border: '1px solid #4a5060',
          borderRadius: '6px',
          fontSize: '12px',
          color: '#a0a0a0'
        }}>
          <strong style={{ color: '#ffd700' }}>🧭 Cardinal Layout:</strong> Thornveil (W) · Temporal Expanse (N) · Human Kingdoms (C) · Vharos (E) · Krag'Thuun (S) · Shattered Isles (SE)
        </div>

      <svg 
        width="900" 
        height="600" 
        viewBox="0 0 900 600"
        style={{ 
          background: 'linear-gradient(135deg, #0f1419 0%, #1a1f2e 100%)',
          borderRadius: '4px',
          border: '1px solid #2a3040',
          width: '100%',
          height: 'auto'
        }}
      >
        {/* Draw progression paths from starter zones to Worldroot (subtle) */}
        <g opacity="0.25">
          {starterZones.map((starter, index) => {
            const worldrootRegion = regions.find(r => r.id === 'worldroot');
            if (!worldrootRegion) return null;
            
            return (
              <line
                key={`starter-path-${index}`}
                x1={starter.position.x}
                y1={starter.position.y}
                x2={worldrootRegion.position.x}
                y2={worldrootRegion.position.y}
                stroke={starter.color}
                strokeWidth="2"
                strokeDasharray="3,6"
                opacity={activeTierFilters.has('Starter') ? 0.4 : 0.1}
              />
            );
          })}
        </g>

        {/* Draw progression paths between regions */}
        <g>
          {progressionPaths.map((path, index) => {
            const fromRegion = regions.find(r => r.id === path.from);
            const toRegion = regions.find(r => r.id === path.to);
            if (!fromRegion || !toRegion) return null;

            const fromVisible = isRegionVisible(fromRegion);
            const toVisible = isRegionVisible(toRegion);
            const pathOpacity = fromVisible && toVisible ? 0.5 : 0.15;

            return (
              <line
                key={`path-${index}`}
                x1={fromRegion.position.x}
                y1={fromRegion.position.y}
                x2={toRegion.position.x}
                y2={toRegion.position.y}
                stroke="#3a4050"
                strokeWidth="2"
                strokeDasharray="5,5"
                opacity={
                  selectedRegion 
                    ? (path.from === selectedRegion || path.to === selectedRegion ? 0.8 : 0.2)
                    : pathOpacity
                }
              />
            );
          })}
        </g>

        {/* Draw starter zone pins */}
        {starterZones.map((starter) => {
          const isHovered = hoveredStarter === starter.race;
          const isStarterVisible = activeTierFilters.has('Starter');
          
          return (
            <g key={`starter-${starter.race}`}>
              {/* Pin marker */}
              <g
                onMouseEnter={() => setHoveredStarter(starter.race)}
                onMouseLeave={() => setHoveredStarter(null)}
                style={{ cursor: 'pointer' }}
                opacity={isStarterVisible ? 1 : 0.2}
              >
                {/* Pin shape */}
                <path
                  d={`M ${starter.position.x} ${starter.position.y - 20} 
                      C ${starter.position.x - 8} ${starter.position.y - 20}, 
                        ${starter.position.x - 12} ${starter.position.y - 16}, 
                        ${starter.position.x - 12} ${starter.position.y - 12} 
                      C ${starter.position.x - 12} ${starter.position.y - 4}, 
                        ${starter.position.x} ${starter.position.y}, 
                        ${starter.position.x} ${starter.position.y} 
                      C ${starter.position.x} ${starter.position.y}, 
                        ${starter.position.x + 12} ${starter.position.y - 4}, 
                        ${starter.position.x + 12} ${starter.position.y - 12} 
                      C ${starter.position.x + 12} ${starter.position.y - 16}, 
                        ${starter.position.x + 8} ${starter.position.y - 20}, 
                        ${starter.position.x} ${starter.position.y - 20} Z`}
                  fill={starter.color}
                  stroke={isHovered ? '#ffd700' : '#2a3040'}
                  strokeWidth={isHovered ? 2 : 1}
                />
                {/* Pin center dot */}
                <circle
                  cx={starter.position.x}
                  cy={starter.position.y - 12}
                  r="4"
                  fill="#ffffff"
                />
              </g>

              {/* Tooltip on hover */}
              {isHovered && (
                <g>
                  <rect
                    x={starter.position.x + 15}
                    y={starter.position.y - 50}
                    width="200"
                    height="80"
                    fill="#1a2030"
                    stroke="#4a5060"
                    strokeWidth="2"
                    rx="6"
                    opacity="0.95"
                  />
                  <text
                    x={starter.position.x + 25}
                    y={starter.position.y - 30}
                    fill="#ffd700"
                    fontSize="12"
                    fontWeight="bold"
                  >
                    {starter.race}
                  </text>
                  <text
                    x={starter.position.x + 25}
                    y={starter.position.y - 15}
                    fill="#cccccc"
                    fontSize="10"
                  >
                    📍 {starter.zone}
                  </text>
                  <text
                    x={starter.position.x + 25}
                    y={starter.position.y}
                    fill="#a0e0ff"
                    fontSize="10"
                  >
                    Faction: {starter.faction}
                  </text>
                  <text
                    x={starter.position.x + 25}
                    y={starter.position.y + 15}
                    fill="#888888"
                    fontSize="9"
                    fontStyle="italic"
                  >
                    Levels 1-10 · {starter.cardinal}
                  </text>
                </g>
              )}
            </g>
          );
        })}

        {/* Worldroot shared hub star marker */}
        {(() => {
          const worldroot = regions.find(r => r.id === 'worldroot');
          if (!worldroot) return null;
          
          return (
            <g key="worldroot-star">
              <polygon
                points={`
                  ${worldroot.position.x},${worldroot.position.y - 15}
                  ${worldroot.position.x + 4},${worldroot.position.y - 5}
                  ${worldroot.position.x + 14},${worldroot.position.y - 5}
                  ${worldroot.position.x + 6},${worldroot.position.y + 2}
                  ${worldroot.position.x + 10},${worldroot.position.y + 12}
                  ${worldroot.position.x},${worldroot.position.y + 6}
                  ${worldroot.position.x - 10},${worldroot.position.y + 12}
                  ${worldroot.position.x - 6},${worldroot.position.y + 2}
                  ${worldroot.position.x - 14},${worldroot.position.y - 5}
                  ${worldroot.position.x - 4},${worldroot.position.y - 5}
                `}
                fill="#ffd700"
                stroke="#ffffff"
                strokeWidth="1"
                opacity={activeTierFilters.has('Starter') ? 0.9 : 0.3}
              />
            </g>
          );
        })()}

        {/* Draw regions */}
        {regions.map((region) => {
          const isHovered = hoveredRegion === region.id;
          const isSelected = selectedRegion === region.id;

          return (
            <g key={region.id}>
              {/* Region circle */}
              <circle
                cx={region.position.x}
                cy={region.position.y}
                r={region.size / 2}
                fill={region.color}
                opacity={getRegionOpacity(region.id)}
                stroke={isSelected ? '#ffd700' : isHovered ? '#ffffff' : '#4a5060'}
                strokeWidth={isSelected ? 3 : isHovered ? 2 : 1}
                style={{ 
                  cursor: 'pointer',
                  filter: getRegionSaturation(region.id),
                  transition: 'all 0.3s ease'
                }}
                onClick={() => handleRegionClick(region.id)}
                onMouseEnter={() => setHoveredRegion(region.id)}
                onMouseLeave={() => setHoveredRegion(null)}
              />

              {/* Region name */}
              <text
                x={region.position.x}
                y={region.position.y - 5}
                textAnchor="middle"
                fill="#ffffff"
                fontSize="14"
                fontWeight="bold"
                opacity={getRegionOpacity(region.id)}
                style={{ pointerEvents: 'none', userSelect: 'none' }}
              >
                {region.name}
              </text>

              {/* Level range */}
              <text
                x={region.position.x}
                y={region.position.y + 10}
                textAnchor="middle"
                fill="#cccccc"
                fontSize="12"
                opacity={getRegionOpacity(region.id)}
                style={{ pointerEvents: 'none', userSelect: 'none' }}
              >
                {region.levelRange}
              </text>

              {/* Traversal unlock badge */}
              <g transform={`translate(${region.position.x}, ${region.position.y + region.size / 2 + 15})`}>
                <rect
                  x="-45"
                  y="-10"
                  width="90"
                  height="20"
                  fill="#1a2030"
                  stroke="#4a5060"
                  strokeWidth="1"
                  rx="4"
                  opacity={getRegionOpacity(region.id) * 0.9}
                  style={{ pointerEvents: 'none' }}
                />
                <text
                  x="0"
                  y="4"
                  textAnchor="middle"
                  fill="#ffd700"
                  fontSize="10"
                  opacity={getRegionOpacity(region.id)}
                  style={{ pointerEvents: 'none', userSelect: 'none' }}
                >
                  🔓 {region.traversalUnlock}
                </text>
              </g>

              {/* Tooltip on hover */}
              {isHovered && (
                <g>
                  <rect
                    x={region.position.x + region.size / 2 + 10}
                    y={region.position.y - 60}
                    width="280"
                    height="120"
                    fill="#1a2030"
                    stroke="#4a5060"
                    strokeWidth="2"
                    rx="6"
                    opacity="0.95"
                  />
                  <text
                    x={region.position.x + region.size / 2 + 20}
                    y={region.position.y - 40}
                    fill="#ffd700"
                    fontSize="13"
                    fontWeight="bold"
                  >
                    {region.name} ({region.tier})
                  </text>
                  <text
                    x={region.position.x + region.size / 2 + 20}
                    y={region.position.y - 22}
                    fill="#cccccc"
                    fontSize="11"
                  >
                    Level {region.minLevel}-{region.maxLevel}
                  </text>
                  <text
                    x={region.position.x + region.size / 2 + 20}
                    y={region.position.y - 7}
                    fill="#a0e0ff"
                    fontSize="11"
                  >
                    Hub: {region.hub}
                  </text>
                  <text
                    x={region.position.x + region.size / 2 + 20}
                    y={region.position.y + 8}
                    fill="#ffd700"
                    fontSize="10"
                  >
                    Unlock: {region.traversalUnlock}
                  </text>
                  <foreignObject
                    x={region.position.x + region.size / 2 + 20}
                    y={region.position.y + 15}
                    width="260"
                    height="40"
                  >
                    <div style={{ color: '#b0b0b0', fontSize: '10px', lineHeight: '1.3' }}>
                      {region.loreHook}
                    </div>
                  </foreignObject>
                </g>
              )}
            </g>
          );
        })}

        {/* Legend */}
        <g transform="translate(20, 480)">
          <rect x="0" y="0" width="280" height="110" fill="#1a2030" stroke="#4a5060" strokeWidth="1" rx="4" opacity="0.95" />
          <text x="10" y="18" fill="#ffd700" fontSize="13" fontWeight="bold">Map Legend</text>
          
          {/* Progression paths */}
          <line x1="10" y1="30" x2="40" y2="30" stroke="#3a4050" strokeWidth="2" strokeDasharray="5,5" />
          <text x="45" y="34" fill="#cccccc" fontSize="10">Progression paths</text>
          
          {/* Region marker */}
          <circle cx="20" cy="48" r="8" fill="#4a7c2f" stroke="#4a5060" strokeWidth="1" />
          <text x="35" y="53" fill="#cccccc" fontSize="10">Region (clickable)</text>
          
          {/* Starter pin */}
          <path
            d="M 20 65 C 14 65, 12 68, 12 71 C 12 76, 20 82, 20 82 C 20 82, 28 76, 28 71 C 28 68, 26 65, 20 65 Z"
            fill="#4169e1"
            stroke="#2a3040"
            strokeWidth="1"
          />
          <circle cx="20" cy="71" r="2" fill="#ffffff" />
          <text x="35" y="75" fill="#cccccc" fontSize="10">Starter zone (1-10)</text>
          
          {/* Shared hub star */}
          <polygon
            points="20,85 22,90 27,90 23,93 25,98 20,95 15,98 17,93 13,90 18,90"
            fill="#ffd700"
            stroke="#ffffff"
            strokeWidth="0.5"
          />
          <text x="35" y="95" fill="#cccccc" fontSize="10">Worldroot (shared hub)</text>
        </g>
      </svg>
      </div>

      {/* Sidebar with scrollable sections */}
      <div style={{
        display: 'flex',
        flexDirection: 'column',
        gap: '16px',
        maxHeight: '800px',
        overflowY: 'auto',
        overflowX: 'hidden',
        paddingRight: '8px'
      }}>
        {/* Progression Section */}
        <div style={{
          padding: '16px',
          backgroundColor: '#1a2030',
          border: '2px solid #4a5060',
          borderRadius: '6px',
          color: '#e0e0e0'
        }}>
          <h3 style={{ 
            margin: '0 0 12px 0', 
            color: '#ffd700', 
            fontSize: '16px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            <span>📈</span> Progression
          </h3>
          <div style={{ fontSize: '12px', lineHeight: '1.6', color: '#cccccc' }}>
            <p style={{ marginBottom: '8px' }}>
              Start at your race's starter zone (1-10), then converge at <strong style={{ color: '#ffd700' }}>Worldroot</strong> as the shared early hub.
            </p>
            <p>
              Choose your path through mid-tier zones (15-45), then advance to high-tier regions (35-60) and endgame content (60+).
            </p>
          </div>
        </div>

        {/* Traversal Section */}
        <div style={{
          padding: '16px',
          backgroundColor: '#1a2030',
          border: '2px solid #4a5060',
          borderRadius: '6px',
          color: '#e0e0e0'
        }}>
          <h3 style={{ 
            margin: '0 0 12px 0', 
            color: '#ffd700', 
            fontSize: '16px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            <span>🔓</span> Traversal
          </h3>
          <div style={{ fontSize: '12px', lineHeight: '1.6', color: '#cccccc' }}>
            <p>
              Each region unlocks unique traversal methods. Complete region quests to earn unlock items and access new areas.
            </p>
          </div>
        </div>

        {/* Starter Zones Section */}
        <div style={{
          padding: '16px',
          backgroundColor: '#1a2030',
          border: '2px solid #4a5060',
          borderRadius: '6px',
          color: '#e0e0e0'
        }}>
          <h3 style={{ 
            margin: '0 0 12px 0', 
            color: '#ffd700', 
            fontSize: '16px',
            display: 'flex',
            alignItems: 'center',
            gap: '8px'
          }}>
            <span>🗺️</span> Starter Zones (1-10)
          </h3>
          <div style={{ 
            display: 'flex', 
            flexDirection: 'column',
            gap: '10px',
            fontSize: '12px'
          }}>
            {starterZones.map((starter) => (
              <div 
                key={starter.race}
                style={{
                  padding: '8px 10px',
                  backgroundColor: '#0f1419',
                  borderLeft: `3px solid ${starter.color}`,
                  borderRadius: '3px',
                }}
              >
                <div style={{ color: '#a0e0ff', fontWeight: 'bold', marginBottom: '4px' }}>
                  {starter.race}
                </div>
                <div style={{ color: '#cccccc', fontSize: '11px', marginBottom: '2px' }}>
                  📍 {starter.zone}
                </div>
                <div style={{ color: '#888888', fontSize: '10px', fontStyle: 'italic' }}>
                  {starter.faction} · {starter.cardinal}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Region details panel (shown when region selected) */}
        {selectedRegion && (
          <div style={{
            padding: '16px',
            backgroundColor: '#1a2030',
            border: '2px solid #ffd700',
            borderRadius: '6px',
            color: '#e0e0e0'
          }}>
            {regions
              .filter(r => r.id === selectedRegion)
              .map(region => (
                <div key={region.id}>
                  <h3 style={{ margin: '0 0 12px 0', color: '#ffd700', fontSize: '18px' }}>
                    {region.name}
                  </h3>
                  <div style={{ display: 'grid', gridTemplateColumns: '1fr', gap: '8px', fontSize: '13px' }}>
                    <div>
                      <strong style={{ color: '#a0e0ff' }}>Tier:</strong> {region.tier}
                    </div>
                    <div>
                      <strong style={{ color: '#a0e0ff' }}>Level Range:</strong> {region.levelRange}
                    </div>
                    <div>
                      <strong style={{ color: '#a0e0ff' }}>Hub:</strong> {region.hub}
                    </div>
                    <div>
                      <strong style={{ color: '#a0e0ff' }}>Progression:</strong> Step {region.progressionOrder}
                    </div>
                  </div>
                  <div style={{ marginTop: '12px' }}>
                    <div style={{ 
                      padding: '8px 10px', 
                      backgroundColor: '#0f1419', 
                      borderLeft: '3px solid #ffd700',
                      marginBottom: '8px',
                      borderRadius: '2px'
                    }}>
                      <strong style={{ color: '#ffd700', fontSize: '12px' }}>🔓 Traversal:</strong>
                      <div style={{ marginTop: '4px', color: '#cccccc', fontSize: '11px' }}>{region.traversalUnlock}</div>
                    </div>
                    <div style={{ 
                      padding: '8px 10px', 
                      backgroundColor: '#0f1419', 
                      borderLeft: '3px solid #a0e0ff',
                      borderRadius: '2px'
                    }}>
                      <strong style={{ color: '#a0e0ff', fontSize: '12px' }}>📖 Lore:</strong>
                      <div style={{ marginTop: '4px', color: '#cccccc', lineHeight: '1.4', fontSize: '11px' }}>{region.loreHook}</div>
                    </div>
                  </div>
                </div>
              ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default EldaraWorldMap;
