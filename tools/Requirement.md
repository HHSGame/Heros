# **ASCII Tilemap Editor Requirements**

## **Introduction**

This document outlines the features and specifications for the ASCII Tilemap Editor, a tool designed to enable game designers to create and manipulate tile-based maps using ASCII characters. The editor supports various map generation styles, robust drawing tools, and essential file management capabilities.

## **User Journey**

* **Specify map size (width / height) \-\> Create empty map \-\> Draw map \-\> Export map**  
  * A game designer can define the dimensions of a new map and initialize it as an empty canvas, then manually draw tiles using various tools, and finally save their work.  
* **Specify map size and feature count (rooms/caves/clearings) \-\> Create random map \-\> Adjust map details \-\> Export map**  
  * A game designer can leverage procedural generation to create a base map in a specific style (Town, Cave, Forest), then fine-tune it with drawing tools, and export the result.  
* **Load map from files \-\> Adjust map details \-\> Export map**  
  * A game designer can load previously saved maps to continue editing or refine existing designs, then re-export their updated work.

## **User Stories**

* As a game designer, I want to **create an ASCII map by specifying its size (width and height)**, so that I can draw map tiles using various tools.  
* As a game designer, I want to **randomly generate an ASCII map based on a chosen style (Town, Cave, Forest)**, so that I can start from an existing map layout rather than drawing completely from scratch.  
* As a game designer, when generating a **Town map**, I want the "Rooms" input to control the **number of interconnected rectangular rooms** created.  
* As a game designer, when generating a **Cave map**, I want the "Rooms" input (labeled "Caves") to control the **number of separate, irregularly shaped cave systems** (connected floor areas separated by walls).  
* As a game designer, when generating a **Forest map**, I want the "Rooms" input (labeled "Clearings") to control the **number of separate, irregularly shaped clearings** (connected open spaces surrounded by trees).  
* As a game designer, I want to **paint individual map tiles** with any chosen character.  
* As a game designer, I want to **draw rectangular rooms and structures using box-drawing characters**.  
* As a game designer, I want to **fill contiguous blocks of the map with specific characters** quickly.  
* As a game designer, I want to **add doors ('+') and stairs ('\>' / '\<')** to my map to indicate interactivity.  
* As a game designer, I want to **pan the map around the viewport** so that I can work on large maps without scrolling.  
* As a game designer, I want to **zoom in and out of my map using buttons and mouse scroll**, so that I can easily control the scale for detailed or overview work.  
* As a game designer, I want to **see the coordinates of the cursor on the map** so that I can place objects accurately.  
* As a game designer, I want to **have rulers on the edges of the viewport** so that I can measure distances and align elements.  
* As a game designer, I want to **export my map as a plain text file** so that I can save my progress and use it elsewhere.  
* As a game designer, I want to **load a map from a plain text file** so that I can resume editing previous work.  
* As a game designer, I want to **clear the current map** to start a new design quickly.  
* As a game designer, I want helpful **on-screen messages** to confirm actions or alert me to issues.

## **Features**

### **1\. Map Generation**

* **Custom Dimensions:** Users can specify map width and height through input fields.  
* **Empty Map Creation:** Initializes a map filled with space characters (' ').  
* **Random Map Generation:**  
  * **Town Style:** Generates a set number of rectangular rooms connected by corridors. Rooms are defined by box-drawing characters (┌┐└┘─│) and floors by periods (.).  
  * **Cave Style:** Generates a set number of distinct, irregularly shaped cave systems (connected . areas) separated by walls ( ), using a drunkard's walk algorithm for organic shapes. The "Rooms" input field's label dynamically changes to "Caves".  
  * **Forest Style:** Generates a set number of distinct, irregularly shaped clearings (connected . areas) separated by trees (\#), using a drunkard's walk algorithm for organic shapes. The "Rooms" input field's label dynamically changes to "Clearings".  
* **Border Handling:** Generated maps ensure the outer border is consistently filled with the appropriate "wall" character for the style (' ' for Town/Cave, '\#' for Forest).

### **2\. Drawing Tools**

* **Paint Tool:** Allows users to draw individual characters directly onto the map by clicking or dragging.  
* **Box Tool:** Enables drawing rectangular outlines using standard box-drawing characters. Supports live preview during drag.  
* **Fill Tool:** Implements a flood-fill algorithm to replace a contiguous area of a specific character with the selected drawing character.  
* **Door Tool:** Specifically places the '+' character.  
* **Stairs Tool:** Places either '\>' (stairs down) or '\<' (stairs up) characters. If a custom character is set to something other than '\>' or '\<', it defaults to '\>'.  
* **Move Tool:** Allows panning the map viewport by clicking and dragging.

### **3\. Character Selection**

* **Custom Character Input:** A text input field to type any single ASCII character for drawing.  
* **Quick Character Groups:** Dropdown menus providing predefined sets of characters categorized for convenience (Basic, Entry/Exit, Creatures, Items/Objects, Numbers). Selecting a character from these dropdowns automatically updates the Custom Char input and sets the tool to Paint.

### **4\. Map Navigation & Visualization**

* **Panning:** Users can pan the map by dragging the mouse when the Move tool is selected.  
* **Zoom:**  
  * Dedicated "Zoom In (+)" and "Zoom Out (-)" buttons.  
  * Mouse wheel support for zooming in and out. Zoom scales towards the mouse cursor.  
* **Coordinate Display:** Shows the current mouse cursor's grid coordinates (X, Y) on the map.  
* **Rulers:** Horizontal (X-axis) and vertical (Y-axis) rulers are displayed along the edges of the canvas, showing grid indices.  
* **Side Indicators:** Visual arrows (▲▼◀▶) appear on the edges of the canvas to indicate when there is more map content beyond the current view, prompting the user to pan.

### **5\. File Operations**

* **Export Map:** Exports the current mapData as a plain .txt file, preserving the ASCII layout.  
* **Load Map:** Imports a map from a selected .txt file.  
  * Automatically determines the width and height from the loaded file.  
  * Validates loaded dimensions against specifications.  
  * Pads shorter lines with spaces or truncates longer lines to match the inferred map width, ensuring a rectangular map.

### **6\. User Interface & Experience**

* **Responsive Design:** The layout is responsive, adapting to different screen sizes using Tailwind CSS. The canvas dynamically adjusts its displayed area.  
* **Feedback Messages:** A custom message box is used to provide non-blocking feedback to the user (e.g., "Map loaded successfully\!", "Map cleared\!").  
* **Tool Highlighting:** Selected drawing tools are visually highlighted.  
* **Cursor Feedback:** The canvas cursor changes to crosshair for drawing tools and grab for the Move tool.

## **Specifications**

### **Map Size Limits**

* **Width:** 20−65536  
* **Height:** 20−65536  
* **Rooms/Caves/Clearings:** 2−65536 (Note: Extremely high values for caves/clearings may result in dense, smaller features due to space constraints and non-overlapping generation.)

### **Specific Map Tiles**

The editor utilizes the following ASCII characters for specific tile types:

* (space character): Void space (walls/invisible walls, used as separator between distinct features in Cave/Forest modes).  
* .: Passable Floors  
* ┌┐└┘─│: Box drawing characters (for rectangular rooms in Town mode).  
* \+: Doors to rooms.  
* ╱╲: (Not explicitly used in current random generation, but available for manual drawing) Together with box drawing characters to draw irregular shaped caves.  
* \>\<: Stairs up/down.  
* \#: Obstacles (e.g., trees in Forest mode) that cannot be passed through.  
* @: Player character (for drawing).  
* A-Za-z: NPCs, Monsters, etc. (for drawing).  
* \*: Lootable items (for drawing).  
* $: Gold (for drawing).  
* \~: Liquid (for drawing).  
* \!: Exclamation (for drawing).  
* ?: Question (for drawing).  
* 0-9: Numbers (for drawing).

### **Technical Details**

* **Technology Stack:** HTML, CSS (Tailwind CSS), JavaScript.  
* **Rendering:** 2D canvas rendering for the tilemap.  
* **Performance:** Implemented efficient drawing loop that only renders visible parts of the map to handle large map sizes. Warnings are provided for extremely large maps that might impact browser performance.  
* **Responsiveness:** Designed to be fully responsive for optimal viewing and usability on various devices (mobile, tablet, desktop).  
* **Accessibility:** Basic touch event support for canvas interaction on touch devices.  
* **File Format:** Plain text (.txt) for map export/import, with each line representing a row of the map.