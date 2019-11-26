-- FUNCTION: dbo."SendCommandToUDP"()
-- DROP FUNCTION dbo."SendCommandToUDP"();

CREATE FUNCTION dbo."SendCommandToUDP"()
    RETURNS trigger
    LANGUAGE 'plpythonu'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$

import socket
import struct
IPADDR = 'localhost'
PORTNUM = 8410
UserId = TD["new"]["UserId"]
DevId = TD["new"]["IdDev"]
y=struct.pack("<I", UserId)
z=struct.pack("<I", DevId)
PACKETDATA=bytearray(y) + bytearray(z)
s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
s.connect((IPADDR, PORTNUM))
s.send(PACKETDATA)
s.close()

$BODY$;

ALTER FUNCTION dbo."SendCommandToUDP"()
    OWNER TO postgres;


-- Trigger: InsUp_SendUDP
-- DROP TRIGGER "InsUp_SendUDP" ON dbo."NBIoTCommands";

CREATE TRIGGER "InsUp_SendUDP"
    AFTER INSERT OR UPDATE 
    ON dbo."NBIoTCommands"
    FOR EACH ROW
    EXECUTE PROCEDURE dbo."SendCommandToUDP"();