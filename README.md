LoreForge/

│

├── LICENSE

├── README.md

│

├── Unity/                                  # Unity Project

│   ├── Assets/

│   │   ├── Scripts/

│   │   │   ├── NPC/                        # NPC agents, memory, and dialogue

│   │   │   ├── Managers/                   # Interaction and dialogue managers

│   │   │   └── Utils/                      # Helper scripts

│   │   │

│   │   ├── StreamingAssets/

│   │   │   └── NPCs/                       # Generated JSONs for WebGL/Runtime

│   │   ├── Scenes/

│   │   │   └── MainScene.unity

│   │   └── Resources/                      # UI, fonts, images

│

├── NPC_Generator/                           # Python NPC Generator

│   ├── data_sources/                        # Raw datasets

│   │   ├── cultures/                        # Cultural norms & behaviors

│   │   ├── names/                           # Culturally relevant names

│   │   ├── traits/                          # Personality archetypes

│   │   └── misc/                            # Extra (professions, factions, etc.)

│   │

│   ├── processed_data/                      # Cleaned & merged datasets

│   │   └── combined_npc_data.csv

│   │

│   ├── output_json/                         # Generated NPC JSONs

│   ├── generator.py                         # JSON generation script

│   ├── preprocess.py                        # Dataset cleaning/merging

│   ├── npc_schema.json                      # JSON structure definition

│   └── requirements.txt                     # Python dependencies

│

└── Docs/                                    # Documentation & diagrams
