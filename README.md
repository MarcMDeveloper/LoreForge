# 🌌 LoreForge

**LoreForge** is an AI-driven NPC generator designed to create fully interactive, culturally aware, and multi-modal NPCs for Unity games. This project combines Python data generation with Unity integration, enabling NPCs to interact, remember past actions, and maintain consistent behavior and dialogue.

---

## 📁 Repository Structure

```plaintext
LoreForge/
├── LICENSE
├── README.md
├── Build
├── Echoes of the crowd/   # Unity Project
│   ├── Assets/
│   │   ├── Scripts/
│   │   │   ├── NPC/       # NPC agents, memory, and dialogue
│   │   │   ├── Managers/  # Interaction and dialogue managers
│   │   │   ├── Security/  # Security and authentication
│   │   │   └── Utils/     # Helper scripts
│   │   ├── StreamingAssets/
│   │   │   └── NPC/       # Generated NPC JSONs
│   │   ├── Scenes/
│   │   │   └── Game.unity
│   │   ├── Resources/     # UI, fonts, images
│   │   ├── UI Toolkit/    # UI components
│   │   ├── TextMesh Pro/  # Text rendering
│   │   └── Settings/      # Project settings
├── NPC_Generator/         # Python NPC Generator
│   ├── data_sources/      # Raw datasets
│   │   ├── cultures/      # Cultural norms & behaviors
│   │   ├── names/         # Culturally relevant names
│   │   ├── traits/        # Personality archetypes
│   │   └── misc/          # Extra (professions, factions, etc.)
│   ├── processed_data/    # Cleaned & merged datasets
│   │   └── combined_npc_data.csv
│   ├── output_json/       # Generated NPC JSONs
│   ├── generator.py       # JSON generation script
│   ├── preprocess.py      # Dataset cleaning/merging
│   ├── npc_schema.json    # JSON structure definition
│   └── requirements.txt   # Python dependencies
└── Docs/                  # Documentation & diagrams
