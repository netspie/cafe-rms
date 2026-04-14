# How to render diagrams

## Prerequisites

```bash
# PlantUML — renders .puml files (UML class diagrams, sequence diagrams, etc.)
# Text-based DSL for UML. Source files use @startuml/@enduml syntax.
# Requires Java runtime. Docs: https://plantuml.com
brew install plantuml

# Mermaid CLI (mmdc) — renders Mermaid diagram blocks embedded in .md files
# Mermaid is a JS-based diagramming tool supporting flowcharts, activity diagrams, etc.
# We use it for activity diagrams written inside ```mermaid code blocks in markdown.
# Docs: https://mermaid.js.org
npm install -g @mermaid-js/mermaid-cli
```

## Render class diagram (PlantUML)

```bash
cd docs/diagrams
plantuml -tpng 0-full-class-diagram.puml
# PlantUML uses the @startuml title as filename, so rename:
mv "CafeRMS - Full Class Diagram.png" 0-full-class-diagram.png
```

## Render activity diagrams (Mermaid)

First extract the mermaid block from .md, then render:

```bash
cd docs/diagrams

# Extract mermaid code from .md and render to .png
for f in 1-activity-online-ordering 2-activity-product-menu-management 3-activity-event-management; do
  awk '/^```mermaid$/{found=1;next} /^```$/{found=0} found' "${f}.md" > "${f}.mmd"
  mmdc -i "${f}.mmd" -o "${f}.png" -w 2400 -b white
  rm "${f}.mmd"
done
```

## Render a single activity diagram

```bash
cd docs/diagrams
awk '/^```mermaid$/{found=1;next} /^```$/{found=0} found' 1-activity-online-ordering.md > tmp.mmd
mmdc -i tmp.mmd -o 1-activity-online-ordering.png -w 2400 -b white
rm tmp.mmd
```
