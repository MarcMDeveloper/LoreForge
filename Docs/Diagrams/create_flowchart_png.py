import matplotlib.pyplot as plt
import matplotlib.patches as patches
from matplotlib.patches import FancyBboxPatch, ConnectionPatch
import numpy as np

def create_npc_architecture_flowchart():
    # Create figure and axis
    fig, ax = plt.subplots(1, 1, figsize=(16, 12))
    ax.set_xlim(0, 10)
    ax.set_ylim(0, 12)
    ax.axis('off')
    
    # Define colors
    colors = {
        'data': '#E8F4FD',
        'processing': '#FFF2CC',
        'unity': '#E1D5E7',
        'api': '#D5E8D4',
        'interaction': '#FFE6CC'
    }
    
    # Define node positions
    nodes = {
        'A': (1, 10),    # Data Sources
        'B': (3, 10),    # Preprocessing
        'C': (5, 10),    # NPC Generator
        'D': (7, 10),    # Unity: Echoes of the Crowd
        'D1': (5, 8),    # NPC Loader
        'D2': (7, 8),    # NPC Agents
        'D3': (9, 8),    # Behavior Controller
        'D4': (3, 8),    # Dialogue Manager
        'E': (1, 6),     # Backend LLM API
        'F': (5, 6),     # Player & NPC Interactions
        'F2': (7, 6)     # NPC ↔ NPC Interactions
    }
    
    # Define node texts
    node_texts = {
        'A': 'Data Sources\n(cultures, names,\ntraits, misc)',
        'B': 'Preprocessing\nMerge → CSV',
        'C': 'NPC Generator\nCSV → JSON',
        'D': 'Unity:\nEchoes of the Crowd',
        'D1': 'NPC Loader',
        'D2': 'NPC Agents\nMemory, Traits,\nRelationships',
        'D3': 'Behavior\nController',
        'D4': 'Dialogue\nManager',
        'E': 'Backend LLM API\n(GPT Model)',
        'F': 'Player &\nNPC Interactions',
        'F2': 'NPC ↔ NPC\nInteractions\n(via GPT API)'
    }
    
    # Draw nodes
    for node_id, (x, y) in nodes.items():
        # Determine color based on node type
        if node_id in ['A']:
            color = colors['data']
        elif node_id in ['B', 'C']:
            color = colors['processing']
        elif node_id in ['D', 'D1', 'D2', 'D3', 'D4']:
            color = colors['unity']
        elif node_id in ['E']:
            color = colors['api']
        else:
            color = colors['interaction']
        
        # Create rounded rectangle
        box = FancyBboxPatch((x-0.8, y-0.6), 1.6, 1.2,
                           boxstyle="round,pad=0.1",
                           facecolor=color,
                           edgecolor='black',
                           linewidth=2)
        ax.add_patch(box)
        
        # Add text
        ax.text(x, y, node_texts[node_id], ha='center', va='center',
               fontsize=9, fontweight='bold', wrap=True)
    
    # Draw connections
    connections = [
        ('A', 'B', ''),
        ('B', 'C', ''),
        ('C', 'D', ''),
        ('D', 'D1', ''),
        ('D', 'D2', ''),
        ('D', 'D3', ''),
        ('D', 'D4', ''),
        ('D4', 'E', 'Send context'),
        ('E', 'D4', 'Return text'),
        ('D4', 'F', ''),
        ('D2', 'F', ''),
        ('D2', 'F2', '')
    ]
    
    for start, end, label in connections:
        x1, y1 = nodes[start]
        x2, y2 = nodes[end]
        
        # Adjust connection points to edge of boxes
        if x1 < x2:  # Right arrow
            x1 += 0.8
            x2 -= 0.8
        elif x1 > x2:  # Left arrow
            x1 -= 0.8
            x2 += 0.8
        elif y1 > y2:  # Down arrow
            y1 -= 0.6
            y2 += 0.6
        elif y1 < y2:  # Up arrow
            y1 += 0.6
            y2 -= 0.6
        
        # Draw arrow
        arrow = ConnectionPatch((x1, y1), (x2, y2), "data", "data",
                              arrowstyle="->", shrinkA=5, shrinkB=5,
                              mutation_scale=20, fc="black", ec="black", linewidth=2)
        ax.add_patch(arrow)
        
        # Add label if present
        if label:
            mid_x = (x1 + x2) / 2
            mid_y = (y1 + y2) / 2
            ax.text(mid_x, mid_y, label, ha='center', va='center',
                   fontsize=8, bbox=dict(boxstyle="round,pad=0.2", facecolor='white', alpha=0.8))
    
    # Add title
    ax.text(5, 11.5, 'NPC System Architecture: Echoes of the Crowd', 
           ha='center', va='center', fontsize=16, fontweight='bold')
    
    # Add legend
    legend_elements = [
        patches.Patch(color=colors['data'], label='Data Sources'),
        patches.Patch(color=colors['processing'], label='Processing'),
        patches.Patch(color=colors['unity'], label='Unity Components'),
        patches.Patch(color=colors['api'], label='External API'),
        patches.Patch(color=colors['interaction'], label='Interactions')
    ]
    ax.legend(handles=legend_elements, loc='upper left', bbox_to_anchor=(0, 0.95))
    
    plt.tight_layout()
    return fig

if __name__ == "__main__":
    # Create the flowchart
    fig = create_npc_architecture_flowchart()
    
    # Save as PNG
    plt.savefig('NPC_System_Architecture.png', dpi=300, bbox_inches='tight', 
                facecolor='white', edgecolor='none')
    print("Flowchart saved as 'NPC_System_Architecture.png'")
    
    # Show the plot
    plt.show()
