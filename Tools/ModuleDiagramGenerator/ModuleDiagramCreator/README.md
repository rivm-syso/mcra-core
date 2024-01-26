# **MCRA Module Diagram Creator**

MCRA Module diagram creator is a tool that generates an SVG image file out of the MCRA module definitions. The image shows the MCRA modular design based on the true and current implementation.

## **Table of Contents**
  - [**Features**](#features)
  - [**Implementation**](#implementation)
    - [**Source code**](#source-code)
    - [**Graphviz**](#graphviz)
  - [**Editing the graph**](#editing-the-graph)
    - [**Graph element in module definition XML file**](#graph-element-in-module-definition-xml-file)
    - [**Important note when editing**](#important-note-when-editing)
  - [**Copyright**](#copyright)

## Features
- Shows all MCRA modules as nodes, specified in XML files located in mcra-core\MCRA.General\ModuleDefinitions\Modules\
- Shows connections defined in the module definition files as edges
- Allows to specify the output format, default is SVG
- Creates a Graphviz diagram of type ["fdp"](https://www.graphviz.org/pdf/dot.1.pdf), which allows to define (x, y) positioning of the nodes. 
The Graphviz fdp engine will try to draw the node on the defined position but may deviate from the definition if the algorithm decides so. For example, when two nodes will overlap the positioning will not be exactly on the prescribed coordinates.

## Implementation

### Source code
- Reads the module definitions from MCRA.General.ModuleDefinitions.McraModuleDefinitions, and not directly from the XML files.
- The XML module definitions have been extended with a location specification in the module diagram, the graph (x, y) coordinates.

### Graphviz
The MCRA module diagram is generated using [Graphviz](https://graphviz.org/), an open source graph visualization software.
In the project, the Graphviz binaries are added through a NuGet package, which makes building and deployment of the solution 
easier because you do not need to download and install the Graphviz tool.
In case you want to experiment wiht the downloaded tool, you can download the Windows installer from [Graphviz Download](https://graphviz.org/download/).

## Editing the graph


### Graph element in module definition XML file
A module is placed in the MCRA module diagram when the XMl file of the module definition contains a Graph section, placed on the root level.
The graph section holds three fields:

1. X             - the x-coordinate, a value between [0, 100] (required)
2. Y             - the y-coordinate, a value between [0, 100] (required)
3. Linebreaks    - to break up a very long name of the module into parts (optional)


```
<Graph>
    <X>31</X>
    <Y>61</Y>
    <Linebreaks>2</Linebreaks>
</Graph>
```

### Important when editing
When editing the (x, y) coordinates in the module definition XML file, take care about the positioning of the module node.
The Graphviz fdp algorithm will try to follow the defined coordinates but may deviate from these when, e.g., overlap of two modules would otherwise occur. 
Always check the result after editing the graph coordinates.


## Copyright
MCRA module diagram creator is developed by Wageningen University & Research, Biometris for RIVM (2024)\
Copyright © 2024 [RIVM](https://www.rivm.nl/en/food-safety/chemicals-in-food/monte-carlo-risk-assessment-mcra
