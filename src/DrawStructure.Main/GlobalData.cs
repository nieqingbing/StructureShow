
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DrawStructure.Main
{
   public class GlobalData
    {
        internal const string ApplicationFolderName = "Structure";
        public const string AppDataFolderName = "CurrentStructure";
        public const string CommentsFolderName = "Comments";
        public const string DefaultName = "Unknown";


        /// <summary>
        /// Return the animation duration. The duration is extended
        /// if special keys are currently pressed (for demo purposes)  
        /// otherwise the specified duration is returned. 
        /// </summary>
        public static TimeSpan GetAnimationDuration(double milliseconds)
        {
            return TimeSpan.FromMilliseconds(
                Keyboard.IsKeyDown(Key.F12) ?
                milliseconds * 5 : milliseconds);
        }

        // The main list of family members that is shared for the entire application.
        // The FamilyCollection and Family fields are accessed from the same thread,
        // so suppressing the CA2211 code analysis warning.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static UML FamilyCollection = new UML();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
        public static UMLCollection Family = FamilyCollection.UMLCollection;
    }
}
