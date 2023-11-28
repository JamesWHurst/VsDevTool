using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hurst.LogNut.Util;
using static Hurst.LogNut.Util.StringLib;


namespace VsDevTool.DomainModels
{
    /// <summary>
    /// This class contains the information that can be acquired from a single reference
    /// to a NuGet package within a .csproj file.
    /// </summary>
    public class NugetPackageReference
    {
        public NugetPackageReference(  )
        {
        }

        public NugetPackageReference( string name, string version )
        {
            this.Name = name;
            this.VersionDeclared = version;
        }

        public string Name { get; set; }

        public string HintPath { get; set; }

        public string VersionDeclared { get; set; }

        public string VersionInPackagePath { get; set; }

        #region ToString
        /// <summary>
        /// Override the ToString method to yield a useful indication of this object's state.
        /// </summary>
        /// <returns>a string denoting some of this object's properties</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("NugetPackageReference(");
            sb.Append("Name = ").Append(StringLib.AsQuotedString(Name));
            sb.Append($", HintPath = {StringLib.AsQuotedString(this.HintPath)}");
            if (this.VersionDeclared != null)
            {
                sb.Append($", VersionDeclared = {AsQuoted(this.VersionDeclared)}");
            }
            if (this.VersionInPackagePath != null)
            {
                sb.Append($", VersionInPackagePath = {AsQuoted(this.VersionInPackagePath)}");
            }
            sb.Append(")");
            return sb.ToString();
        }
        #endregion

    }
}
