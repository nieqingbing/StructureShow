﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DrawStructure.Main
{
    #region Relationship classes

    [Serializable]
    public abstract class Relationship
    {
        private RelationshipType relationshipType;

        private Shape relationTo;

        // The person's Id will be serialized instead of the relationTo person object to avoid
        // circular references during Xml Serialization. When family data is loaded, the corresponding
        // person object will be assigned to the relationTo property (please see app.xaml.cs).
        private string shapeId;

        // Store the person's name with the Id to make the xml file more readable
        private string shapeName;

        /// <summary>
        /// The Type of relationship.  Parent, child, sibling, or spouse
        /// </summary>
        public RelationshipType RelationshipType
        {
            get { return relationshipType; }
            set { relationshipType = value; }
        }

        /// <summary>
        /// The person id the relationship is to. See comment on personId above.
        /// </summary>
        [XmlIgnore]
        public Shape RelationTo
        {
            get { return relationTo; }
            set
            {
                relationTo = value;
                shapeId = ((Shape)value).Id;
                shapeName = ((Shape)value).Name;
            }
        }

        public string ShapeId
        {
            get { return shapeId; }
            set { shapeId = value; }
        }

        public string ShapeName
        {
            get { return shapeName; }
            set { shapeName = value; }
        }
    }

    /// <summary>
    /// Describes the kinship between a child and parent.
    /// </summary>
    [Serializable]
    public class ParentRelationship : Relationship
    {
        // The ParentChildModifier are not currently used by the application
        private ParentChildModifier parentChildModifier;
        public ParentChildModifier ParentChildModifier
        {
            get { return parentChildModifier; }
            set { parentChildModifier = value; }
        }

        // Paramaterless constructor required for XML serialization
        public ParentRelationship() { }

        public ParentRelationship(Shape personId, ParentChildModifier parentChildType)
        {
            RelationshipType = RelationshipType.Parent;
            this.RelationTo = personId;
            this.parentChildModifier = parentChildType;
        }

        public override string ToString()
        {
            return RelationTo.Name;
        }
    }

    /// <summary>
    /// Describes the kindship between a parent and child.
    /// </summary>
    [Serializable]
    public class ChildRelationship : Relationship
    {
        // The ParentChildModifier are not currently used by the application
        private ParentChildModifier parentChildModifier;
        public ParentChildModifier ParentChildModifier
        {
            get { return parentChildModifier; }
            set { parentChildModifier = value; }
        }

        // Paramaterless constructor required for XML serialization
        public ChildRelationship() { }

        public ChildRelationship(Shape person, ParentChildModifier parentChildType)
        {
            RelationshipType = RelationshipType.Child;
            this.RelationTo = person;
            this.parentChildModifier = parentChildType;
        }
    }

    /// <summary>
    /// Describes the kindship between a couple
    /// </summary>
    [Serializable]
    public class SpouseRelationship : Relationship
    {
        private SpouseModifier spouseModifier;
        private DateTime? marriageDate;
        private DateTime? divorceDate;

        public SpouseModifier SpouseModifier
        {
            get { return spouseModifier; }
            set { spouseModifier = value; }
        }

        public DateTime? MarriageDate
        {
            get { return marriageDate; }
            set { marriageDate = value; }
        }

        public DateTime? DivorceDate
        {
            get { return divorceDate; }
            set { divorceDate = value; }
        }

        // Paramaterless constructor required for XML serialization
        public SpouseRelationship() { }

        public SpouseRelationship(Shape person, SpouseModifier spouseType)
        {
            RelationshipType = RelationshipType.Spouse;
            this.spouseModifier = spouseType;
            this.RelationTo = person;
        }
    }

    /// <summary>
    /// Describes the kindship between a siblings
    /// </summary>
    [Serializable]
    public class SiblingRelationship : Relationship
    {
        // Paramaterless constructor required for XML serialization
        public SiblingRelationship() { }

        public SiblingRelationship(Shape person)
        {
            RelationshipType = RelationshipType.Sibling;
            this.RelationTo = person;
        }
    }

    #endregion

    #region Relationships collection

    /// <summary>
    /// Collection of relationship for a person object
    /// </summary>
    [Serializable]
    public class RelationshipCollection : ObservableCollection<Relationship> { }

    #endregion

    #region Relationship Type/Modifier Enums

    /// <summary>
    /// Enumeration of connection types between person objects
    /// </summary>
    public enum RelationshipType
    {
        Child,
        Parent,
        Sibling,
        Spouse
    }

    public enum SpouseModifier
    {
        Current,
        Former
    }

    public enum ParentChildModifier
    {
        Natural,
        Adopted,
        Foster
    }

    #endregion
}
