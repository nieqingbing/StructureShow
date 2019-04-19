using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static StructureShow.Main.Model.Comments;

namespace StructureShow.Main.Model
{
    public class Shape
    {
        #region Properties
        private string id;
        [XmlAttribute]
        public string Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        private PhotoCollection photos;
        public PhotoCollection Photos
        {
            get { return photos; }
        }

        private Comments comments;
        public Comments Comments
        {
            get { return comments; }
            set
            {
                if (comments != value)
                {
                    comments = value;
                    OnPropertyChanged("Comments");
                }
            }
        }

        /// <summary>
        /// Gets or sets the person's graphical identity
        /// </summary>
        [XmlIgnore, System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public string Avatar
        {
            get
            {
                string avatar = "";

                if (photos != null && photos.Count > 0)
                {
                    foreach (Photo photo in photos)
                    {
                        if (photo.IsAvatar)
                            return photo.FullyQualifiedPath;
                    }
                }

                return avatar;
            }
            set
            {
                // This setter is used for change notification.
                OnPropertyChanged("Avatar");
                OnPropertyChanged("HasAvatar");
            }
        }


        [XmlIgnore]
        public bool IsDeletable
        {
            get
            {
                return false;
            }
        }

         private RelationshipCollection relationships;
        public RelationshipCollection Relationships
        {
            get { return relationships; }
        }

       
        [XmlIgnore]
        public Collection<Shape> Input
        {
            get
            {
                Collection<Shape> spouses = new Collection<Shape>();
                foreach (Relationship rel in relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Spouse)
                        spouses.Add(rel.RelationTo);
                }
                return spouses;
            }
        }

        [XmlIgnore]
        public Collection<Shape> CurrentSpouses
        {
            get
            {
                Collection<Shape> spouses = new Collection<Shape>();
                foreach (Relationship rel in relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Spouse)
                    {
                        SpouseRelationship spouseRel = rel as SpouseRelationship;
                        if (spouseRel != null && spouseRel.SpouseModifier == SpouseModifier.Current)
                            spouses.Add(rel.RelationTo);
                    }
                }
                return spouses;
            }
        }

        [XmlIgnore]
        public Collection<Shape> PreviousSpouses
        {
            get
            {
                Collection<Shape> spouses = new Collection<Shape>();
                foreach (Relationship rel in relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Spouse)
                    {
                        SpouseRelationship spouseRel = rel as SpouseRelationship;
                        if (spouseRel != null && spouseRel.SpouseModifier == SpouseModifier.Former)
                            spouses.Add(rel.RelationTo);
                    }
                }
                return spouses;
            }
        }

        /// <summary>
        /// Accessor for the person's children
        /// </summary>
        [XmlIgnore]
        public Collection<Shape> Children
        {
            get
            {
                Collection<Shape> children = new Collection<Shape>();
                foreach (Relationship rel in relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Child)
                        children.Add(rel.RelationTo);
                }
                return children;
            }
        }

        /// <summary>
        /// Accessor for all of the person's parents
        /// </summary>
        [XmlIgnore]
        public Collection<Shape> Parents
        {
            get
            {
                Collection<Shape> parents = new Collection<Shape>();
                foreach (Relationship rel in relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Parent)
                        parents.Add(rel.RelationTo);
                }
                return parents;
            }
        }

        /// <summary>
        /// Accessor for the person's siblings
        /// </summary>
        [XmlIgnore]
        public Collection<Shape> Siblings
        {
            get
            {
                Collection<Shape> siblings = new Collection<Shape>();
                foreach (Relationship rel in relationships)
                {
                    if (rel.RelationshipType == RelationshipType.Sibling)
                        siblings.Add(rel.RelationTo);
                }
                return siblings;
            }
        }

        /// <summary>
        /// Accessor for the person's half siblings. A half sibling is a person
        /// that contains one or more same parents as the person, but does not 
        /// contain all of the same parents.
        /// </summary>
        [XmlIgnore]
        public Collection<Shape> HalfSiblings
        {
            get
            {
                // List that is returned.
                Collection<Shape> halfSiblings = new Collection<Shape>();

                // Get list of full siblings (a full sibling cannot be a half sibling).
                Collection<Shape> siblings = this.Siblings;

                // Iterate through each parent, and determine if the parent's children
                // are half siblings.
                foreach (Shape parent in Parents)
                {
                    foreach (Shape child in parent.Children)
                    {
                        if (child != this && !siblings.Contains(child) &&
                            !halfSiblings.Contains(child))
                        {
                            halfSiblings.Add(child);
                        }
                    }
                }

                return halfSiblings;
            }
        }

        /// <summary>
        /// Get the person's parents as a ParentSet object
        /// </summary>
        [XmlIgnore]
        public ParentSet ParentSet
        {
            get
            {
                // Only need to get the parent set if there are two parents.
                if (Parents.Count == 2)
                {
                    ParentSet parentSet = new ParentSet(Parents[0], Parents[1]);
                    return parentSet;
                }
                else return null;
            }
        }


        /// <summary>
        /// Calculated property that returns whether the person has 2 or more parents.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "value")]
        public bool HasParents
        {
            get
            {
                return (Parents.Count >= 2);
            }
            set
            {
                // This setter is used for change notification.
                OnPropertyChanged("HasParents");
            }
        }

        /// <summary>
        /// Calculated property that returns whether the person has an avatar photo.
        /// </summary>
        [XmlIgnore]
        public bool HasAvatar
        {
            get
            {
                if (photos != null && photos.Count > 0)
                {
                    foreach (Photo photo in photos)
                    {
                        if (photo.IsAvatar)
                            return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Calculated property that returns string text for this person's parents
        /// </summary>
        [XmlIgnore]
        public string ParentsText
        {
            get
            {
                Collection<Shape> parents = Parents;

                string parentsText = string.Empty;
                if (parents.Count > 0)
                {
                    parentsText = parents[0].Name;

                    if (parents.Count == 2)
                        parentsText += " and " + parents[1].Name;
                    else
                    {
                        for (int i = 1; i < parents.Count; i++)
                        {
                            if (i == parents.Count - 1)
                                parentsText += ", and " + parents[i].Name;
                            else
                                parentsText += ", " + parents[i].Name;
                        }
                    }
                }
                return parentsText;
            }
        }

    

        /// <summary>
        /// Calculated property that returns string text for this person's siblings
        /// </summary>
        [XmlIgnore]
        public string SiblingsText
        {
            get
            {
                Collection<Shape> siblings = Siblings;

                string siblingsText = string.Empty;
                if (siblings.Count > 0)
                {
                    siblingsText = siblings[0].Name;

                    if (siblings.Count == 2)
                        siblingsText += " and " + siblings[1].Name;
                    else
                    {
                        for (int i = 1; i < siblings.Count; i++)
                        {
                            if (i == siblings.Count - 1)
                                siblingsText += ", and " + siblings[i].Name;
                            else
                                siblingsText += ", " + siblings[i].Name;
                        }
                    }
                }
                return siblingsText;
            }
        }

      
        /// <summary>
        /// Calculated property that returns string text for this person's children.
        /// </summary>
        [XmlIgnore]
        public string ChildrenText
        {
            get
            {
                Collection<Shape> children = Children;

                string childrenText = string.Empty;
                if (children.Count > 0)
                {
                    childrenText = children[0].Name;

                    if (children.Count == 2)
                        childrenText += " and " + children[1].Name;
                    else
                    {
                        for (int i = 1; i < children.Count; i++)
                        {
                            if (i == children.Count - 1)
                                childrenText += ", and " + children[i].Name;
                            else
                                childrenText += ", " + children[i].Name;
                        }
                    }
                }
                return childrenText;
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of a person object.
        /// Each new instance will be given a unique identifier.
        /// This parameterless constructor is also required for serialization.
        /// </summary>
        public Shape()
        {
            this.id = Guid.NewGuid().ToString();
            this.relationships = new RelationshipCollection();
            this.photos = new PhotoCollection();
            this.name = GlobalData.DefaultName;
        }

        /// <summary>
        /// Creates a new instance of the person class with the firstname and the lastname.
        /// </summary>
        public Shape(string name) : this()
        {
            if (!string.IsNullOrEmpty(name))
                this.name = name;
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// INotifyPropertyChanged requires a property called PropertyChanged.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Fires the event for the property when it changes.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IEquatable Members

        /// <summary>
        /// Determine equality between two person classes
        /// </summary>
        public bool Equals(Shape other)
        {
            return (this.Id == other.Id);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the spouse relationship for the specified spouse.
        /// </summary>
        public SpouseRelationship GetSpouseRelationship(Shape spouse)
        {
            foreach (Relationship relationship in this.relationships)
            {
                SpouseRelationship spouseRelationship = relationship as SpouseRelationship;
                if (spouseRelationship != null)
                {
                    if (spouseRelationship.RelationTo.Equals(spouse))
                        return spouseRelationship;
                }
            }

            return null;
        }

        /// <summary>
        /// Called to delete the person's photos
        /// </summary>
        public void DeletePhotos()
        {
            // Delete the person's photos
            foreach (Photo photo in this.photos)
            {
                photo.Delete();
            }
        }

        /// <summary>
        /// Called to delete the person's story
        /// </summary>
        public void DeleteStory()
        {
            if (this.comments != null)
            {
                this.comments.Delete();
                this.comments = null;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }
        #endregion
    }
}
