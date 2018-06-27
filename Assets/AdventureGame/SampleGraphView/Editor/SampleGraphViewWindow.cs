using UnityEditor;

public class SampleGraphViewWindow : GraphViewWindow {
    
    [MenuItem("SampleGraphView/Open Window")]
    public static void OpenWindow()
    {
        GetWindow<SampleGraphViewWindow>();
    }

	// Use this for initialization
	void OnEnable()
	{
        var sampleGraphView = new SampleGraphView();
        m_GraphView = sampleGraphView;

        m_GraphView.name = "theView";
        m_GraphView.persistenceKey = "theView";
        m_GraphView.StretchToParentSize();

        this.GetRootVisualContainer().Add(m_GraphView);

        if (!LoadGraphData(k_TestGraphDataPath))
        {
	        Node node = CreateRootNode();
	        m_GraphView.AddElement(node);
        }

        m_GraphView.graphViewChanged += OnGraphViewChanged;
        m_GraphView.nodeCreationRequest += OnRequestNodeCreation;
    }

    protected void OnRequestNodeCreation(NodeCreationContext context)
    {
        SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), this);
    }

    public GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        EditorApplication.update += DelayedSaveGraphData;
        return change;
    }

    void DelayedSaveGraphData()
    {
        EditorApplication.update -= DelayedSaveGraphData;
        SaveGraphData(k_TestGraphDataPath);
    }

    Node CreateRootNode()
    {
        Node node = new Node();
        node.title = "Start";
        node.capabilities &= ~(Capabilities.Movable | Capabilities.Deletable);
        node.style.backgroundColor = Color.green;
        node.SetPosition(new Rect(new Vector2(-50, -50), new Vector2(100, 100)));

        Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        outputPort.portName = "start";
        outputPort.userData = null;
        
        node.outputContainer.Add(outputPort);
        return node;
    }

    Node DeserializeNode(SerializableGraphData.SerializableGraphNode graphNode)
    {
        Node node = graphNode.m_title == "Start" ? CreateRootNode() : CreateNode(graphNode.m_title);
        node.SetPosition(new Rect(graphNode.m_position, Vector2.zero));
        return node;
    }

    Node CreateNode(string title)
    {
        Node node = new Node();
        node.title = title;
        
        node.capabilities |= Capabilities.Movable;
        Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
        inputPort.portName = "in port";
        inputPort.userData = null;
        node.inputContainer.Add(inputPort);

        Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
        outputPort.portName = "out port";
        outputPort.userData = null;
        node.outputContainer.Add(outputPort);

        return node;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>();
        tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

        Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");
        tree.Add(new SearchTreeGroupEntry(new GUIContent("Test Category"), 1));
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item1", icon)) { level = 2, userData = typeof(SampleGraphView) });
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item2", icon)) { level = 2, userData = typeof(SampleGraphView) });
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item3", icon)) { level = 2, userData = typeof(SampleGraphView) });
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item4", icon)) { level = 2, userData = typeof(SampleGraphView) });
        tree.Add(new SearchTreeEntry(new GUIContent("Test Item5", icon)) { level = 2, userData = typeof(SampleGraphView) });

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
    {
        if (!(entry is SearchTreeGroupEntry))
        {
            Node node = CreateNode("Default");
            m_GraphView.AddElement(node);

            Vector2 pointInWindow = context.screenMousePosition - position.position;
            Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);
            node.SetPosition(new Rect(pointInGraph, Vector2.zero));
            node.Select(m_GraphView, false);

            SaveGraphData(k_TestGraphDataPath);
            return true;
        }
        return false;
    }

    public void SaveGraphData(string outputPath)
    {
        List<Node> nodes = m_GraphView.nodes.ToList();
        SerializableGraphData graphData = ScriptableObject.CreateInstance<SerializableGraphData>();
        graphData.m_graphNodes = new List<SerializableGraphData.SerializableGraphNode>();
        for (int i = 0; i < nodes.Count; ++i)
        {
            SerializableGraphData.SerializableGraphNode graphNode = new SerializableGraphData.SerializableGraphNode();
            Node currentNode = nodes[i];

            graphNode.m_title = currentNode.title;
            graphNode.m_position = currentNode.GetPosition().position;
            graphNode.m_outputs = new List<SerializableGraphData.SerializableGraphEdge>();

            foreach (Port port in currentNode.outputContainer)
            {
                foreach (Edge edge in port.connections)
                {
                    SerializableGraphData.SerializableGraphEdge serializedEdge = new SerializableGraphData.SerializableGraphEdge();
                    serializedEdge.m_sourcePort = currentNode.outputContainer.IndexOf(edge.output);
                    serializedEdge.m_targetNode = nodes.IndexOf(edge.input.node);
                    serializedEdge.m_targetPort = edge.input.node.inputContainer.IndexOf(edge.input);
                    graphNode.m_outputs.Add(serializedEdge);
                }
            }
            graphData.m_graphNodes.Add(graphNode);
        }

        AssetDatabase.CreateAsset(graphData, outputPath);
        AssetDatabase.SaveAssets();
    }

    public bool LoadGraphData(string inputPath)
    {
        SerializableGraphData graphData = AssetDatabase.LoadAssetAtPath<SerializableGraphData>(inputPath);
        if (graphData == null)
        {
            return false;
        }

        // create the nodes
        List<Node> createdNodes = new List<Node>();
        for (int i = 0; i < graphData.m_graphNodes.Count; ++i)
        {
            Node node = DeserializeNode(graphData.m_graphNodes[i]);
            createdNodes.Add(node);
            m_GraphView.AddElement(node);
        }

        //connect the nodes
        for (int i = 0; i < graphData.m_graphNodes.Count; ++i)
        {
            for (int iEdge = 0; iEdge < graphData.m_graphNodes[i].m_outputs.Count; ++iEdge)
            {
                SerializableGraphData.SerializableGraphEdge edge = graphData.m_graphNodes[i].m_outputs[iEdge];
                Port outputPort = createdNodes[i].outputContainer[edge.m_sourcePort] as Port;
                Port inputPort = createdNodes[edge.m_targetNode].inputContainer[edge.m_targetPort] as Port;
                m_GraphView.AddElement(outputPort.ConnectTo(inputPort));
            }
        }

        return true;
    }
}
