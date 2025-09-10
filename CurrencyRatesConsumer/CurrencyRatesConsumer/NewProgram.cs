using System;
using System.Threading;
using LSEG.Ema.Access;
using LSEG.Ema.Domain.Login;
using static LSEG.Ema.Access.DataType;

namespace CurrencyRatesConsumer
{
    /// <summary>
    /// Simple EMA Consumer for receiving spot currency rates using LSEG Real-Time SDK
    /// This example demonstrates basic subscription to FX rates with real-time updates
    /// </summary>
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting Currency Rates Consumer...");

            OmmConsumer? consumer = null;

            try
            {
                // Create the AppClient to handle incoming messages
                var appClient = new AppClient();

                Console.WriteLine("Connecting to market data server...");

                // Create OmmConsumer configuration
                // For RTDS (on-premise): Use .Host("your-ads-server:14002")
                // For RTO (cloud): Use .ClientId("client-id").ClientSecret("client-secret")
                var config = new OmmConsumerConfig()
                    .Host("localhost:14002");    // Change to your ADS server

                // Create the EMA Consumer
                consumer = new OmmConsumer(config);

                // Register for login (required)
                var loginReq = new LoginReq();
                consumer.RegisterClient(loginReq.Message(), appClient);

                // List of currency pairs to subscribe to (hardcoded as requested)
                // These are common Reuters Instrument Codes (RICs) for major FX pairs
                string[] currencyRics =
                {
                    "EUR=",     // EUR/USD
                    "GBP=",     // GBP/USD  
                    "JPY=",     // USD/JPY
                    "CHF=",     // USD/CHF
                    "CAD=",     // USD/CAD
                    "AUD=",     // AUD/USD
                    "NZD="      // NZD/USD
                };

                Console.WriteLine("Subscribing to currency rates:");

                // Create a view to request specific fields only
                // Starting with minimal fields: BID and ASK only
                var fieldArray = new OmmArray();
                fieldArray.FixedWidth = 2;  // 2 bytes per field ID

                // Add only the specific fields you want to receive
                fieldArray.AddInt(22);    // BID only
                fieldArray.AddInt(25);    // ASK only
                fieldArray.Complete();    // Important: Complete the array

                // Create element list for the view request
                var viewElementList = new ElementList();
                viewElementList.AddUInt(LSEG.Ema.Rdm.EmaRdm.ENAME_VIEW_TYPE, 1);  // 1 = Field ID List
                viewElementList.AddArray(LSEG.Ema.Rdm.EmaRdm.ENAME_VIEW_DATA, fieldArray);
                viewElementList.Complete();  // Important: Complete the element list

                // Subscribe to each currency pair with field view
                foreach (string ric in currencyRics)
                {
                    Console.WriteLine($"  - {ric} (requesting BID and ASK only)");

                    var requestMsg = new RequestMsg()
                        .ServiceName("ELEKTRON_DD")  // Change to your service name
                        .Name(ric)
                        .Payload(viewElementList);   // Add the view to request specific fields

                    consumer.RegisterClient(requestMsg, appClient);
                }

                Console.WriteLine("View request created with BID (22) and ASK (25) fields only");
                Console.WriteLine("If you still receive all fields, the server may not support View requests");

                Console.WriteLine("\nListening for rate updates... Press Ctrl+C to exit");

                // Keep the application running to receive updates
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (OmmException ex)
            {
                Console.WriteLine($"EMA Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                // Clean up resources
                consumer?.Uninitialize();
            }
        }
    }

    /// <summary>
    /// Application client that handles incoming messages from the EMA Consumer
    /// </summary>
    public class AppClient : IOmmConsumerClient
    {
        private readonly LoginRefresh _loginRefresh = new();

        public void OnRefreshMsg(RefreshMsg refreshMsg, IOmmConsumerEvent consumerEvent)
        {
            Console.WriteLine($"\n=== REFRESH MESSAGE ===");

            // Handle Login domain messages
            if (refreshMsg.DomainType() == LSEG.Ema.Rdm.EmaRdm.MMT_LOGIN)
            {
                _loginRefresh.Clear();
                Console.WriteLine("Login Response:");
                Console.WriteLine(_loginRefresh.Message(refreshMsg).ToString());
                return;
            }

            // Handle Market Price domain messages
            Console.WriteLine($"Item: {(refreshMsg.HasName ? refreshMsg.Name() : "<not set>")}");
            Console.WriteLine($"Service: {(refreshMsg.HasServiceName ? refreshMsg.ServiceName() : "<not set>")}");
            Console.WriteLine($"State: {refreshMsg.State()}");

            // Process payload if present
            if (refreshMsg.Payload().DataType != DataTypes.NO_DATA)
            {
                ProcessPayload(refreshMsg.Payload(), refreshMsg.Name());
            }
        }

        public void OnUpdateMsg(UpdateMsg updateMsg, IOmmConsumerEvent consumerEvent)
        {
            Console.WriteLine($"\n=== UPDATE MESSAGE ===");

            string itemName = updateMsg.HasName ? updateMsg.Name() : consumerEvent.Handle.ToString();
            Console.WriteLine($"Item: {itemName}");

            // Process payload if present
            if (updateMsg.Payload().DataType != DataTypes.NO_DATA)
            {
                ProcessPayload(updateMsg.Payload(), itemName);
            }
        }

        public void OnStatusMsg(StatusMsg statusMsg, IOmmConsumerEvent consumerEvent)
        {
            Console.WriteLine($"\n=== STATUS MESSAGE ===");

            string itemName = statusMsg.HasName ? statusMsg.Name() : consumerEvent.Handle.ToString();
            Console.WriteLine($"Item: {itemName}");
            Console.WriteLine($"Service: {(statusMsg.HasServiceName ? statusMsg.ServiceName() : "<not set>")}");

            if (statusMsg.HasState)
            {
                Console.WriteLine($"State: {statusMsg.State()}");
            }
        }

        public void OnGenericMsg(GenericMsg genericMsg, IOmmConsumerEvent consumerEvent) =>
            Console.WriteLine("Received Generic Message");

        public void OnAckMsg(AckMsg ackMsg, IOmmConsumerEvent consumerEvent) =>
            Console.WriteLine("Received Ack Message");

        public void OnAllMsg(Msg msg, IOmmConsumerEvent consumerEvent) =>
            Console.WriteLine($"Received message type: {msg.DataType}");

        /// <summary>
        /// Process the payload containing field data (rates, bid/ask prices, etc.)
        /// </summary>
        /// <param name="payload">The message payload</param>
        /// <param name="itemName">Name of the instrument</param>
        private void ProcessPayload(ComplexTypeData payload, string itemName)
        {
            Console.WriteLine($"Processing data for: {itemName}");

            if (payload.DataType == DataTypes.FIELD_LIST)
            {
                var fieldList = payload.FieldList();

                // Define variables for all the fields we're requesting
                double? bid = null, ask = null, last = null, netChange = null;
                double? bidHigh = null, bidLow = null, dailyHigh = null, dailyLow = null;
                double? openPrice = null, prevClose = null;
                DateTime? quoteTime = null, lastActivityTime = null, activeDate = null;
                string displayName = "";

                foreach (FieldEntry fieldEntry in fieldList)
                {
                    switch (fieldEntry.FieldId)
                    {
                        case 22:    // BID
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                bid = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 25:    // ASK
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                ask = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 30:    // BID_HIGH_1 (Bid High)
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                bidHigh = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 31:    // BID_LOW_1 (Bid Low)
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                bidLow = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 6:     // TRDPRC_1 (Last trade price)
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                last = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 11:    // NETCHNG_1 (Net change)
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                netChange = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 12:    // HIGH_1 (Daily High)
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                dailyHigh = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 13:    // LOW_1 (Daily Low)
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                dailyLow = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 19:    // OPEN_PRC (Opening Price)
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                openPrice = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 21:    // HST_CLOSE (Previous Close)
                            if (fieldEntry.LoadType == DataTypes.REAL)
                                prevClose = fieldEntry.OmmRealValue().AsDouble();
                            break;

                        case 3:     // DSPLY_NAME (Display name)
                            if (fieldEntry.LoadType == DataTypes.RMTES)
                                displayName = fieldEntry.OmmRmtesValue().ToString();
                            break;

                        case 17:    // ACTIV_DATE (Active Date)
                            if (fieldEntry.LoadType == DataTypes.DATE)
                            {
                                var date = fieldEntry.OmmDateValue();
                                activeDate = new DateTime(date.Year, date.Month, date.Day);
                            }
                            break;

                        case 5:     // TIMACT (Time of Last Activity)
                            if (fieldEntry.LoadType == DataTypes.TIME)
                            {
                                var time = fieldEntry.OmmTimeValue();
                                lastActivityTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                    time.Hour, time.Minute, time.Second);
                            }
                            break;

                        case 3855:  // QUOTIM (Quote time)
                            if (fieldEntry.LoadType == DataTypes.TIME)
                            {
                                var time = fieldEntry.OmmTimeValue();
                                quoteTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                    time.Hour, time.Minute, time.Second);
                            }
                            break;

                        default:
                            // Log any other fields received
                            Console.WriteLine($"  Other Field {fieldEntry.FieldId}: {fieldEntry.Load}");
                            break;
                    }
                }

                // Display all the requested field information
                Console.WriteLine($"--- {itemName} {(string.IsNullOrEmpty(displayName) ? "" : $"({displayName})")} ---");

                // Current rates
                if (bid.HasValue) Console.WriteLine($"  Bid: {bid.Value:F5}");
                if (ask.HasValue) Console.WriteLine($"  Ask: {ask.Value:F5}");
                if (last.HasValue) Console.WriteLine($"  Last: {last.Value:F5}");

                // Daily statistics
                if (dailyHigh.HasValue) Console.WriteLine($"  Daily High: {dailyHigh.Value:F5}");
                if (dailyLow.HasValue) Console.WriteLine($"  Daily Low: {dailyLow.Value:F5}");
                if (openPrice.HasValue) Console.WriteLine($"  Open: {openPrice.Value:F5}");
                if (prevClose.HasValue) Console.WriteLine($"  Prev Close: {prevClose.Value:F5}");

                // Bid statistics
                if (bidHigh.HasValue) Console.WriteLine($"  Bid High: {bidHigh.Value:F5}");
                if (bidLow.HasValue) Console.WriteLine($"  Bid Low: {bidLow.Value:F5}");

                // Changes
                if (netChange.HasValue) Console.WriteLine($"  Net Change: {netChange.Value:F5}");

                // Calculated values
                if (bid.HasValue && ask.HasValue)
                {
                    var mid = (bid.Value + ask.Value) / 2;
                    var spread = ask.Value - bid.Value;
                    Console.WriteLine($"  Mid: {mid:F5}");
                    Console.WriteLine($"  Spread: {spread:F5}");
                }

                // Timestamps
                if (activeDate.HasValue) Console.WriteLine($"  Active Date: {activeDate.Value:yyyy-MM-dd}");
                if (lastActivityTime.HasValue) Console.WriteLine($"  Last Activity: {lastActivityTime.Value:HH:mm:ss}");
                if (quoteTime.HasValue) Console.WriteLine($"  Quote Time: {quoteTime.Value:HH:mm:ss}");

                Console.WriteLine($"  Updated: {DateTime.Now:HH:mm:ss.fff}");
            }
            else
            {
                Console.WriteLine($"Payload type: {payload.DataType}");
            }
        }
    }
}

/*
=== SETUP INSTRUCTIONS ===

1. Install Required NuGet Package:
   Install-Package LSEG.Ema.Core

2. Create EmaConfig.xml file in your output directory:

<?xml version="1.0" encoding="UTF-8"?>
<EmaConfig>
    <ConsumerGroup>
        <DefaultConsumer value="Consumer_1"/>
        <ConsumerList>
            <Consumer>
                <Name value="Consumer_1"/>
                <ChannelSet value="Channel_1"/>
                <Dictionary value="Dictionary_1"/>
                <Logger value="Logger_1"/>
                <XmlTraceToStdout value="0"/>
            </Consumer>
        </ConsumerList>
    </ConsumerGroup>
    
    <ChannelGroup>
        <ChannelList>
            <Channel>
                <Name value="Channel_1"/>
                <ChannelType value="ChannelType::RSSL_SOCKET"/>
                <CompressionType value="CompressionType::None"/>
                <GuaranteedOutputBuffers value="5000"/>
                <Host value="localhost"/>
                <Port value="14002"/>
            </Channel>
        </ChannelList>
    </ChannelGroup>
    
    <DictionaryGroup>
        <DictionaryList>
            <Dictionary>
                <Name value="Dictionary_1"/>
                <DictionaryType value="DictionaryType::FileDictionary"/>
                <RdmFieldDictionaryFileName value="./RDMFieldDictionary"/>
                <EnumTypeDefFileName value="./enumtype.def"/>
            </Dictionary>
        </DictionaryList>
    </DictionaryGroup>
    
    <LoggerGroup>
        <LoggerList>
            <Logger>
                <Name value="Logger_1"/>
                <LoggerType value="LoggerType::Stdout"/>
                <LoggerSeverity value="LoggerSeverity::Success"/>
            </Logger>
        </LoggerList>
    </LoggerGroup>
</EmaConfig>

Alternative: For RTO (Cloud) Connection, use this EmaConfig.xml:

<?xml version="1.0" encoding="UTF-8"?>
<EmaConfig>
    <ConsumerGroup>
        <DefaultConsumer value="Consumer_4"/>
        <ConsumerList>
            <Consumer>
                <Name value="Consumer_4"/>
                <ChannelSet value="Channel_4"/>
                <Logger value="Logger_1"/>
                <Dictionary value="Dictionary_1"/>
                <XmlTraceToStdout value="0"/>
            </Consumer>
        </ConsumerList>
    </ConsumerGroup>
    
    <ChannelGroup>
        <ChannelList>
            <Channel>
                <Name value="Channel_4"/>
                <ChannelType value="ChannelType::RSSL_ENCRYPTED"/>
                <CompressionType value="CompressionType::None"/>
                <GuaranteedOutputBuffers value="5000"/>
                <!-- EMA discovers host and port from RTO service discovery -->
                <!-- when both are not set and session management is enabled -->
                <Location value="us-east-1"/>
                <EnableSessionManagement value="1"/>
                <EncryptedProtocolType value="EncryptedProtocolType::RSSL_SOCKET"/>
            </Channel>
        </ChannelList>
    </ChannelGroup>
    
    <DictionaryGroup>
        <DictionaryList>
            <Dictionary>
                <Name value="Dictionary_1"/>
                <DictionaryType value="DictionaryType::ChannelDictionary"/>
            </Dictionary>
        </DictionaryList>
    </DictionaryGroup>
    
    <LoggerGroup>
        <LoggerList>
            <Logger>
                <Name value="Logger_1"/>
                <LoggerType value="LoggerType::Stdout"/>
                <LoggerSeverity value="LoggerSeverity::Success"/>
            </Logger>
        </LoggerList>
    </LoggerGroup>
</EmaConfig>

3. For Real-Time Optimized (Cloud) Connection:
   Replace the OmmConsumerConfig with:
   
   var config = new OmmConsumerConfig()
       .ClientId("your-client-id")
       .ClientSecret("your-client-secret");

4. Connection Details to Update:
   - Host: Update "localhost:14002" to match your ADS server
   - ServiceName: Verify "ELEKTRON_DD" or use your service name
   - RIC Codes: The example uses standard FX RIC codes

5. Required Files (for RTDS):
   - RDMFieldDictionary  
   - enumtype.def
   These files define field IDs and should be provided by your market data team.

6. Common Field IDs (Reuters Standard):
   - 22: BID
   - 25: ASK  
   - 6: TRDPRC_1 (Last Price)
   - 3: DSPLY_NAME (Display Name)
   - 11: NETCHNG_1 (Net Change)
   - 3855: QUOTIM (Quote Time)

=== KEY API CORRECTIONS MADE ===

1. Removed Username() method (not available in C# API)
2. Used ComplexTypeData instead of Payload for ProcessPayload parameter
3. Removed HasPayload checks - check DataType != NO_DATA instead
4. Used consumerEvent.Handle.ToString() instead of consumerEvent.Item.Name()
5. Removed unnecessary Data.DataType calls
6. Used expression body methods for simple callbacks
7. Simplified collection initialization
8. Removed unused parameter warning

=== KEY DIFFERENCES FROM RFA ===

1. Uses LSEG.Ema.Access namespace (not Reuters.Rfa)
2. OmmConsumer instead of Session/EventQueue pattern
3. Callback-based architecture (IOmmConsumerClient interface)
4. LoginReq.Message() for login requests
5. RequestMsg for item subscriptions
6. FieldEntry.OmmRealValue().AsDouble() for accessing numeric values
7. ComplexTypeData instead of Payload type

=== MIGRATION TIPS ===

- RFA RespType.REFRESH → OnRefreshMsg callback
- RFA RespType.UPDATE → OnUpdateMsg callback  
- RFA RespType.STATUS → OnStatusMsg callback
- RFA MMT_LOGIN domain handling is similar
- Field access patterns are comparable but use Omm* value accessors
- Configuration is XML-based (EmaConfig.xml) instead of programmatic
*/