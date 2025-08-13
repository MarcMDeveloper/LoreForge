AI-NPC-Generator/
│
├── LICENSE
├── README.md
│
├── Unity/                                  # Unity project
│   ├── Assets/
│   │   ├── Scripts/                        # Unity C# scripts
│   │   │   ├── NPC/                        # NPC agent, dialogue, memory scripts
│   │   │   ├── Managers/                   # Interaction, Dialogue managers
│   │   │   └── Utils/                      # Helper scripts
│   │   │
│   │   ├── StreamingAssets/
│   │   │   └── NPCs/                        # Generated JSONs bundled in build
│   │   │
│   │   ├── Scenes/
│   │   │   └── MainScene.unity
│   │   │
│   │   └── Resources/                       # UI, images, fonts
│   │
│   ├── ProjectSettings/
│   └── Packages/
│
├── NPC_Generator/                           # Python NPC generator
│   ├── data_sources/                        # Raw datasets
│   │   ├── cultures/                        # Cultural datasets
│   │   ├── names/                           # Names datasets
│   │   ├── traits/                          # Personality/trait datasets
│   │   └── misc/                            # Any extra data sources
│   │
│   ├── processed_data/                      # Cleaned CSVs from preprocessing
│   │   ├── combined_npc_data.csv            # Final merged dataset
│   │
│   ├── output_json/                         # Final generated NPC JSONs
│   │   ├── npc_001.json
│   │   ├── npc_002.json
│   │   └── ...
│   │
│   ├── generator.py                         # Main generator script
│   ├── preprocess.py                        # Data cleaning & merging into CSV
│   ├── npc_schema.json                      # JSON structure definition
│   ├── README.md                            # Instructions for the generator
│   └── requirements.txt                     # Python dependencies
│
└── Docs/                                    # Documentation & diagrams
    ├── architecture_diagram.png
    ├── npc_interaction_diagram.png
    └── design_notes.md
