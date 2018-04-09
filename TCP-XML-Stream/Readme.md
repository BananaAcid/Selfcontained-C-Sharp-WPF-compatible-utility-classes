# XML Stream decoder for .NET Core and .NET 4+
__Includes the lib, a GUI to test and a Node-RED server script__

> XMLNetworkStreamClient.cs - the client lib

> XMLNetworkStreamClient-TestGUI.csproj - the client test GUI

> XMLNetworkStreamServer - Tester - Node-RED.txt - contains the configuration 'Node-RED flow'. Copy and paste it into the Node-RED GUI. https://nodered.org/


How XML Streams work:
An XML stream is always open and duplex. XML chunks will be send and received by both sides. 
Server answers are usually based on ID keys, in par with the previously send client XML message with that same specific ID.


This client is event based for any status change. Initialize the class and register the event you want to listen to.



Methods and properties:

        public EventHandler<XElement> OnDataReceived;
        public EventHandler<Boolean> OnStatusChange; // bool = isAlive


        public bool isConnected { get; }
        public bool isAlive { get; }

        public async Task sendData(XElement data)
        public async Task sendData(string data)


MIT license used.