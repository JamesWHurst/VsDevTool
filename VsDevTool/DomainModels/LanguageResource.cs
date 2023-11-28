using System.Text;


namespace VsDevTool.DomainModels
{
    /// <summary>
    /// Instances of this class encompass one translatable resource item, 
    /// containing a key, value in English, value in some other languages, and descriptions.
    /// </summary>
    public class LanguageResource
    {
        public LanguageResource()
        {
        }

        public LanguageResource( string valueEnglish, string valueOtherLanguage )
        {
            this.EnglishValue = valueEnglish;
            this.OtherLanguageValue = valueOtherLanguage;
        }

        public string Key { get; set; }

        public string EnglishValue { get; set; }

        public string OtherLanguageValue { get; set; }

        public string DescriptionInEnglish { get; set; }

        public string DescriptionInOtherLanguage { get; set; }

        public bool IsToBeTranslated { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder( "LanguageResource(" );
            if (Key != null)
            {
                sb.Append( "Key=" ).Append( this.Key ).Append( "," );
            }
            if (EnglishValue != null)
            {
                sb.Append( "EnglishValue=" ).Append( this.EnglishValue ).Append( "," );
            }
            if (OtherLanguageValue != null)
            {
                sb.Append( "OtherLanguageValue=" ).Append( this.OtherLanguageValue ).Append( "," );
            }
            if (DescriptionInEnglish != null)
            {
                sb.Append( "DescriptionInEnglish=" ).Append( this.DescriptionInEnglish ).Append( "," );
            }
            if (DescriptionInOtherLanguage != null)
            {
                sb.Append( "DescriptionInOtherLanguage=" ).Append( this.DescriptionInOtherLanguage ).Append( "," );
            }
            sb.Append( "IsToBeTranslated=" ).Append( this.IsToBeTranslated );
            sb.Append( ")" );
            return sb.ToString();
        }
    }
}
