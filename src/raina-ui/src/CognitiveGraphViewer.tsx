import React, { useEffect, useRef, useState, useCallback } from "react";
import { RotateCcw, Layout, Circle, X, Play, ChevronDown, ChevronRight } from "lucide-react";
import Sigma from "sigma";
import Graph from "graphology";
import gsap from "gsap";
import * as d3 from "d3";
import type { MouseCoords } from "sigma/types";
import forceAtlas2 from "graphology-layout-forceatlas2";
import type { ActivationHistoryItem, GraphData, GraphEdge, GraphNode } from "./models/models";

interface CognitiveGraphViewerProps {
  graphData: GraphData | null;
  isActive: boolean; // NEW: prop to indicate if the tab is active
}

const CognitiveGraphViewer: React.FC<CognitiveGraphViewerProps> = ({ graphData, isActive }) => {
  const containerRef = useRef<HTMLDivElement>(null);
  const [isInitialized] = useState(true);
  const [selectedNode, setSelectedNode] = useState<GraphNode | null>(null);
  const [isAnimating, setIsAnimating] = useState(false);
  const [layoutMode, setLayoutMode] = useState<"force-atlas2" | "force" | "circular">("force");
  const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({
    details: true,
    slots: true,
    history: true,
  });

  // Use correct types from graphology and sigma.js
  const sigmaRef = useRef<Sigma | null>(null);
  const graphRef = useRef<Graph | null>(null);
  const gsapRef = useRef<typeof gsap | null>(null);

  // Initialize graph
  const initializeGraph = useCallback((): void => {
    if (!isInitialized || !containerRef.current) return;
    cleanup();
    try {
      // Create graph
      graphRef.current = new Graph({ multi: false, type: "undirected" });
      gsapRef.current = gsap;

      // Create sigma instance
      sigmaRef.current = new Sigma(graphRef.current, containerRef.current, {
        renderLabels: true,
        labelFont: "Arial",
        labelSize: 12,
        labelColor: { color: "#374151" },
        defaultNodeColor: "#6B7280",
        defaultEdgeColor: "#9CA3AF",
        minCameraRatio: 0.1,
        maxCameraRatio: 10,
        enableEdgeEvents: true,
        renderEdgeLabels: true,
        edgeLabelFont: "Arial",
        edgeLabelSize: 10,
        edgeLabelColor: { color: "#374151" },
        allowInvalidContainer: false,
      });

      // Add event listeners
      sigmaRef.current.on("clickNode", (e: { node: string }) => {
        const nodeId = e.node;
        if (!graphRef.current) return;
        const nodeData = graphRef.current.getNodeAttributes(nodeId) as GraphNode;
        setSelectedNode(nodeData);
        pulseNode(nodeId);
      });

      sigmaRef.current.on("clickStage", () => {
        setSelectedNode(null);
      });

      // Add drag functionality
      let isDragging = false;
      let draggedNode: string | null = null;

      sigmaRef.current.on("downNode", (e: { node: string }) => {
        isDragging = true;
        draggedNode = e.node;
        sigmaRef.current?.getCamera().disable();
      });

      sigmaRef.current.getMouseCaptor().on("mousemove", (coords: MouseCoords) => {
        if (isDragging && draggedNode && sigmaRef.current && graphRef.current) {
          const pos = sigmaRef.current.viewportToGraph(coords);
          graphRef.current.setNodeAttribute(draggedNode, "x", pos.x);
          graphRef.current.setNodeAttribute(draggedNode, "y", pos.y);
        }
      });

      sigmaRef.current.getMouseCaptor().on("mouseup", () => {
        if (isDragging && sigmaRef.current) {
          isDragging = false;
          draggedNode = null;
          sigmaRef.current.getCamera().enable();
        }
      });

      console.log("Graph initialized successfully");
    } catch (error) {
      console.error("Error initializing graph:", error);
    }
  }, [isInitialized]);

  // Load graph data
  const loadGraphData = useCallback(
    (graphData: GraphData): void => {
      console.log("Loading Graph Data...");
      if (!graphRef.current || !sigmaRef.current) return;

      try {
        // Clear existing graph
        graphRef.current.clear();

        // Add nodes - graphData.nodes is an array of GraphNode objects
        graphData.nodes.forEach((nodeData) => {
          const nodeSize = calculateNodeSize(nodeData);
          const nodeColor = getCognitiveNodeColor(nodeData);

          if (graphRef.current) {
            graphRef.current.addNode(nodeData.id, {
              ...nodeData,
              x: nodeData.x || Math.random() * 1000,
              y: nodeData.y || Math.random() * 1000,
              size: nodeSize,
              color: nodeColor,
              label: nodeData.label || nodeData.id,
              originalSize: nodeSize,
              originalColor: nodeColor,
            });
          }
        });

        // Add edges - graphData.edges is an array of GraphEdge objects
        graphData.edges.forEach((edgeData) => {
          if (!graphRef.current || !graphRef.current.hasNode(edgeData.source) || !graphRef.current.hasNode(edgeData.target)) {
            return;
          }

          const avgWeight = ((edgeData.weightAtoB || 0.5) + (edgeData.weightBtoA || 0.5)) / 2;
          const edgeSize = Math.max(1, avgWeight * 4);
          const edgeColor = getCognitiveEdgeColor(edgeData);

          try {
            graphRef.current.addEdge(edgeData.source, edgeData.target, {
              ...edgeData,
              size: edgeSize,
              color: edgeColor,
              label: edgeData.label || edgeData.relationAtoB || "Related",
              originalSize: edgeSize,
              originalColor: edgeColor,
            });
          } catch (error) {
            console.warn(`Error adding edge:`, error);
          }
        });

        // Apply layout
        if (layoutMode === "circular") {
          applyCircularLayout();
        } else if (layoutMode === "force-atlas2") {
          applyForceAtlas2Layout();
        } else {
          applyForceLayout();
        }

        sigmaRef.current.refresh();
        console.log(`Graph loaded: ${graphData.nodes.length} nodes, ${graphData.edges.length} edges`);
      } catch (error) {
        console.error("Error loading graph data:", error);
      }
    },
    [layoutMode]
  );

  // Helper functions
  const calculateNodeSize = (nodeData: GraphNode): number => {
    const baseSize = 8;
    const activationSize = (nodeData.activationLevel || 0) * 10;
    const connectionCount = 1; // We don't have edge count here, so use default
    const connectionSize = Math.min(connectionCount * 2, 20);
    const calculatedSize = (baseSize + activationSize + connectionSize) / 2;

    // Ensure minimum size of 6 and maximum size of 50 for visibility
    return Math.max(Math.min(calculatedSize, 10), 6);
  };

  const getCognitiveNodeColor = (nodeData: GraphNode): string => {
    const activation = nodeData.activationLevel || 0;

    if (!nodeData.isWorkingMemory) {
      return "#7F1D1D"; // Dark red for non-working memory
    }

    const typeColors: Record<string, { r: number; g: number; b: number }> = {
      Declarative: { r: 59, g: 130, b: 246 }, // Blue
      Procedural: { r: 34, g: 197, b: 94 }, // Green
      ContextSummary: { r: 251, g: 146, b: 60 }, // Orange
      Memory: { r: 236, g: 72, b: 153 }, // Pink
      Utterance: { r: 14, g: 165, b: 233 }, // Sky
      Conversation: { r: 234, g: 179, b: 8 }, // Yellow
      Instance: { r: 168, g: 85, b: 247 }, // Purple
    };

    const chunkType = nodeData.chunkType || "Instance";
    const baseColor = typeColors[chunkType] || typeColors["Instance"];

    const intensity = 0.3 + activation * 0.7;
    const r = Math.floor(baseColor.r * intensity);
    const g = Math.floor(baseColor.g * intensity);
    const b = Math.floor(baseColor.b * intensity);

    return `rgb(${r}, ${g}, ${b})`;
  };

  const getCognitiveEdgeColor = (edgeData: GraphEdge): string => {
    const relationColors: Record<string, string> = {
      Contains: "#22C55E",
      PartOf: "#22C55E",
      Association: "#3B82F6",
      ResponseTo: "#F97316",
      HasResponse: "#F97316",
      UnderstoodBy: "#EAB308",
      Understands: "#EAB308",
      DirectedTo: "#EF4444",
      ChangesOver: "#A855F7",
      ChangedBy: "#A855F7",
    };

    const relation = edgeData.relationAtoB || "Association";
    return relationColors[relation] || "#06B6D4";
  };
  const cleanup = useCallback((): void => {
    try {
      if (sigmaRef.current) {
        // Remove all event listeners
        sigmaRef.current.removeAllListeners();
        // Kill the sigma instance
        sigmaRef.current.kill();
        sigmaRef.current = null;
      }

      if (graphRef.current) {
        graphRef.current.clear();
        graphRef.current = null;
      }

      // Clear the container
      if (containerRef.current) {
        containerRef.current.innerHTML = "";
      }
    } catch (error) {
      console.warn("Error during cleanup:", error);
    }
  }, []);
  const applyD3SpacingOnly = async (): Promise<void> => {
    if (!graphRef.current) return;

    type D3NodeType = GraphNode & { id: string; x: number; y: number; size: number };

    const nodes: D3NodeType[] = graphRef.current.nodes().map((nodeId: string) => {
      const attrs = graphRef.current!.getNodeAttributes(nodeId) as GraphNode;
      return { ...attrs, id: nodeId, x: attrs.x ?? 0, y: attrs.y ?? 0, size: attrs.size ?? 8 };
    });

    // Only use collision force to spread nodes apart, no other forces
    const simulation = d3
      .forceSimulation<D3NodeType>(nodes)
      .force(
        "collision",
        d3
          .forceCollide<D3NodeType>()
          .radius((d: D3NodeType) => d.size * 5.5) // Adjust multiplier for desired spacing
          .strength(0.9)
          .iterations(30) // More iterations for better collision resolution
      )
      .alpha(0.5) // Lower alpha since we're just spacing
      .alphaDecay(0.05)
      .stop(); // Don't auto-start

    return new Promise<void>((resolve) => {
      simulation.on("tick", () => {
        nodes.forEach((node: D3NodeType) => {
          if (graphRef.current && graphRef.current.hasNode(node.id)) {
            graphRef.current.setNodeAttribute(node.id, "x", node.x);
            graphRef.current.setNodeAttribute(node.id, "y", node.y);
          }
        });
        sigmaRef.current?.refresh();
      });

      simulation.on("end", () => {
        sigmaRef.current?.refresh();
        resolve();
      });

      simulation.restart(); // Start the simulation
    });
  };
  const applyForceAtlas2Layout = async (): Promise<void> => {
    if (!graphRef.current) return;

    try {
      // Set random initial positions if nodes don't have positions
      graphRef.current.nodes().forEach((nodeId: string) => {
        const node = graphRef.current!.getNodeAttributes(nodeId);
        if (!node.x || !node.y) {
          graphRef.current!.setNodeAttribute(nodeId, "x", Math.random() * 1000);
          graphRef.current!.setNodeAttribute(nodeId, "y", Math.random() * 1000);
        }
      });

      // ForceAtlas2 settings
      const settings = {
        iterations: 500, // Number of iterations
        settings: {
          gravity: 0.05, // Gravity strength
          scalingRatio: 200, // How much repulsion vs attraction
          strongGravityMode: false,
          barnesHutOptimize: true,
          barnesHutTheta: 0.5,
          linLogMode: false,
          adjustSizes: true, // Don't adjust for node sizes
          edgeWeightInfluence: 1,
          slowDown: 1,
          startingIterations: 1,
          iterationsPerRender: 10,
        },
      };

      // Run ForceAtlas2 with progress updates
      //let iterationCount = 0;
      //const maxIterations = settings.iterations;

      await new Promise<void>((resolve) => {
        let iterationCount = 0;
        const runIteration = () => {
          if (iterationCount >= settings.iterations || !graphRef.current) {
            resolve();
            return;
          }

          const batchSize = Math.min(settings.settings.iterationsPerRender, settings.iterations - iterationCount);
          forceAtlas2.assign(graphRef.current, {
            iterations: batchSize,
            settings: settings.settings,
          });
          iterationCount += batchSize;

          if (iterationCount < settings.iterations) {
            requestAnimationFrame(runIteration);
          } else {
            resolve();
          }
        };
        runIteration();
      });

      // Step 2: Run D3 force for spacing (keep positions but add collision)
      await applyD3SpacingOnly();
      await applyD3SpacingOnly();
    } catch (error) {
      console.error("Error applying ForceAtlas2 layout:", error);
      // Fallback to force layout
      await applyForceLayout();
    }
  };
  // Apply D3 force layout
  const applyForceLayout = async (): Promise<void> => {
    if (!graphRef.current) return;

    type D3NodeType = GraphNode & { id: string; x: number; y: number; size: number };
    type D3LinkType = {
      source: string | D3NodeType;
      target: string | D3NodeType;
      id?: string;
      label?: string;
      relationAtoB?: string;
      relationBtoA?: string;
      weightAtoB?: number;
      weightBtoA?: number;
      size?: number;
      color?: string;
      originalSize?: number;
      originalColor?: string;
      [key: string]: unknown;
    };

    const nodes: D3NodeType[] = graphRef.current.nodes().map((nodeId: string) => {
      const attrs = graphRef.current!.getNodeAttributes(nodeId) as GraphNode;
      return { ...attrs, id: nodeId, x: attrs.x ?? 0, y: attrs.y ?? 0, size: attrs.size ?? 8 };
    });

    const links: D3LinkType[] = graphRef.current.edges().map((edgeId: string) => {
      const edgeAttrs = graphRef.current!.getEdgeAttributes(edgeId) as GraphEdge;
      return {
        ...edgeAttrs,
        source: graphRef.current!.source(edgeId),
        target: graphRef.current!.target(edgeId),
      };
    });

    const simulation = d3
      .forceSimulation<D3NodeType>(nodes)
      .force(
        "link",
        d3
          .forceLink<D3NodeType, D3LinkType>(links)
          .id((d: D3NodeType) => d.id)
          .distance((d: D3LinkType) => {
            const source = typeof d.source === "string" ? nodes.find((n) => n.id === d.source) : d.source;
            const target = typeof d.target === "string" ? nodes.find((n) => n.id === d.target) : d.target;
            return ((source?.size ?? 8) + (target?.size ?? 8)) * 4;
          })
          .strength(0.2)
      )
      .force("charge", d3.forceManyBody().strength(-1500).distanceMax(800))
      .force(
        "collision",
        d3
          .forceCollide<D3NodeType>()
          .radius((d: D3NodeType) => d.size * 1.5)
          .strength(0.8)
      )
      .force("center", d3.forceCenter(500, 300))
      .alpha(1)
      .alphaDecay(0.02);

    return new Promise<void>((resolve) => {
      simulation.on("tick", () => {
        nodes.forEach((node: D3NodeType) => {
          if (graphRef.current && graphRef.current.hasNode(node.id)) {
            graphRef.current.setNodeAttribute(node.id, "x", node.x);
            graphRef.current.setNodeAttribute(node.id, "y", node.y);
          }
        });

        if (simulation.alpha() > 0.3 && Math.random() < 0.1) {
          sigmaRef.current?.refresh();
        }
      });

      simulation.on("end", () => {
        sigmaRef.current?.refresh();
        resolve();
      });
    });
  };

  // Arrange nodes in a circular layout
  const applyCircularLayout = (): void => {
    if (!graphRef.current) return;

    const nodes = graphRef.current.nodes();
    const nodeCount = nodes.length;
    const radius = Math.max(200, nodeCount * 15);
    const centerX = 500;
    const centerY = 300;

    // Group nodes by type
    const nodesByType: Record<string, string[]> = {};
    nodes.forEach((nodeId: string) => {
      if (graphRef.current) {
        const node = graphRef.current.getNodeAttributes(nodeId);
        const type = node.chunkType || "Unknown";
        if (!nodesByType[type]) nodesByType[type] = [];
        nodesByType[type].push(nodeId);
      }
    });

    const typeCount = Object.keys(nodesByType).length;
    const anglePerType = (2 * Math.PI) / typeCount;

    Object.entries(nodesByType).forEach(([, typeNodes], typeIndex) => {
      const typeAngle = typeIndex * anglePerType;
      const typeRadius = radius + (typeIndex % 2) * 50;

      typeNodes.forEach((nodeId, nodeIndex) => {
        if (!graphRef.current) return;
        const node = graphRef.current.getNodeAttributes(nodeId);
        const nodeAngle = typeAngle + (nodeIndex / typeNodes.length) * anglePerType;

        const targetX = centerX + Math.cos(nodeAngle) * typeRadius;
        const targetY = centerY + Math.sin(nodeAngle) * typeRadius;

        if (gsapRef.current) {
          gsapRef.current.to(node as Record<string, unknown>, {
            duration: 1.5,
            x: targetX,
            y: targetY,
            ease: "power2.out",
            onUpdate: (): void => {
              sigmaRef.current?.refresh();
            },
          });
        } else {
          node.x = targetX;
          node.y = targetY;
        }
      });
    });

    if (!gsapRef.current) {
      sigmaRef.current?.refresh();
    }
  };

  // Reset layout with better initial positioning
  const resetLayout = async (): Promise<void> => {
    if (!graphRef.current) return;

    const nodes = graphRef.current.nodes();

    // Reset velocities and spread nodes initially
    nodes.forEach((nodeId: string) => {
      if (graphRef.current) {
        const attrs = graphRef.current.getNodeAttributes(nodeId);
        attrs.vx = 0;
        attrs.vy = 0;
      }
    });

    spreadNodesInitially(nodes);
    await applyForceLayout();
  };

  // Helper function: Spread nodes initially to avoid clustering
  const spreadNodesInitially = (nodes: string[]): void => {
    if (!graphRef.current) return;

    const nodeCount = nodes.length;
    const spacing = Math.max(100, Math.sqrt(nodeCount) * 50);
    const gridSize = Math.ceil(Math.sqrt(nodeCount));

    nodes.forEach((nodeId: string, index: number) => {
      if (!graphRef.current) return;

      const node = graphRef.current.getNodeAttributes(nodeId);
      const row = Math.floor(index / gridSize);
      const col = index % gridSize;

      // Add some randomness to avoid perfect grid
      const randomOffset = 30;
      node.x = col * spacing + (Math.random() - 0.5) * randomOffset;
      node.y = row * spacing + (Math.random() - 0.5) * randomOffset;

      // Initialize velocity
      node.vx = 0;
      node.vy = 0;
    });
  };

  const pulseNode = (nodeId: string) => {
    if (!graphRef.current || !graphRef.current.hasNode(nodeId) || !gsapRef.current) return;

    const nodeAttrs = graphRef.current.getNodeAttributes(nodeId);
    const originalSize = nodeAttrs.originalSize ?? nodeAttrs.size ?? 8;

    gsapRef.current
      .timeline()
      .to(nodeAttrs as Record<string, unknown>, {
        duration: 0.2,
        size: (originalSize as number) * 1.5,
        ease: "power2.out",
        onUpdate: (): void => {
          sigmaRef.current?.refresh();
        },
      })
      .to(nodeAttrs as Record<string, unknown>, {
        duration: 0.4,
        size: originalSize,
        ease: "elastic.out(1, 0.3)",
        onUpdate: (): void => {
          sigmaRef.current?.refresh();
        },
      });
  };

  const animateActivationHistory = async (historyItem: ActivationHistoryItem, targetNodeId: string) => {
    if (isAnimating || !historyItem.activatedBy || !gsapRef.current) return;

    setIsAnimating(true);

    const activationChain = [...historyItem.activatedBy].reverse();
    activationChain.push(targetNodeId);

    const validPath = activationChain.filter((nodeId) => nodeId && graphRef.current?.hasNode(nodeId));

    if (validPath.length < 2) {
      setIsAnimating(false);
      return;
    }

    // Animate pathway
    for (let i = 0; i < validPath.length - 1; i++) {
      const fromNodeId = validPath[i];
      // const toNodeId = validPath[i + 1]; // Removed unused variable

      // Animate nodes
      await animateNodeActivation(fromNodeId, true);
      await new Promise((resolve) => setTimeout(resolve, 200));
    }

    // Final node
    await animateNodeActivation(validPath[validPath.length - 1], true, true);
    await new Promise((resolve) => setTimeout(resolve, 1000));

    // Reset
    for (const nodeId of validPath) {
      await animateNodeActivation(nodeId, false);
    }

    setIsAnimating(false);
  };

  const animateNodeActivation = async (nodeId: string, isActive: boolean, isFinal: boolean = false) => {
    if (!graphRef.current || !graphRef.current.hasNode(nodeId) || !gsapRef.current) return;

    const nodeAttrs = graphRef.current.getNodeAttributes(nodeId);
    const originalSize = nodeAttrs.originalSize ?? nodeAttrs.size ?? 8;
    const originalColor = nodeAttrs.originalColor ?? nodeAttrs.color ?? "#000";

    if (isActive) {
      const activationColor = isFinal ? "#EAB308" : "#3B82F6";
      const activationSize = (originalSize as number) * (isFinal ? 2.0 : 1.5);

      return new Promise<void>((resolve) => {
        if (gsapRef.current)
          gsapRef.current
            .timeline()
            .to(nodeAttrs as Record<string, unknown>, {
              duration: 0.3,
              size: activationSize,
              color: activationColor,
              ease: "power2.out",
              onUpdate: (): void => {
                sigmaRef.current?.refresh();
              },
            })
            .to(nodeAttrs as Record<string, unknown>, {
              duration: 0.4,
              size: (originalSize as number) * 1.2,
              ease: "elastic.out(1, 0.3)",
              onUpdate: (): void => {
                sigmaRef.current?.refresh();
              },
              onComplete: resolve,
            });
      });
    } else {
      return new Promise<void>((resolve) => {
        gsapRef.current!.to(nodeAttrs as Record<string, unknown>, {
          duration: 0.5,
          size: originalSize,
          color: originalColor,
          ease: "power2.out",
          onUpdate: (): void => {
            sigmaRef.current?.refresh();
          },
          onComplete: resolve,
        });
      });
    }
  };

  const toggleSection = (section: string) => {
    setExpandedSections((prev) => ({
      ...prev,
      [section]: !prev[section],
    }));
  };
  const getNextLayoutMode = (current: string) => {
    switch (current) {
      case "force":
        return "force-atlas2";
      case "force-atlas2":
        return "circular";
      case "circular":
        return "force";
      default:
        return "force";
    }
  };
  const getLayoutLabel = (mode: string) => {
    switch (mode) {
      case "force":
        return "Force";
      case "force-atlas2":
        return "ForceAtlas2";
      case "circular":
        return "Circular";
      default:
        return "Force";
    }
  };
  const getLayoutIcon = (mode: string) => {
    switch (mode) {
      case "force":
        return <Layout size={14} />;
      case "forceatlas2":
        return <Circle size={14} />;
      case "circular":
        return <RotateCcw size={14} />;
      default:
        return <Layout size={14} />;
    }
  };
  // Initialize graph when component mounts and libraries are loaded
  useEffect(() => {
    if (isActive && isInitialized) {
      initializeGraph();
      if (graphData) {
        loadGraphData(graphData);
      }
    }
  }, [isActive, isInitialized, initializeGraph, graphData, loadGraphData]);
  return (
    <div className="h-full flex flex-col lg:flex-row relative">
      {/* Main Graph Container */}
      <div className="flex-1 relative bg-gradient-to-br from-blue-50 to-purple-50">
        <div ref={containerRef} className="w-full h-full" />

        {/* Controls Overlay */}
        <div className="absolute top-4 left-4 bg-white/90 backdrop-blur-sm rounded-lg shadow-lg border border-gray-200 p-4 space-y-3">
          <h3 className="text-sm font-semibold text-gray-900 flex items-center">🧠 Cognitive Graph</h3>

          <div className="flex flex-wrap gap-2">
            <button
              onClick={() => setLayoutMode(getNextLayoutMode(layoutMode))}
              className="flex items-center space-x-1 px-3 py-1.5 bg-blue-100 text-blue-700 rounded-md hover:bg-blue-200 transition-colors text-sm"
            >
              {getLayoutIcon(layoutMode)}
              <span>{getLayoutLabel(layoutMode)}</span>
            </button>

            <button
              onClick={resetLayout}
              className="flex items-center space-x-1 px-3 py-1.5 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 transition-colors text-sm"
            >
              <RotateCcw size={14} />
              <span>Reset</span>
            </button>
          </div>

          {isAnimating && (
            <div className="flex items-center space-x-2 text-xs text-orange-600">
              <Play size={12} className="animate-pulse" />
              <span>Playing animation...</span>
            </div>
          )}
        </div>
      </div>

      {/* Node Details Panel */}
      {selectedNode && (
        <div className="lg:w-96 w-full bg-white border-l border-gray-200 shadow-lg flex flex-col max-h-full lg:max-h-none">
          {/* Header */}
          <div className="flex items-center justify-between p-4 border-b border-gray-200 bg-gray-50">
            <h3 className="text-lg font-semibold text-gray-900 truncate">{selectedNode.label || selectedNode.id}</h3>
            <button onClick={() => setSelectedNode(null)} className="p-1 hover:bg-gray-200 rounded-full transition-colors">
              <X size={16} className="text-gray-500" />
            </button>
          </div>

          {/* Content */}
          <div className="flex-1 overflow-y-auto p-4 space-y-4">
            {/* Basic Details */}
            <div>
              <button
                onClick={() => toggleSection("details")}
                className="flex items-center justify-between w-full text-left p-2 hover:bg-gray-50 rounded-md"
              >
                <h4 className="font-medium text-gray-900">Details</h4>
                {expandedSections.details ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
              </button>

              {expandedSections.details && (
                <div className="mt-2 space-y-2 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-600">Type:</span>
                    <span className="font-medium">{selectedNode.chunkType || "Unknown"}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Category:</span>
                    <span className="font-medium">{selectedNode.cognitiveCategory || "Unknown"}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Semantic Type:</span>
                    <span className="font-medium">{selectedNode.semanticType || "Unknown"}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Activation:</span>
                    <span className="font-medium">{selectedNode.activationLevel?.toFixed(3) || "0.000"}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-600">Size:</span>
                    <span className="font-medium">{selectedNode.size?.toFixed(1) || "0.0"}</span>
                  </div>
                </div>
              )}
            </div>

            {/* Slots Information */}
            {selectedNode.slots && (
              <div>
                <button
                  onClick={() => toggleSection("slots")}
                  className="flex items-center justify-between w-full text-left p-2 hover:bg-gray-50 rounded-md"
                >
                  <h4 className="font-medium text-gray-900">Key Information</h4>
                  {expandedSections.slots ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                </button>

                {expandedSections.slots && (
                  <div className="mt-2 space-y-2">
                    {Object.entries(selectedNode.slots)
                      .filter(([key]) => ["EntityName", "Text", "Intent", "SpeakerName", "ListenerName"].includes(key))
                      .map(([key, slot]) => {
                        const value = typeof slot?.value === "string" ? slot.value : JSON.stringify(slot?.value || slot);
                        if (!value || value === "{}" || value.length > 200) return null;

                        return (
                          <div key={key} className="bg-blue-50 border border-blue-200 rounded-lg p-3">
                            <div className="text-xs font-medium text-blue-800 mb-1">{key}:</div>
                            <div className="text-sm text-gray-900">{value}</div>
                          </div>
                        );
                      })
                      .filter(Boolean)}
                  </div>
                )}
              </div>
            )}

            {/* Activation History */}
            {selectedNode.activationHistory && selectedNode.activationHistory.length > 0 && (
              <div>
                <button
                  onClick={() => toggleSection("history")}
                  className="flex items-center justify-between w-full text-left p-2 hover:bg-gray-50 rounded-md"
                >
                  <h4 className="font-medium text-gray-900">Activation History</h4>
                  {expandedSections.history ? <ChevronDown size={16} /> : <ChevronRight size={16} />}
                </button>

                {expandedSections.history && (
                  <div className="mt-2 space-y-2 max-h-64 overflow-y-auto">
                    {selectedNode.activationHistory.slice(0, 8).map((item, index) => (
                      <div
                        key={index}
                        onClick={() => animateActivationHistory(item, selectedNode.id)}
                        className={`bg-gray-50 border border-gray-200 rounded-lg p-3 cursor-pointer hover:bg-blue-50 hover:border-blue-300 transition-colors ${
                          isAnimating ? "opacity-50 cursor-not-allowed" : ""
                        }`}
                      >
                        <div className="text-xs font-medium text-gray-800 mb-1">
                          Seq. {item.sequenceNumber} {item.formattedDate}
                        </div>
                        <div className="text-xs text-gray-600 mb-1">
                          Activation: {item.previousValue?.toFixed(3) || "0.000"} → {item.newValue?.toFixed(3) || "0.000"}(
                          {item.change && item.change >= 0 ? "+" : ""}
                          {item.change?.toFixed(3) || "0.000"})
                        </div>
                        {item.activationReason && <div className="text-xs text-orange-600 mb-1">Reason: {item.activationReason}</div>}
                        {item.activationSource && <div className="text-xs text-orange-600 mb-1">Source: {item.activationSource}</div>}
                        {item.activatedByChunk && item.activatedByChunk !== "00000000-0000-0000-0000-000000000000" && (
                          <div className="text-xs text-teal-600 mb-1">Triggered by: {item.activatedByChunk.substring(0, 8)}...</div>
                        )}
                        {item.activatedBy && item.activatedBy.length > 0 && (
                          <div className="text-xs text-purple-600 mb-1">Pathway: {item.activatedBy.length} nodes</div>
                        )}
                        <div className="text-xs text-blue-600 mt-2">Click to animate pathway</div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default CognitiveGraphViewer;
