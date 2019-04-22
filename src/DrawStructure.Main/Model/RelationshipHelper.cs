using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawStructure.Main
{
    /// <summary>
    /// Contains the relationship logic rules for adding people and how they relate to each other.
    /// </summary>
    public static class RelationshipHelper
    {
        /// <summary>
        /// Performs the business logic for adding the Child relationship between the person and the child.
        /// </summary>
        public static void AddChild(UMLCollection family, Shape shape, Shape child)
        {
            // Add the new child as a sibling to any existing children
            foreach (Shape existingSibling in shape.Children)
            {
                family.AddSibling(existingSibling, child);
            }

         
        }

        /// <summary>
        /// Performs the business logic for adding the Parent relationship between the person and the parent.
        /// </summary>
        public static void AddParent(UMLCollection family, Shape shape, Shape parent)
        {

            // Add the parent to the main collection of people.
            family.Add(parent);

            // Setter for property change notification
            shape.HasParents = true;
        }

        /// <summary>
        /// Performs the business logic for adding the Parent relationship between the person and the parents.
        /// </summary>
        public static void AddParent(UMLCollection family, Shape shape, ParentSet parentSet)
        {
            // First add child to parents.
            family.AddChild(parentSet.FirstParent, shape, ParentChildModifier.Natural);
            family.AddChild(parentSet.SecondParent, shape, ParentChildModifier.Natural);

            // Next update the siblings. Get the list of full siblings for the person. 
            // A full sibling is a sibling that has both parents in common. 
            List<Shape> siblings = GetChildren(parentSet);
            foreach (Shape sibling in siblings)
            {
                if (sibling != shape)
                    family.AddSibling(shape, sibling);
            }
        }

        /// <summary>
        /// Return a list of children for the parent set.
        /// </summary>
        private static List<Shape> GetChildren(ParentSet parentSet)
        {
            // Get list of both parents.
            List<Shape> firstParentChildren = new List<Shape>(parentSet.FirstParent.Children);
            List<Shape> secondParentChildren = new List<Shape>(parentSet.SecondParent.Children);

            // Combined children list that is returned.
            List<Shape> children = new List<Shape>();

            // Go through and add the children that have both parents.            
            foreach (Shape child in firstParentChildren)
            {
                if (secondParentChildren.Contains(child))
                    children.Add(child);
            }

            return children;
        }

        /// <summary>
        /// Performs the business logic for adding the Spousal relationship between the person and the spouse.
        /// </summary>
        public static void AddSpouse(UMLCollection family, Shape person, Shape spouse, SpouseModifier modifier)
        {
           
        }

        /// <summary>
        /// Performs the business logic for adding the Sibling relationship between the person and the sibling.
        /// </summary>
        public static void AddSibling(UMLCollection family, Shape person, Shape sibling)
        {
            // Handle siblings
            if (person.Siblings.Count > 0)
            {
                // Make the siblings siblings to each other.
                foreach (Shape existingSibling in person.Siblings)
                {
                    family.AddSibling(existingSibling, sibling);
                }
            }

            if (person.Parents != null)
            {
                switch (person.Parents.Count)
                {
                    // No parents
                    case 0:
                        family.AddSibling(person, sibling);
                        break;

                    // Single parent
                    case 1:
                        family.AddSibling(person, sibling);
                        family.AddChild(person.Parents[0], sibling, ParentChildModifier.Natural);
                        break;

                    // 2 parents
                    case 2:
                        // Add the sibling as a child of the same parents
                        foreach (Shape parent in person.Parents)
                        {
                            family.AddChild(parent, sibling, ParentChildModifier.Natural);
                        }

                        family.AddSibling(person, sibling);
                        break;

                    default:
                        family.AddSibling(person, sibling);
                        break;
                }
            }
        }

      

       

       
        /// <summary>
        /// Performs the business logic for changing the person parents
        /// </summary>
        public static void ChangeParents(UMLCollection family, Shape shape, ParentSet newParentSet)
        {
            // Don't do anything if there is nothing to change or if the parents are the same
            if (shape.ParentSet == null || newParentSet == null || shape.ParentSet.Equals(newParentSet))
                return;

            // store the current parent set which will be removed
            ParentSet formerParentSet = shape.ParentSet;

            // Remove the first parent
            RemoveParentChildRelationship(shape, formerParentSet.FirstParent);

            // Remove the person as a child of the second parent
            RemoveParentChildRelationship(shape, formerParentSet.SecondParent);

            // Remove the sibling relationships
            RemoveSiblingRelationships(shape);

            // Add the new parents
            AddParent(family, shape, newParentSet);
        }

        /// <summary>
        /// Helper function for removing sibling relationships
        /// </summary>
        private static void RemoveSiblingRelationships(Shape shape)
        {
            for (int i = shape.Relationships.Count - 1; i >= 0; i--)
            {
                if (shape.Relationships[i].RelationshipType == RelationshipType.Sibling)
                    shape.Relationships.RemoveAt(i);
            }
        }

        /// <summary>
        /// Helper function for removing a parent relationship
        /// </summary>
        private static void RemoveParentChildRelationship(Shape person, Shape parent)
        {
            foreach (Relationship relationship in person.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Parent && relationship.RelationTo.Equals(parent))
                {
                    person.Relationships.Remove(relationship);
                    break;
                }
            }

            foreach (Relationship relationship in parent.Relationships)
            {
                if (relationship.RelationshipType == RelationshipType.Child && relationship.RelationTo.Equals(person))
                {
                    parent.Relationships.Remove(relationship);
                    break;
                }
            }
        }

        /// <summary>
        /// Performs the business logic for changing the deleting the person
        /// </summary>
        public static void DeletePerson(UMLCollection family, Shape personToDelete)
        {
            if (!personToDelete.IsDeletable)
                return;

            // Remove the personToDelete from the relationships that contains the personToDelete.
            foreach (Relationship relationship in personToDelete.Relationships)
            {
                foreach (Relationship rel in relationship.RelationTo.Relationships)
                {
                    if (rel.RelationTo.Equals(personToDelete))
                    {
                        relationship.RelationTo.Relationships.Remove(rel);
                        break;
                    }
                }
            }

            // Delete the person's photos and story
            personToDelete.DeletePhotos();
            personToDelete.DeleteStory();

            family.Remove(personToDelete);
        }
    }
}
