using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawStructure.Main
{
    /// <summary>
    /// Representation for a Parent couple.  E.g. Bob and Sue
    /// </summary>
    public class ParentSet : IEquatable<ParentSet>
    {
        private Shape firstParent;

        private Shape secondParent;

        public Shape FirstParent
        {
            get { return firstParent; }
            set { firstParent = value; }
        }

        public Shape SecondParent
        {
            get { return secondParent; }
            set { secondParent = value; }
        }

        public ParentSet(Shape firstParent, Shape secondParent)
        {
            this.firstParent = firstParent;
            this.secondParent = secondParent;
        }

        public string Name
        {
            get
            {
                string name = "";
                name += firstParent.Name + " + " + secondParent.Name;
                return name;
            }
        }

        // Parameterless contstructor required for serialization
        public ParentSet() { }

        #region IEquatable<ParentSet> Members

        /// <summary>
        /// Determine equality between two ParentSet classes.  Note: Bob and Sue == Sue and Bob
        /// </summary>
        public bool Equals(ParentSet other)
        {
            if (other != null)
            {
                if (this.firstParent.Equals(other.firstParent) && this.secondParent.Equals(other.secondParent))
                    return true;

                if (this.firstParent.Equals(other.secondParent) && this.secondParent.Equals(other.firstParent))
                    return true;
            }

            return false;
        }

        #endregion
    }
}
