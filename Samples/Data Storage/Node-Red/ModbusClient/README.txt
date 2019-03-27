Canary Labs Node-Red storage example
- Tested with Node-Red v0.18.4
- Depends upon node-red-contrib-modbus module (v3.4.0 tested)
- Reads coils, inputs, holding registers, and input registers from a Modbus device and logs to the Canary historian
- Utilizes the Canary Sender Web API for storage
- This flow does not support client buffering of data. If connection is lost with the Canary Sender API, data may be lost

Configuration Steps
1. Download the CanaryModbusStorage.json flow from Github and import into Node-Red
2. Configure the modbus read nodes with the registers you'd like to read (Coils, Inputs, Holding Registers, and Input Registers)
3. Configure the Log Coils, Log Inputs, Log Holding Registers, and Log Input Registers functions to map tag names to register indexes
4. Modify the URLs in the CanaryStoreData, GetCanaryUserToken, and GetCanarySessionToken nodes to hit the correct endpoint. (Currently set to localhost). Leave the paths alone
5. Configure the historian name in the Format SessionToken JSON node
- This is the name of the historian machine (set to "Josh" by default)

Note: the flow is setup to use anonymous authentication to the Canary Sender API
If you have the anonymous endpoint turned off, you will need to change the following:
- URLs in CanaryStoreData, GetCanaryUserToken, and GetCanarySessionToken nodes to use https instead of http.
- Set the username and password in the Format UserToken JSON node to a user with access to the sender api. 
		
