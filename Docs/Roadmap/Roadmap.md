# 📅 LoreForge Development Roadmap

This roadmap outlines the 3-week plan for building the **LoreForge AI NPC System**, from Unity agent creation to the integration of a Python-powered NPC generator. The tasks are as small as possible to mantain an order with the important sections.

---

## **Week 1 – Unity NPC Agent Setup & Dialogue System/ NPC to user**

- ✅ Set up Repo project (`LoreForge`)  
- ✅ Add basic docs and gitignore to the project  
- ✅ Set up Unity project (`Echoes of the crowd`)  
- ✅ Create the different scenes (`Main Menu`, `MainScene`) 

- ✅ Create `NPC` script structure  
- ✅ Define NPC ID, Name, Culture, Traits fields  
- ✅ Create mock up Json with two `NPC`
- ✅ Create NPC constructor for callling it from the NPCs manager and the json loader

- ✅ Set up the singleton managers (`JsonLoader`, `NPCManager`, `DialogueManager`)
- ✅ Implement JSON loader for NPCs
- ✅ Implement `NPC manager` as singleton that can read NPCs JSON  
- ✅ Debug data loading  

- ✅ NPC creation with basic prefab
- ✅ Create the UI to see the the different NPC selection with images
- ✅ Implement the NPCManager to show the different NPCs in the screen
- Implement OnHover function that shows the data
- Set a mock up onPress for future dialogue manager call

- Implement **basic AI agent class**
- Create Agent constructor and a basic update

- Design **dialogue manager** script  
- Connect AI agent to dialogue manager 
- Create the UI to see the text of the NPC dialogue with user
- Implement **player-to-NPC greeting**  

---

## **Week 2 – Enhancing Agent Memory & Behavior NPC to NPC**

- Implement **NPC-to-NPC interaction system**  
- Create the UI to see the text of the NPC dialogue with NPC
- Test multiple agents in scene  
- Debug system prompts for AI dialogue  

- Implement **short-term memory** in NPC agent  
- Track player interactions in runtime  

- Improve **behavior logic** based on personality traits (risk-averse, friendly, etc.)  
- Create simple decision-making examples  

- Integrate AI-generated dialogue dynamically with memory  
- Test dialogue adapting to player actions  

- Debug and polish NPC interactions  
- Optimize agent update loops and dialogue triggers  

---

## **Week 3 – Random NPC Generator & Integration**

- Set up Python `NPC_Generator` project  
- Create folder structure for datasets: `cultures`, `names`, `traits`, `misc`  

- Implement **data preprocessing script**  
- Merge datasets into `combined_npc_data.csv`  

- Implement **generator.py** to produce NPC JSONs from CSV  
- Ensure JSON matches Unity schema  

- Test generated NPCs in Unity  
- Load multiple generated NPCs into scene  
- Validate system prompt correctness  

- Polish Python generator  
- Document pipeline from dataset → CSV → JSON → Unity  
- Prepare final demo scene  

---

## *Next Steps - In case there is time*

- Music changes depending of the tone of conversation (positive/negative)
- Main Menu detects if there is money in OPEN AI before allowing to continue the game
- Saves data from user as jsons so works when returns

### 📝 Notes
- **Weeks 1–2** focus on Unity-based agent creation, dialogue, and behavior.
- **Week 3** focuses on Python-based random NPC generation.