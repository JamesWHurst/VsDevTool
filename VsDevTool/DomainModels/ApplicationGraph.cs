using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;


namespace VsDevTool.DomainModels
{
    /// <summary>
    /// This enum-type denotes the 'scope' that is being considered at this moment -
    /// which may be ApplicationScope, SolutionScope, or ProjectScope.
    /// </summary>
    public enum ApplicationAnalysisScope
    {
        /// <summary>
        /// The analysis encompasses a set of more than one solutions.
        /// </summary>
        ApplicationScope,

        /// <summary>
        /// The analysis applies to a solution, which in turn is comprised of some number of projects.
        /// </summary>
        SolutionScope,

        /// <summary>
        /// The analysis applies to one specific project.
        /// </summary>
        ProjectScope
    }

    /// <summary>
    /// This enum-type denotes the version of the .NET Framework to target. The default is 4.51
    /// </summary>
    public enum NetFrameworkVersion
    {
        /// <summary>
        /// .NET Framework version 4.5.1
        /// </summary>
        Version4p51,

        /// <summary>
        /// .NET Framework version 3.5
        /// </summary>
        Version3p5,

        /// <summary>
        /// .NET Framework version 4.0
        /// </summary>
        Version4,

        /// <summary>
        /// .NET Framework version 4.6.1
        /// </summary>
        Version4p61
    }

    /// <summary>
    /// This is the root of the tree of objects that represent a software application,
    /// in the context of this particular tool.
    /// </summary>
    [Serializable]
    [XmlRoot("ApplicationGraph")]
    public class ApplicationGraph
    {
        /// <summary>
        /// The name that the architect assigns to this application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The list of Visual Studio solutions that comprise this application.
        /// </summary>
        [XmlArrayItem( "VsSolution", typeof( VsSolution ) )]
        [XmlArray( "VsSolutions" )]
        public List<VsSolution> VsSolutions { get; set; }

        #region SerializeToXML
        /// <summary>
        /// You must override this in your subclass to accomplish the serialization of your class to the XML file.
        /// </summary>
        public void SerializeToXML(string pathname)
        {
            XmlSerializer serializer = new XmlSerializer( typeof(ApplicationGraph) );
            using (var textWriter = new StreamWriter( pathname ))
            {
                serializer.Serialize( textWriter, this );
            }
        }
        #endregion

    }
}
