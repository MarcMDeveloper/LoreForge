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
- ✅ Implement JSON loader for NPCs, static
- ✅ Implement `NPC manager` as singleton that can read NPCs JSON  
- ✅ Debug data loading  

- ✅ NPC creation with basic prefab
- ✅ Create the UI to see the the different NPC selection with images
- ✅ Implement the NPCManager to show the different NPCs in the screen
- ✅ Set up the prefab for showing data
- ✅ Implement OnHover function that shows the data
- ✅ Set a mock up onPress for future dialogue manager call

- ✅ Create the Base Chat Canvas and add a Scroll View for Messages
- ✅ Create Message Prefabs
- ✅ Add the Input Bar, Set Anchors & Auto-Layout and Sorting & Visibility

- ✅ Set next input as function to send answer and show the text
- ✅ Create Agent constructor and a basic update separate in functions
- ✅ Implement the different functions
- ✅ Improve **behavior logic** based on personality traits (risk-averse, friendly, etc.)  


---

## **Week 2 – Enhancing Agent Memory & Behavior NPC to NPC**

- ✅ Design **dialogue manager** script  
- ✅ Connect AI agent to dialogue manager 
- ✅ Track player interactions in runtime 
- ✅ Implement **short-term memory** in NPC agent  
- ✅ Test dialogue adapting to player actions 

- ✅ When agent start chat set previous summary to system prompt
- ✅ When close a conversation save as summary
- ✅ When open a conversation already started show summary of previous

- ✅ Implement **NPC-to-NPC interaction system**  
- ✅ Set start the conversation of the two NPC
- ✅ Choose randomly two NPC to speak
- ✅ Save the dialogue when finish
- ✅ Load and show the dialogue

- ✅ **Design + art time**
- ✅ Refactor UI NPC to be in a grid and put some art on it
- ✅ Add some art to the NPC prefab
- ✅ Refactor `JsonLoader`, `NPCManager`, `NPC`
- ✅ Refactor `UITooltip` art, refactor + design

- ✅ Refactor dialogues UI and put some art
- ✅ Add some art on the bubble chat prefab
- ✅ Add some art to the chat canvas
- ✅ Add some art on the dialogues view chat


---

## **Week 3 – Random NPC Generator & Integration**

- ✅ Set up Python `NPC_Generator` project  
- ✅ Create folder structure for datasets: `cultures`, `names`, `traits`, `misc`  

- Implement **data preprocessing script**  
- Merge datasets into `combined_npc_data.csv`  

- Implement **generator.py** to produce NPC JSONs from CSV  
- Ensure JSON matches Unity schema  

- ✅ Test generated NPCs in Unity  
- ✅ Load multiple generated NPCs into scene  
- ✅ Validate system prompt correctness  

- Polish Python generator  
- Prepare final demo scene  
- Prepare presentation

---

## *Next Steps - In case there is time*

- Music changes depending of the tone of conversation (positive/negative)
- Main Menu detects if there is money in OPEN AI before allowing to continue the game
- Saves data from user as jsons so works when returns
- ✅ Search and implement some art
- Load json an images from drive with specific links to avoid using the same of the build.
- ✅ Look security aki key

### 📝 Notes
- **Weeks 1–2** focus on Unity-based agent creation, dialogue, and behavior.
- **Week 3** focuses on Python-based random NPC generation.