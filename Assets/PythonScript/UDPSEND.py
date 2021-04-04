import socket
import json

UDP_IP = "127.0.0.1"
UDP_PORT = 8080
info = {"vector":[-40,46,15]}

print("UDP target IP: %s" % UDP_IP)
print("UDP target port: %s" % UDP_PORT)


sock = socket.socket(socket.AF_INET, # Internet
                     socket.SOCK_DGRAM) # UDP
                     
MESSAGE = str(info).encode()                  
print("message: %s" % MESSAGE)
sock.sendto(MESSAGE, (UDP_IP, UDP_PORT))

while True:
    print("running")