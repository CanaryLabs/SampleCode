Canary Labs Node-Red Store And Forward Example
- Tested with Node-Red v0.20.3
- Depends upon node-red-contrib-modbus module (v4.1.3 tested)
- Depends upon node-red-node-sqlite (v0.3.6 tested)
- Reads coils, inputs, holding registers, and input registers from a Modbus device and logs to the Canary historian
- Utilizes the Canary Sender Web API for storage
- This flow supports client buffering of data by writing / reading to / from a local sqlite database. 

Configuration Steps
1. Download the CanaryModbusStoreAndForward.json flow from Github and import into Node-Red
2. Download the buffer.sql file for creating the sqlite buffer table
3. Create the sqlite database on disk and use the script to create the buffer table
4. Configure the Store to buffer, Read buffer, Update Sent Status, and Purge Sent Records nodes to reference the created sqlite database
5. Configure the modbus read nodes with the registers you'd like to read (Coils, Inputs, Holding Registers, and Input Registers)
6. Configure the Log Coils, Log Inputs, Log Holding Registers, and Log Input Registers functions to map tag names to register indexes
7. Modify the URLs in the TestTokens, RevokeSessionToken, RevokeUserToken, GetCanaryUserToken, GetCanarySessionToken, and CanaryStoreData nodes to hit the correct endpoint. (Currently set to localhost). Leave the paths alone
8. Configure the historian name in the Format SessionToken JSON node
- This is the name of the historian machine (set to "Josh" by default)

Note: the flow is setup to use anonymous authentication to the Canary Sender API
If you have the anonymous endpoint turned off, you will need to change the following:
- URLs in TestTokens, RevokeSessionToken, RevokeUserToken, GetCanaryUserToken, GetCanarySessionToken, and CanaryStoreData nodes to use https instead of http.
- Set the username and password in the Format UserToken JSON node to a user with access to the sender api. 
		
