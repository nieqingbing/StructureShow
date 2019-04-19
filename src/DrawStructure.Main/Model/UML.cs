using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StructureShow.Main.Model
{
    public class ContentChangedEventArgs : EventArgs
    {
        private Shape newShape;

        public Shape NewShape
        {
            get { return newShape; }
        }

        public ContentChangedEventArgs(Shape newPerson)
        {
            this.newShape = newPerson;
        }
    }

    /// <summary>
    /// Contains the collection of person nodes and which person in the list is the currently
    /// selected person. This class exists mainly because of xml serialization limitations.
    /// Properties are not serialized in a class that is derived from a collection class 
    /// (as the PeopleCollection class is). Therefore the People collection is contained in 
    /// this class, along with other important properties that need to be serialized.
    /// </summary>
    [XmlRoot("Root")]
    [XmlInclude(typeof(ParentRelationship))]
    [XmlInclude(typeof(ChildRelationship))]
    [XmlInclude(typeof(SpouseRelationship))]
    [XmlInclude(typeof(SiblingRelationship))]
    public class UML
    {
        #region fields and constants

        // The constants specific to this class
        private static class Const
        {
            public const string DataFileName = "default.family";
        }


        // The fully qualified path and filename for the family file.
        private string fullyQualifiedFilename;

        // Version of the file. This is not used at this time, but allows a future
        // version of the application to handle previous file formats.
        private string fileVersion = "1.0";

        private string OPCContentFileName = "content.xml";

        #endregion

        #region Properties

        private UMLCollection umlCollection;
        public UMLCollection UMLCollection
        {
            get { return umlCollection; }
        }

        private string currentShapeId;
        [XmlAttribute(AttributeName = "Current")]
        public string CurrentShapeId
        {
            get { return currentShapeId; }
            set { currentShapeId = value; }
        }

        private string currentShapeName;
        // Name of current selected Shape (included for readability in xml file).
        [XmlAttribute(AttributeName = "CurrentName")]
        public string CurrentShapeName
        {
            get { return currentShapeName; }
            set { currentShapeName = value; }
        }

        // Version of the file.
        [XmlAttribute(AttributeName = "FileVersion")]
        public string Version
        {
            get { return fileVersion; }
            set { fileVersion = value; }
        }

        [XmlIgnore]
        public static string ApplicationFolderPath
        {
            get
            {
                return Path.Combine(
                  Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                  GlobalData.ApplicationFolderName);
            }
        }

        [XmlIgnore]
        public static string DefaultFullyQualifiedFilename
        {
            get
            {
                // Absolute path to the application folder
                string appLocation = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    GlobalData.ApplicationFolderName);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(appLocation))
                    Directory.CreateDirectory(appLocation);

                return Path.Combine(appLocation, Const.DataFileName);
            }
        }

        /// <summary>
        /// Fully qualified filename (absolute pathname and filename) for the data file
        /// </summary>
        [XmlIgnore]
        public string FullyQualifiedFilename
        {
            get { return fullyQualifiedFilename; }

            set { fullyQualifiedFilename = value; }
        }

        #endregion

        public UML()
        {
            umlCollection = new UMLCollection();
        }

        #region Loading and saving

        /// <summary>
        /// Persist the current list of people to disk.
        /// </summary>
        public void Save()
        {
            // Return right away if nothing to save.
            if (this.UMLCollection == null || this.UMLCollection.Count == 0)
                return;

            // Set the current person id and name before serializing
            this.CurrentShapeName = this.UMLCollection.Current.Name;
            this.CurrentShapeId = this.UMLCollection.Current.Id;

            // Use the default path and filename if none was provided
            if (string.IsNullOrEmpty(this.FullyQualifiedFilename))
                this.FullyQualifiedFilename = UML.DefaultFullyQualifiedFilename;

            // Setup temp folders for this family to be packaged into OPC later
            string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                GlobalData.ApplicationFolderName);
            tempFolder = Path.Combine(tempFolder, GlobalData.AppDataFolderName);

            // Create the necessary directories
            Directory.CreateDirectory(tempFolder);

            // Create xml content file
            XmlSerializer xml = new XmlSerializer(typeof(UML));
            using (Stream stream = new FileStream(Path.Combine(tempFolder, OPCContentFileName), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                xml.Serialize(stream, this);
            }

            // save to file package
            OPCUtility.CreatePackage(FullyQualifiedFilename, tempFolder);

            this.UMLCollection.IsDirty = false;
        }

        /// <summary>
        /// Saves the list of people to disk using the specified filename and path
        /// </summary>
        /// <param name="FQFilename">Fully qualified path and filename of family tree file to save</param>
        public void Save(string FQFilename)
        {
            this.fullyQualifiedFilename = FQFilename;
            Save();
        }

        /// <summary>
        /// Load the list of people from Family.Show version 2.0 file format
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void LoadVersion2()
        {
            // Loading, clear existing nodes
            this.UMLCollection.Clear();

            try
            {
                // Use the default path and filename if none were provided
                if (string.IsNullOrEmpty(this.FullyQualifiedFilename))
                {
                    this.FullyQualifiedFilename = UML.DefaultFullyQualifiedFilename;
                }

                XmlSerializer xml = new XmlSerializer(typeof(UML));
                using (Stream stream = new FileStream(this.FullyQualifiedFilename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    UML pc = (UML)xml.Deserialize(stream);
                    stream.Close();

                    foreach (Shape shape in pc.UMLCollection)
                    {
                        this.UMLCollection.Add(shape);
                    }

                    // Setup temp folders for this family to be packaged into OPC later
                    string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        GlobalData.ApplicationFolderName);
                    tempFolder = Path.Combine(tempFolder, GlobalData.AppDataFolderName);
                    RecreateDirectory(tempFolder);

                    string photoFolder = Path.Combine(tempFolder, Photo.Const.PhotosFolderName);
                    RecreateDirectory(photoFolder);

                    string storyFolder = Path.Combine(tempFolder, GlobalData.CommentsFolderName);
                    RecreateDirectory(storyFolder);

                    foreach (Shape p in this.UMLCollection)
                    {
                        // To avoid circular references when serializing family data to xml, only the person Id
                        // is seralized to express relationships. When family data is loaded, the correct
                        // person object is found using the person Id and assigned to the appropriate relationship.
                        foreach (Relationship r in p.Relationships)
                        {
                            r.RelationTo = this.UMLCollection.Find(r.ShapeId);
                        }

                        // store the stories into temp directory to be packaged into OPC later
                        foreach (Photo photo in p.Photos)
                        {
                            string photoOldPath = Path.Combine(Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                GlobalData.ApplicationFolderName), photo.RelativePath);
                            if (File.Exists(photoOldPath))
                            {
                                string photoFile = Path.Combine(photoFolder, Path.GetFileName(photo.FullyQualifiedPath));

                                // Remove spaces since they'll be packaged as %20, breaking relative paths that expect spaces
                                photoFile = photoFile.Replace(" ", "");
                                photo.RelativePath = photo.RelativePath.Replace(" ", "");

                                File.Copy(photoOldPath, photoFile, true);
                            }
                        }

                      
                        if (p.Comments != null)
                        {
                            string storyOldPath = Path.Combine(Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                GlobalData.ApplicationFolderName), p.Comments.RelativePath);
                            if (File.Exists(storyOldPath))
                            {
                                string storyFile = Path.Combine(storyFolder, Path.GetFileName(p.Comments.AbsolutePath));

                                // Remove spaces since they'll be packaged as %20, breaking relative paths that expect spaces
                                storyFile = ReplaceEncodedCharacters(storyFile);
                                p.Comments.RelativePath = ReplaceEncodedCharacters(p.Comments.RelativePath);

                                File.Copy(storyOldPath, storyFile, true);
                            }
                        }
                    }

                    // Set the current person in the list
                    this.CurrentShapeId = pc.CurrentShapeId;
                    this.CurrentShapeName = pc.currentShapeName;
                    this.UMLCollection.Current = this.UMLCollection.Find(this.CurrentShapeId);
                }

                this.UMLCollection.IsDirty = false;
                return;
            }
            catch (Exception)
            {
                // Could not load the file. Handle all exceptions
                // the same, ignore and continue.
                this.fullyQualifiedFilename = string.Empty;
            }
        }

        private static string ReplaceEncodedCharacters(string fileName)
        {
            fileName = fileName.Replace(" ", "");
            fileName = fileName.Replace("{", "");
            fileName = fileName.Replace("}", "");
            return fileName;
        }

        /// <summary>
        /// Delete to clear existing files and re-Create the necessary directories
        /// </summary>
        public static void RecreateDirectory(string folderToDelete)
        {
            try
            {
                if (Directory.Exists(folderToDelete))
                {
                    Directory.Delete(folderToDelete, true);
                }

                Directory.CreateDirectory(folderToDelete);
            }
            catch
            {
                // ignore deletion errors
            }
        }

        /// <summary>
        /// Load the list of people from disk using the Open Package Convention format
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void LoadOPC()
        {
            // Loading, clear existing nodes
            this.UMLCollection.Clear();

            try
            {
                // Use the default path and filename if none were provided
                if (string.IsNullOrEmpty(this.FullyQualifiedFilename))
                {
                    this.FullyQualifiedFilename = UML.DefaultFullyQualifiedFilename;
                }

                string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    GlobalData.ApplicationFolderName);
                tempFolder = Path.Combine(tempFolder, GlobalData.AppDataFolderName + @"\");

                OPCUtility.ExtractPackage(FullyQualifiedFilename, tempFolder);

                XmlSerializer xml = new XmlSerializer(typeof(UML));
                using (Stream stream = new FileStream(tempFolder + OPCContentFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    UML pc = (UML)xml.Deserialize(stream);
                    stream.Close();

                    foreach (Shape shape in pc.UMLCollection)
                    {
                        this.UMLCollection.Add(shape);
                    }

                    // To avoid circular references when serializing family data to xml, only the person Id
                    // is seralized to express relationships. When family data is loaded, the correct
                    // person object is found using the person Id and assigned to the appropriate relationship.
                    foreach (Shape p in this.UMLCollection)
                    {
                        foreach (Relationship r in p.Relationships)
                        {
                            r.RelationTo = this.UMLCollection.Find(r.ShapeId);
                        }
                    }

                    // Set the current person in the list
                    this.CurrentShapeId = pc.CurrentShapeId;
                    this.CurrentShapeName = pc.CurrentShapeName;
                    this.UMLCollection.Current = this.UMLCollection.Find(this.CurrentShapeId);
                }

                this.UMLCollection.IsDirty = false;
                return;
            }
            catch
            {
                // Could not load the file. Handle all exceptions
                // the same, ignore and continue.
                this.fullyQualifiedFilename = string.Empty;
            }
        }

        #endregion
    }


    /// <summary>
    /// List of people.
    /// </summary>
    [Serializable]
    public class UMLCollection : ObservableCollection<Shape>, INotifyPropertyChanged
    {
        public UMLCollection() { }

        private Shape current;
        private bool dirty;

        /// <summary>
        /// Person currently selected in application
        /// </summary>
        public Shape Current
        {
            get { return current; }
            set
            {
                if (current != value)
                {
                    current = value;
                    OnPropertyChanged("Current");
                    OnCurrentChanged();
                }
            }
        }

        /// <summary>
        /// Get or set if the list has been modified.
        /// </summary>
        public bool IsDirty
        {
            get { return dirty; }
            set { dirty = value; }
        }

        public bool IsOldVersion { get; set; }

        /// <summary>
        /// A person or relationship was added, removed or modified in the list. This is used
        /// instead of CollectionChanged since CollectionChanged can be raised before the 
        /// relationships are setup (the Person was added to the list, but its Parents, Children,
        /// Sibling and Spouse collections have not been established). This means the subscriber 
        /// (the diagram control) will update before all of the information is available and 
        /// relationships will not be displayed.
        /// 
        /// The ContentChanged event addresses this problem and allows the flexibility to
        /// raise the event after *all* people have been added to the list, and *all* of
        /// their relationships have been established. 
        /// 
        /// Objects that add or remove people from the list, or add or remove relationships
        /// should call OnContentChanged when they want to notify subscribers that all
        /// changes have been made.
        /// </summary>
        public event EventHandler<ContentChangedEventArgs> ContentChanged;

        /// <summary>
        /// The details of a person changed.
        /// </summary>
        public void OnContentChanged()
        {
            dirty = true;
            if (ContentChanged != null)
                ContentChanged(this, new ContentChangedEventArgs(null));
        }

        /// <summary>
        /// The details of a person changed, and a new person was added to the collection.
        /// </summary>
        public void OnContentChanged(Shape newPerson)
        {
            dirty = true;
            if (ContentChanged != null)
                ContentChanged(this, new ContentChangedEventArgs(newPerson));
        }

        /// <summary> 
        /// The primary person changed in the list.
        /// </summary>
        public event EventHandler CurrentChanged;
        protected void OnCurrentChanged()
        {
            if (CurrentChanged != null)
                CurrentChanged(this, EventArgs.Empty);
        }

        #region Add new people / relationships

        /// <summary>
        /// Adds Parent-Child relationship between person and child with the provided parent-child relationship type.
        /// </summary>
        public void AddChild(Shape person, Shape child, ParentChildModifier parentChildType)
        {
            //add child relationship to person
            person.Relationships.Add(new ChildRelationship(child, parentChildType));

            //add person as parent of child
            child.Relationships.Add(new ParentRelationship(person, parentChildType));

            //add the child to the main people list
            if (!this.Contains(child))
            {
                this.Add(child);
            }
        }

        /// <summary>
        /// Add Spouse relationship between the person and the spouse with the provided spouse relationship type.
        /// </summary>
        public void AddSpouse(Shape person, Shape spouse, SpouseModifier spouseType)
        {
            //assign spouses to each other    
            person.Relationships.Add(new SpouseRelationship(spouse, spouseType));
            spouse.Relationships.Add(new SpouseRelationship(person, spouseType));

            //add the spouse to the main people list
            if (!this.Contains(spouse))
            {
                this.Add(spouse);
            }
        }

        /// <summary>
        /// Adds sibling relation between the person and the sibling
        /// </summary>
        public void AddSibling(Shape person, Shape sibling)
        {
            //assign sibling to each other    
            person.Relationships.Add(new SiblingRelationship(sibling));
            sibling.Relationships.Add(new SiblingRelationship(person));

            //add the sibling to the main people list
            if (!this.Contains(sibling))
            {
                this.Add(sibling);
            }
        }

        #endregion

        public Shape Find(string id)
        {
            foreach (Shape person in this)
            {
                if (person.Id == id)
                    return person;
            }

            return null;
        }

        #region INotifyPropertyChanged Members

        protected override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
