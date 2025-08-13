# NPC System Architecture: Echoes of the Crowd

```mermaid
flowchart TD
    A[Data Sources<br>(cultures, names, traits, misc)] --> B[Preprocessing<br>Merge → CSV]
    B --> C[NPC Generator<br>CSV → JSON]
    C --> D[Unity: Echoes of the Crowd]
    
    D --> D1[NPC Loader]
    D --> D2[NPC Agents<br>Memory, Traits, Relationships]
    D --> D3[Behavior Controller]
    D --> D4[Dialogue Manager]

    D4 -->|Send context| E[Backend LLM API<br>(GPT Model)]
    E -->|Return text| D4

    D4 --> F[Player & NPC Interactions]
    D2 --> F
    D2 --> F2[NPC ↔ NPC Interactions<br>(via GPT API)]
