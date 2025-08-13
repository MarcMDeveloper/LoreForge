# NPC System Architecture: Echoes of the Crowd

```mermaid
flowchart TD
    A[Data Sources\n(cultures, names, traits, misc)] --> B[Preprocessing\nMerge → CSV]
    B --> C[NPC Generator\nCSV → JSON]
    C --> D[Unity: Echoes of the Crowd]
    
    D --> D1[NPC Loader]
    D --> D2[NPC Agents\nMemory, Traits, Relationships]
    D --> D3[Behavior Controller]
    D --> D4[Dialogue Manager]

    D4 -->|Send context| E[Backend LLM API\n(GPT Model)]
    E -->|Return text| D4

    D4 --> F[Player & NPC Interactions]
    D2 --> F
    D2 --> F2[NPC ↔ NPC Interactions\n(via GPT API)]
