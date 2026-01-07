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
}

const starterZones: StarterZone[] = [
  { race: 'Sylvaen', zone: 'Thornveil Enclave', region: 'Worldroot' },
  { race: 'High Elves (Aelthar)', zone: 'Temporal Steppes', region: 'Verdaniel' },
  { race: 'Humans', zone: 'Borderkeep', region: 'Central Territories' },
  { race: 'Therakai – Wildborn', zone: 'The Untamed Reaches', region: 'Vael' },
  { race: 'Therakai – Pathbound', zone: 'The Carved Valleys', region: 'Vael' },
  { race: 'Gronnak', zone: 'The Scarred Highlands', region: 'Korrath/Null Scars' },
  { race: 'Void-Touched', zone: 'Blackwake Haven', region: 'Nereth' }
];

const EldaraWorldMap: React.FC = () => {
  const [selectedRegion, setSelectedRegion] = useState<string | null>(null);
  const [hoveredRegion, setHoveredRegion] = useState<string | null>(null);

  const getRegionOpacity = (regionId: string): number => {
    if (!selectedRegion) return 1;
    return regionId === selectedRegion ? 1 : 0.3;
  };

  const getRegionSaturation = (regionId: string): string => {
    if (!selectedRegion) return 'saturate(100%)';
    return regionId === selectedRegion ? 'saturate(100%)' : 'saturate(30%)';
  };

  const handleRegionClick = (regionId: string) => {
    setSelectedRegion(selectedRegion === regionId ? null : regionId);
  };

  return (
    <div className="eldara-world-map" style={{ 
      width: '100%', 
      maxWidth: '1200px', 
      margin: '0 auto',
      padding: '20px',
      backgroundColor: '#0a0e1a',
      borderRadius: '8px',
      fontFamily: 'system-ui, -apple-system, sans-serif'
    }}>
      <div style={{ marginBottom: '20px', textAlign: 'center' }}>
        <h2 style={{ color: '#e0e0e0', marginBottom: '8px', fontSize: '24px' }}>
          World of Eldara - Progression Map
        </h2>
        <p style={{ color: '#a0a0a0', fontSize: '14px' }}>
          Click a region to focus. Hover for details.
        </p>
      </div>

      {/* Starter Zones Reference Panel */}
      <div style={{
        marginBottom: '20px',
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
          <span>🗺️</span> Starter Zones (Levels 1-10, Faction-Aligned)
        </h3>
        <div style={{ 
          display: 'grid', 
          gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))', 
          gap: '12px',
          fontSize: '13px'
        }}>
          {starterZones.map((starter, index) => (
            <div 
              key={index}
              style={{
                padding: '8px 12px',
                backgroundColor: '#0f1419',
                borderLeft: '3px solid #4a7c2f',
                borderRadius: '3px',
                display: 'flex',
                flexDirection: 'column',
                gap: '4px'
              }}
            >
              <div style={{ color: '#a0e0ff', fontWeight: 'bold' }}>
                {starter.race}
              </div>
              <div style={{ color: '#cccccc', fontSize: '12px' }}>
                📍 {starter.zone}
              </div>
              <div style={{ color: '#888888', fontSize: '11px', fontStyle: 'italic' }}>
                Region: {starter.region}
              </div>
            </div>
          ))}
        </div>
      </div>

      <svg 
        width="900" 
        height="600" 
        viewBox="0 0 900 600"
        style={{ 
          background: 'linear-gradient(135deg, #0f1419 0%, #1a1f2e 100%)',
          borderRadius: '4px',
          border: '1px solid #2a3040'
        }}
      >
        {/* Draw progression paths */}
        <g>
          {progressionPaths.map((path, index) => {
            const fromRegion = regions.find(r => r.id === path.from);
            const toRegion = regions.find(r => r.id === path.to);
            if (!fromRegion || !toRegion) return null;

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
                    : 0.5
                }
              />
            );
          })}
        </g>

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
        <g transform="translate(20, 520)">
          <rect x="0" y="0" width="200" height="70" fill="#1a2030" stroke="#4a5060" strokeWidth="1" rx="4" opacity="0.9" />
          <text x="10" y="20" fill="#ffd700" fontSize="12" fontWeight="bold">Progression Legend</text>
          <line x1="10" y1="30" x2="40" y2="30" stroke="#3a4050" strokeWidth="2" strokeDasharray="5,5" />
          <text x="45" y="34" fill="#cccccc" fontSize="10">Parallel paths</text>
          <circle cx="20" cy="50" r="8" fill="#4a7c2f" stroke="#4a5060" strokeWidth="1" />
          <text x="35" y="55" fill="#cccccc" fontSize="10">Region (clickable)</text>
        </g>
      </svg>

      {/* Region details panel */}
      {selectedRegion && (
        <div style={{
          marginTop: '20px',
          padding: '20px',
          backgroundColor: '#1a2030',
          border: '2px solid #4a5060',
          borderRadius: '6px',
          color: '#e0e0e0'
        }}>
          {regions
            .filter(r => r.id === selectedRegion)
            .map(region => (
              <div key={region.id}>
                <h3 style={{ margin: '0 0 12px 0', color: '#ffd700', fontSize: '20px' }}>
                  {region.name}
                </h3>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '12px', fontSize: '14px' }}>
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
                <div style={{ marginTop: '16px' }}>
                  <div style={{ 
                    padding: '8px 12px', 
                    backgroundColor: '#0f1419', 
                    borderLeft: '3px solid #ffd700',
                    marginBottom: '12px',
                    borderRadius: '2px'
                  }}>
                    <strong style={{ color: '#ffd700' }}>🔓 Traversal Unlock:</strong>
                    <div style={{ marginTop: '4px', color: '#cccccc' }}>{region.traversalUnlock}</div>
                  </div>
                  <div style={{ 
                    padding: '8px 12px', 
                    backgroundColor: '#0f1419', 
                    borderLeft: '3px solid #a0e0ff',
                    borderRadius: '2px'
                  }}>
                    <strong style={{ color: '#a0e0ff' }}>📖 Lore:</strong>
                    <div style={{ marginTop: '4px', color: '#cccccc', lineHeight: '1.5' }}>{region.loreHook}</div>
                  </div>
                </div>
              </div>
            ))}
        </div>
      )}
    </div>
  );
};

export default EldaraWorldMap;
