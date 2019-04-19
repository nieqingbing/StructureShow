using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using StructureShow.Main.Model;

namespace StructureShow.Main.Control
{
    class DiagramLogic
    {
        #region fields

        // List of the connections, specify connections between two nodes.
        private List<DiagramConnector> connections = new List<DiagramConnector>();

        // Map that allows quick lookup of a Person object to connection information.
        // Used when setting up the connections between nodes.
        private Dictionary<Shape, DiagramConnectorNode> personLookup =
            new Dictionary<Shape, DiagramConnectorNode>();

        // List of people, global list that is shared by all objects in the application.
        private UMLCollection family;

        // Callback when a node is clicked.
        private RoutedEventHandler nodeClickHandler;

        // Filter year for nodes and connectors.
        private double displayYear;

        #endregion

        #region properties

        /// <summary>
        /// Sets the callback that is called when a node is clicked.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public RoutedEventHandler NodeClickHandler
        {
            set { nodeClickHandler = value; }
        }

        /// <summary>
        /// Gets the list of people in the family.
        /// </summary>
        public UMLCollection Family
        {
            get { return family; }
        }

        /// <summary>
        /// Gets the list of connections between nodes.
        /// </summary>
        public List<DiagramConnector> Connections
        {
            get { return connections; }
        }

        /// <summary>
        /// Gets the person lookup list. This includes all of the 
        /// people and nodes that are displayed in the diagram.
        /// </summary>
        public Dictionary<Shape, DiagramConnectorNode> PersonLookup
        {
            get { return personLookup; }
        }

       

      

        #endregion

        public DiagramLogic()
        {
            // The list of people, this is a global list shared by the application.
            family = GlobalData.Family;

            Clear();
        }

        #region get people

        /// <summary>
        /// Return a list of parents for the people in the specified row.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static Collection<Shape> GetParents(DiagramRow row)
        {
            // List that is returned.
            Collection<Shape> list = new Collection<Shape>();

            // Get possible children in the row.
            List<Shape> rowList = GetPrimaryAndRelatedPeople(row);

            // Add each parent to the list, make sure the parent is only added once.
            foreach (Shape person in rowList)
            {
                foreach (Shape parent in person.Parents)
                {
                    if (!list.Contains(parent))
                        list.Add(parent);
                }
            }

            return list;
        }

        /// <summary>
        /// Return a list of children for the people in the specified row.
        /// </summary>
        public static List<Shape> GetChildren(DiagramRow row)
        {
            // List that is returned.
            List<Shape> list = new List<Shape>();

            // Get possible parents in the row.
            List<Shape> rowList = GetPrimaryAndRelatedPeople(row);

            // Add each child to the list, make sure the child is only added once.
            foreach (Shape person in rowList)
            {
                foreach (Shape child in person.Children)
                {
                    if (!list.Contains(child))
                        list.Add(child);
                }
            }

            return list;
        }

        /// <summary>
        /// Return list of people in the row that are primary or related node types.
        /// </summary>
        private static List<Shape> GetPrimaryAndRelatedPeople(DiagramRow row)
        {
            List<Shape> list = new List<Shape>();
            foreach (DiagramGroup group in row.Groups)
            {
                foreach (DiagramNode node in group.Nodes)
                {
                    if (node.Type == NodeType.Related || node.Type == NodeType.Primary)
                        list.Add(node.Shape);
                }
            }

            return list;
        }

        /// <summary>
        /// Remove any people from the 'other' list from the 'people' list.
        /// </summary>
        private static void RemoveDuplicates(Collection<Shape> people, Collection<Shape> other)
        {
            foreach (Shape person in other)
                people.Remove(person);
        }

        #endregion

        #region create nodes

        /// <summary>
        /// Create a DiagramNode.
        /// </summary>
        private DiagramNode CreateNode(Shape person, NodeType type, bool clickEvent, double scale)
        {
            DiagramNode node = CreateNode(person, type, clickEvent);
            node.Scale = scale;
            return node;
        }

        /// <summary>
        /// Create a DiagramNode.
        /// </summary>
        private DiagramNode CreateNode(Shape person, NodeType type, bool clickEvent)
        {
            DiagramNode node = new DiagramNode();
            node.Shape = person;
            node.Type = type;
            if (clickEvent)
                node.Click += nodeClickHandler;

            return node;
        }

        /// <summary>
        /// Add the siblings to the specified row and group.
        /// </summary>
        private void AddSiblingNodes(DiagramRow row, DiagramGroup group,
            Collection<Shape> siblings, NodeType nodeType, double scale)
        {
            foreach (Shape sibling in siblings)
            {
                if (!personLookup.ContainsKey(sibling))
                {
                    // Siblings node.
                    DiagramNode node = CreateNode(sibling, nodeType, true, scale);
                    group.Add(node);
                    personLookup.Add(node.Shape, new DiagramConnectorNode(node, group, row));
                }
            }
        }

        /// <summary>
        /// Add the spouses to the specified row and group.
        /// </summary>
        private void AddSpouseNodes(Shape person, DiagramRow row,
            DiagramGroup group, Collection<Shape> spouses,
            NodeType nodeType, double scale, bool married)
        {
            foreach (Shape spouse in spouses)
            {
                if (!personLookup.ContainsKey(spouse))
                {
                    // Spouse node.
                    DiagramNode node = CreateNode(spouse, nodeType, true, scale);
                    group.Add(node);

                    // Add connection.
                    DiagramConnectorNode connectorNode = new DiagramConnectorNode(node, group, row);
                    personLookup.Add(node.Shape, connectorNode);
                    connections.Add(new MarriedDiagramConnector(married, personLookup[person], connectorNode));
                }
            }
        }

        #endregion

        #region create rows

        /// <summary>
        /// Creates the primary row. The row contains groups: 1) The primary-group 
        /// that only contains the primary node, and 2) The optional left-group 
        /// that contains spouses and siblings.
        /// </summary>
        public DiagramRow CreatePrimaryRow(Shape person, double scale, double scaleRelated)
        {
            // The primary node contains two groups, 
            DiagramGroup primaryGroup = new DiagramGroup();
            DiagramGroup leftGroup = new DiagramGroup();

            // Set up the row.
            DiagramRow row = new DiagramRow();

            // Add primary node.
            DiagramNode node = CreateNode(person, NodeType.Primary, false, scale);
            primaryGroup.Add(node);
            personLookup.Add(node.Shape, new DiagramConnectorNode(node, primaryGroup, row));

            // Current spouses.
            Collection<Shape> currentSpouses = person.CurrentSpouses;
            AddSpouseNodes(person, row, leftGroup, currentSpouses,
                NodeType.Spouse, scaleRelated, true);

            // Previous spouses.
            Collection<Shape> previousSpouses = person.PreviousSpouses;
            AddSpouseNodes(person, row, leftGroup, previousSpouses,
                NodeType.Spouse, scaleRelated, false);

            // Siblings.
            Collection<Shape> siblings = person.Siblings;
            AddSiblingNodes(row, leftGroup, siblings, NodeType.Sibling, scaleRelated);

            // Half siblings.
            Collection<Shape> halfSiblings = person.HalfSiblings;
            AddSiblingNodes(row, leftGroup, halfSiblings, NodeType.SiblingLeft, scaleRelated);

            if (leftGroup.Nodes.Count > 0)
            {
                leftGroup.Reverse();
                row.Add(leftGroup);
            }

            row.Add(primaryGroup);

            return row;
        }

        /// <summary>
        /// Create the child row. The row contains a group for each child. 
        /// Each group contains the child and spouses.
        /// </summary>
        public DiagramRow CreateChildrenRow(List<Shape> children, double scale, double scaleRelated)
        {
            // Setup the row.
            DiagramRow row = new DiagramRow();

            foreach (Shape child in children)
            {
                // Each child is in their group, the group contains the child 
                // and any spouses. The groups does not contain siblings.
                DiagramGroup group = new DiagramGroup();
                row.Add(group);

                // Child.
                if (!personLookup.ContainsKey(child))
                {
                    DiagramNode node = CreateNode(child, NodeType.Related, true, scale);
                    group.Add(node);
                    personLookup.Add(node.Shape, new DiagramConnectorNode(node, group, row));
                }

                // Current spouses.
                Collection<Shape> currentSpouses = child.CurrentSpouses;
                AddSpouseNodes(child, row, group, currentSpouses,
                    NodeType.Spouse, scaleRelated, true);

                // Previous spouses.
                Collection<Shape> previousSpouses = child.PreviousSpouses;
                AddSpouseNodes(child, row, group, previousSpouses,
                    NodeType.Spouse, scaleRelated, false);

                // Connections.
                AddParentConnections(child);

                group.Reverse();
            }

            return row;
        }

        /// <summary>
        /// Create the parent row. The row contains a group for each parent. 
        /// Each groups contains the parent, spouses and siblings.
        /// </summary>
        public DiagramRow CreateParentRow(Collection<Shape> parents, double scale, double scaleRelated)
        {
            // Set up the row.
            DiagramRow row = new DiagramRow();

            int groupCount = 0;

            foreach (Shape person in parents)
            {
                // Each parent is in their group, the group contains the parent,
                // spouses and siblings.
                DiagramGroup group = new DiagramGroup();
                row.Add(group);

                // Determine if this is a left or right oriented group.
                bool left = (groupCount++ % 2 == 0) ? true : false;

                // Parent.
                if (!personLookup.ContainsKey(person))
                {
                    DiagramNode node = CreateNode(person, NodeType.Related, true, scale);
                    group.Add(node);
                    personLookup.Add(node.Shape, new DiagramConnectorNode(node, group, row));
                }

                // Current spouses.
                Collection<Shape> currentSpouses = person.CurrentSpouses;
                RemoveDuplicates(currentSpouses, parents);
                AddSpouseNodes(person, row, group, currentSpouses,
                    NodeType.Spouse, scaleRelated, true);

                // Previous spouses.
                Collection<Shape> previousSpouses = person.PreviousSpouses;
                RemoveDuplicates(previousSpouses, parents);
                AddSpouseNodes(person, row, group, previousSpouses,
                    NodeType.Spouse, scaleRelated, false);

                // Siblings.
                Collection<Shape> siblings = person.Siblings;
                AddSiblingNodes(row, group, siblings, NodeType.Sibling, scaleRelated);

                // Half siblings.
                Collection<Shape> halfSiblings = person.HalfSiblings;
                AddSiblingNodes(row, group, halfSiblings, left ?
                    NodeType.SiblingLeft : NodeType.SiblingRight, scaleRelated);

                // Connections.
                AddChildConnections(person);
                AddChildConnections(currentSpouses);
                AddChildConnections(previousSpouses);

                if (left)
                    group.Reverse();
            }

            // Add connections that span across groups.
            AddSpouseConnections(parents);

            return row;
        }

        #endregion

        #region connections

        /// <summary>
        /// Add connections for each person and the person's children in the list.
        /// </summary>
        private void AddChildConnections(Collection<Shape> parents)
        {
            foreach (Shape person in parents)
                AddChildConnections(person);
        }

        /// <summary>
        /// Add connections between the child and child's parents.
        /// </summary>
        private void AddParentConnections(Shape child)
        {
            foreach (Shape parent in child.Parents)
            {
                if (personLookup.ContainsKey(parent) &&
                    personLookup.ContainsKey(child))
                {
                    connections.Add(new ChildDiagramConnector(
                        personLookup[parent], personLookup[child]));
                }
            }
        }

        /// <summary>
        /// Add connections between the parent and parent抯 children.
        /// </summary>
        private void AddChildConnections(Shape parent)
        {
            foreach (Shape child in parent.Children)
            {
                if (personLookup.ContainsKey(parent) &&
                    personLookup.ContainsKey(child))
                {
                    connections.Add(new ChildDiagramConnector(
                        personLookup[parent], personLookup[child]));
                }
            }
        }

        /// <summary>
        /// Add marriage connections for the people specified in the 
        /// list. Each marriage connection is only specified once.
        /// </summary>
        private void AddSpouseConnections(Collection<Shape> list)
        {
            // Iterate through the list. 
            for (int current = 0; current < list.Count; current++)
            {
                // The person to check for marriages.
                Shape person = list[current];

                // Check for current / former marriages in the rest of the list.                    
                for (int i = current + 1; i < list.Count; i++)
                {
                    Shape spouse = list[i];
                    SpouseRelationship rel = person.GetSpouseRelationship(spouse);

                    // Current marriage.
                    if (rel != null && rel.SpouseModifier == SpouseModifier.Current)
                    {
                        if (personLookup.ContainsKey(person) &&
                            personLookup.ContainsKey(spouse))
                        {
                            connections.Add(new MarriedDiagramConnector(true,
                                personLookup[person], personLookup[spouse]));
                        }
                    }

                    // Former marriage
                    if (rel != null && rel.SpouseModifier == SpouseModifier.Former)
                    {
                        if (personLookup.ContainsKey(person) &&
                            personLookup.ContainsKey(spouse))
                        {
                            connections.Add(new MarriedDiagramConnector(false,
                                personLookup[person], personLookup[spouse]));
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Clear 
        /// </summary>
        public void Clear()
        {
            // Remove any event handlers from the nodes. Otherwise 
            // the delegate maintains a reference to the object 
            // which can hinder garbage collection. 
            foreach (DiagramConnectorNode node in personLookup.Values)
                node.Node.Click -= nodeClickHandler;

            // Clear the connection info.
            connections.Clear();
            personLookup.Clear();

            // Time filter.
            displayYear = DateTime.Now.Year;
        }

        /// <summary>
        /// Return the DiagramNode for the specified Person.
        /// </summary>
        public DiagramNode GetDiagramNode(Shape person)
        {
            if (person == null)
                return null;

            if (!personLookup.ContainsKey(person))
                return null;

            return personLookup[person].Node;
        }

        /// <summary>
        /// Return the bounds (relative to the diagram) for the specified person.
        /// </summary>
        public Rect GetNodeBounds(Shape person)
        {
            Rect bounds = Rect.Empty;
            if (person != null && personLookup.ContainsKey(person))
            {
                DiagramConnectorNode connector = personLookup[person];
                bounds = new Rect(connector.TopLeft.X, connector.TopLeft.Y,
                    connector.Node.ActualWidth, connector.Node.ActualHeight);
            }

            return bounds;
        }
    }
}
