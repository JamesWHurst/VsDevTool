using System.Net;
using System.Text;
#if !NETFX_CORE
using System.Net.Mail;
#endif


// The intent of this module is to provide an email facility that is API-neutral
// for .NET Framework as well as for UWP (Universal Windows Platform) code.
//
// This needs to be migrated over to use MimeKit and MailKit.


namespace Hurst.LogNut.OutputPipes
{
    //public class EmailLib
    //{
    //    public EmailClient EmailClientDefault { get; set; }
    //}

    /// <summary>
    /// This is a platform-neutral class implementation for sending email, substituting for SmtpClient (which is only available on .NET Framework).
    /// </summary>
    public class EmailClient
    {
        /// <summary>
        /// Create a new EmailClient instance.
        /// </summary>
        public EmailClient()
        {
#if !NETFX_CORE
            _smtpClient = new SmtpClient();
#endif
        }

        /// <summary>
        /// Get or set the <see cref="NetworkCredential"/> for this email-client to use.
        /// </summary>
        public NetworkCredential Credentials
        {
            get
            {
                if (_networkCredential == null)
                {
                    _networkCredential = new NetworkCredential();
                }
                return _networkCredential;
            }
            set { _networkCredential = value; }
        }

        /// <summary>
        /// Get or set whether to use SSL.
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Get or set the port-number to use for the email server.
        /// </summary>
        public int Port
        {
            get { return _smtpPortNumber; }
            set { _smtpPortNumber = value; }
        }

        /// <summary>
        /// Get or set the address of the SMTP server to use.
        /// </summary>
        public string Host
        {
            get { return _smtpServer; }
            set { _smtpServer = value; }
        }

        /// <summary>
        /// Return this object back to it's initial state.
        /// </summary>
        public void SetToDefaults()
        {

        }

        /// <summary>
        /// Send an email message composed from the given information.
        /// </summary>
        /// <param name="fromAddress">the email-address of the sender</param>
        /// <param name="toAddress">the email-address of the receiver</param>
        /// <param name="subject">the text to use as the 'Subject' of this message</param>
        /// <param name="messageBody">the text to use to comprise the body-content of the email-message</param>
        public void SendEmailMessage( string fromAddress, string toAddress, string subject, string messageBody )
        {
#if !NETFX_CORE
            MailMessage mailMessage = new MailMessage( fromAddress, toAddress, subject, messageBody );
            _smtpClient.Send( mailMessage );
#endif
        }

#if DEBUG
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "EmailClient(Host=" );
            if (_smtpServer != null)
            {
                sb.Append( _smtpServer );
            }
            else
            {
                sb.Append( "null" );
            }
            if (_smtpPortNumber != 25)
            {
                sb.Append( ",Port=" ).Append( _smtpPortNumber );
            }
            sb.Append( ")" );
            return sb.ToString();
        }
#endif

        /// <summary>
        /// The SMTP server address to use when emailing out log notifications.
        /// </summary>
        private string _smtpServer;

        private NetworkCredential _networkCredential;

        /// <summary>
        /// The SMTP server port-number to use when emailing out log notifications. Default is port 25.
        /// </summary>
        private int _smtpPortNumber = 25;

        ///// <summary>
        ///// The user-name to use when accessing the SMTP email server.
        ///// </summary>
        //    private string _smtpUsername;

        ///// <summary>
        ///// The user-password to use when accessing the SMTP email server.
        ///// </summary>
        //   private string _smtpPassword;

#if !NETFX_CORE
        /// <summary>
        /// This is the SmtpClient object, which is only available on the full .NET Framework.
        /// </summary>
        public SmtpClient _smtpClient;
#endif
    }

    //_smtpClient.EnableSsl = true;
    //            _smtpClient.Credentials = new NetworkCredential( _smtpUsername, _smtpPassword );
    //_smtpClient.Port = SmtpPortNumber;

    //internal SmtpClient SmtpClient
    //{
    //    get
    //    {
    //        if (_smtpClient == null)
    //        {
    //            _smtpClient = new SmtpClient( _smtpServer, _smtpPortNumber );
    //        }
    //        _smtpClient.EnableSsl = true;
    //        _smtpClient.Credentials = new NetworkCredential( _smtpUsername, _smtpPassword );
    //        _smtpClient.Port = SmtpPortNumber;
    //        return _smtpClient;
    //    }
    //}

}
