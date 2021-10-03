using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using  Opc.Ua.Security.Certificates;
using System.Text;
using System.Threading.Tasks;
using GQLServer.GQL;
using GQLServer.Models;
using HotChocolate.Subscriptions;
using Opc.Ua;
using Opc.Ua.Client;
using System.Linq;

namespace GQLServer.infra
{
    public class UAClient
    {
        #region Private Fields
        private ApplicationConfiguration _configuration;
        private ObservableCollection<Tag> _tags;
        private List<Subscription> _subscriptions;
        private Session _session;
        private readonly IOutput _output;
        private readonly XmlConfig _xmlConfig;
        private readonly ITopicEventSender _eventSender;
        private int TimeOut = 30000;
        #endregion
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the UAClient class.
        /// </summary>
        public UAClient(IOutput output, XMLParser xmlParser, ITopicEventSender eventSender)
        {

            _eventSender = eventSender;
            _xmlConfig = xmlParser.OpcUaXmlFileRead("XmlConfig.xml");
            _output = output;
            _configuration = CreateClientConfiguration();
            _tags= new ObservableCollection<Tag>();           
            ConnectAsync().Wait();
            
        }        
        #endregion
        #region Public Properties
        /// <summary>
        /// Gets the client session.
        /// </summary>
        public Session Session => _session;
        //Gets the clients subscriptions
        public List<Subscription> Subscriptions => _subscriptions;
        //Gets the clients active tags 
        public ObservableCollection<Tag> Tags => _tags;
        #endregion
        #region Public Methods
        /// <summary>
        /// Creates a session with the UA server
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_session != null && _session.Connected == true)
                {
                    _output.WriteLine("Session already connected!");
                }
                else
                {
                    _output.WriteLine("Connecting...");

                    // Get the endpoint by connecting to server's discovery endpoint.
                    // Try to find the first endopint without security.
                    EndpointDescription endpointDescription = CoreClientUtils.SelectEndpoint(_xmlConfig.XmlOpcUaServer.XmlOpcUrl, false);
                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(_configuration);
                    ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                    // Create the session
                    Session session = await Session.Create(
                        _configuration,
                        endpoint,
                        false,
                        false,
                        _configuration.ApplicationName,
                        30 * 60 * 1000,
                        new UserIdentity(),
                        null
                    );

                    if (session != null && session.Connected)
                    {
                        _session = session;
                    }

                    // Session created successfully.
                    _output.WriteLine($"New Session Created with SessionName = {_session.SessionName}");
                }                
            }
            catch (Exception ex)
            {
                // Log Error
                _output.WriteLine($"Create Session Error : {ex.Message}");
                return false;
            }
            try
            {
                CreateSubscriptionsFromXml(_xmlConfig.XmlOpcUaServer.XmlOpcUaSubscriptions);
                _output.WriteLine(" Subscriptions created and added to session");
                return true;
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Create Subscription error: {ex.Message}");
                return false;
            }
            
        }

        /// <summary>
        /// Disconnects the session.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                if (_session != null)
                {
                    _output.WriteLine("Disconnecting...");

                    _session.Close();
                    _session.Dispose();
                    _session = null;

                    // Log Session Disconnected event
                    _output.WriteLine("Session Disconnected.");
                }
                else
                {
                    _output.WriteLine("Session not created!");
                }
            }
            catch (Exception ex)
            {
                // Log Error
                _output.WriteLine($"Disconnect Error : {ex.Message}");
            }
        }
        #endregion
        #region Private Methods
        private void CreateSubscriptionsFromXml(List<XmlOpcUaSubscription> xmlSubscriptions)
        {
            _subscriptions = new List<Subscription>();

            foreach (XmlOpcUaSubscription xmlSub in xmlSubscriptions)
            {
                try
                {
                    int pubInt = Convert.ToInt32(xmlSub.XmlPublishingInterval);

                    // A list of monitored items is created, which are later recorded by the client.
                    var monitoredItems = new List<MonitoredItem>();

                    // For this purpose, the list of Opc variables from the XML configuration is transferred.
                    foreach (XmlOpcUaVariable opcvariable in xmlSub.XmlOpcUaVariables)
                    {
                        // A monitored item is created for each C_OpcUaVariable object.
                        MonitoredItem monitoredItem = CreateMonitoredItemFromXml(opcvariable);

                        // The created monitored item is added to the list.
                        monitoredItems.Add(monitoredItem);
                    }

                    // Subscription is created.
                    Subscription sub = CreateSubscription(pubInt);

                    // MonitoredItems are added.
                    if (monitoredItems != null)
                    {
                        sub.AddItems(monitoredItems);
                    }

                    // Subscription is added to the internal list.
                    _subscriptions.Add(sub);
                }
                catch (Exception ex)
                {
                    throw new Exception(" The subscription with the interval " + xmlSub.XmlPublishingInterval + " from the server " + _xmlConfig.XmlOpcUaServer.XmlOpcUrl + " could not be created ", ex);
                }
                foreach (Subscription sub in _subscriptions)
                {
                    try
                    {
                        AddSubscriptionToSession(sub);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(" The subscription with the Interval " + sub.PublishingInterval + " could not to the server " + _xmlConfig.XmlOpcUaServer.XmlOpcUrl + " added ", ex);
                    }
                }
            }
        }

        private MonitoredItem CreateMonitoredItemFromXml(XmlOpcUaVariable opcvariable)
        {
            MonitoredItem monitoredItem = new MonitoredItem
            {
                DisplayName = opcvariable.XmlVarLabel,
                StartNodeId = ConvertS7DataToNodeID(opcvariable.XmlS7db, opcvariable.XmlS7var), // ns = 5; s = Counter1
                AttributeId = Attributes.Value,
                MonitoringMode = MonitoringMode.Reporting,
                SamplingInterval = Convert.ToInt32(opcvariable.XmlSamplingInterval), // Specifies the interval how often the variable is queried and checked for changes Unit: ms
                QueueSize = 1, // intermediate buffer size
                DiscardOldest = true
            };
            // An action on data change is added to each monitored item
            monitoredItem.Notification += WriteDataOnNotificationAsync;
            // monitoredItem.Notification + = WriteConsoleOnNotification;
            // Every monitored item is checked
            if (monitoredItem.Status.Error != null && StatusCode.IsBad(monitoredItem.Status.Error.StatusCode))
            {
                throw ServiceResultException.Create(monitoredItem.Status.Error.StatusCode.Code, " Creation of the monitored item failed ");
            }
            Tag tag = new Tag{ TagName = opcvariable.XmlVarLabel};
            _tags.Add(tag);
            // return
            return monitoredItem;
        }

        private Subscription CreateSubscription(int publishingInterval)
        {
            // Check for a polling interval of 0.
            if (publishingInterval == 0)
            {
                throw new Exception(" Please enter a valid value for the polling interval. ");
            }

            // Make sure that each polling interval is configured only once.
            foreach (Subscription sub in _subscriptions)
            {
                if (sub.PublishingInterval == publishingInterval)
                {
                    throw new Exception(" A subscription with this interval is already available. ");
                }
            }

            // Create the subscription.
            var subscription = new Subscription()
            {
                PublishingEnabled = true,
                PublishingInterval = publishingInterval,
                MinLifetimeInterval = Convert.ToUInt32(TimeOut),
                DisplayName = " Subscription_ " + Convert.ToString(publishingInterval)
            };

            // Returns the subscription created
            return subscription;
        }

        private string ConvertS7DataToNodeID(string s7db, string s7var)
        {
            string m_nodeId = null;
            //Shema für S7-NodeID: ns3;s="S7_DB"."S7_Var"
            StringBuilder e = new StringBuilder();
            e.Append("ns=3;s=\"");
            e.Append(s7db);
            e.Append("\".\"");
            e.Append(s7var);
            e.Append("\"");
            return m_nodeId = e.ToString();
        }

        private void AddSubscriptionToSession(Subscription subscription)
        {
            // Name will be unified. Only changes the name if the publishing interval has been changed
            subscription.DisplayName = " Subscription_ " + Convert.ToString(subscription.PublishingInterval);

            // Add the subscription to the session.
            _session.AddSubscription(subscription);

            // The subscription is created on the session.
            subscription.Create();
        }

        private async void WriteDataOnNotificationAsync(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            var tag = _tags.Where(t => monitoredItem.DisplayName == t.TagName).FirstOrDefault();
            var value = (MonitoredItemNotification)monitoredItem.LastValue;            
            tag.Value = value.Value.WrappedValue.Value.ToString();
            BuiltInType type = value.Value.WrappedValue.TypeInfo.BuiltInType;
            await _eventSender.SendAsync(nameof(GQLSubscription.OnTagUpdated), tag);
        }
        private static ApplicationConfiguration CreateClientConfiguration()
        {
            // The application configuration can be loaded from any file.
            // ApplicationConfiguration.Load() method loads configuration by looking up a file path in the App.config.
            // This approach allows applications to share configuration files and to update them.
            // This example creates a minimum ApplicationConfiguration using its default constructor.
            ApplicationConfiguration configuration = new ApplicationConfiguration();

            // Step 1 - Specify the client identity.
            configuration.ApplicationName = "UA Client";
            configuration.ApplicationType = ApplicationType.Client;
            configuration.ApplicationUri = "urn:MyClient"; //Kepp this syntax
            configuration.ProductUri = "SiemensAG.IndustryOnlineSupport";

            // Step 2 - Specify the client's application instance certificate.
            // Application instance certificates must be placed in a windows certficate store because that is 
            // the best way to protect the private key. Certificates in a store are identified with 4 parameters:
            // StoreLocation, StoreName, SubjectName and Thumbprint.
            // When using StoreType = Directory you need to have the opc.ua.certificategenerator.exe installed on your machine

            configuration.SecurityConfiguration = new SecurityConfiguration();
            configuration.SecurityConfiguration.ApplicationCertificate = new CertificateIdentifier();
            configuration.SecurityConfiguration.ApplicationCertificate.StoreType = CertificateStoreType.X509Store;
            configuration.SecurityConfiguration.ApplicationCertificate.StorePath = "CurrentUser\\My";
            configuration.SecurityConfiguration.ApplicationCertificate.SubjectName = configuration.ApplicationName;
            configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates = true;
            configuration.SecurityConfiguration.RejectSHA1SignedCertificates = false;

            // Define trusted root store for server certificate checks
            configuration.SecurityConfiguration.TrustedIssuerCertificates.StoreType = CertificateStoreType.X509Store;
            configuration.SecurityConfiguration.TrustedIssuerCertificates.StorePath = "CurrentUser\\Root";
            configuration.SecurityConfiguration.TrustedPeerCertificates.StoreType = CertificateStoreType.X509Store;
            configuration.SecurityConfiguration.TrustedPeerCertificates.StorePath = "CurrentUser\\Root";

            // find the client certificate in the store.
            Task<X509Certificate2> clientCertificate = configuration.SecurityConfiguration.ApplicationCertificate.Find(true);

            // create a new self signed certificate if not found.
            if (clientCertificate.Result == null)
            {
                X509Certificate2 cert = CertificateBuilder.Create("CN=UAclient").CreateForRSA();
            }

            // Step 3 - Specify the supported transport quotas.
            // The transport quotas are used to set limits on the contents of messages and are
            // used to protect against DOS attacks and rogue clients. They should be set to
            // reasonable values.
            configuration.TransportQuotas = new TransportQuotas();
            configuration.TransportQuotas.OperationTimeout = 360000;
            configuration.TransportQuotas.SecurityTokenLifetime = 86400000;
            configuration.TransportQuotas.MaxStringLength = 67108864;
            configuration.TransportQuotas.MaxByteStringLength = 16777216; //Needed, i.e. for large TypeDictionarys

            // Step 4 - Specify the client specific configuration.
            configuration.ClientConfiguration = new ClientConfiguration();
            configuration.ClientConfiguration.DefaultSessionTimeout = 360000;

            // Step 5 - Validate the configuration.
            // This step checks if the configuration is consistent and assigns a few internal variables
            // that are used by the SDK. This is called automatically if the configuration is loaded from
            // a file using the ApplicationConfiguration.Load() method.
            configuration.Validate(ApplicationType.Client);

            return configuration;
        }
        /// <summary>Gets the local IP addresses and the DNS name</summary>
        /// <returns>The list of IPs and names</returns>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        private static List<string> GetLocalIpAddressAndDns()
        {
            List<string> localIps = new List<string>();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIps.Add(ip.ToString());
                }
            }
            if (localIps.Count == 0)
            {
                throw new Exception("Local IP Address Not Found!");
            }
            localIps.Add(Dns.GetHostName());
            return localIps;
        }
        #endregion
    }
    
}