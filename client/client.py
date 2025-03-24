from socket import *
from connectionException import ConnectionError

class Client():
	def __init__(self, server_ip="localhost", server_port=5555):
		self.server_ip = server_ip
		self.server_port = int(server_port)

	def setDestination(self, server_ip, server_port):
		self.server_ip = server_ip
		self.server_port = int(server_port)

	def connect(self):
		self.client_socket = socket(AF_INET, SOCK_STREAM)
		try:
			self.client_socket.connect((self.server_ip, self.server_port))
			self.client_socket.settimeout(600)
			print("Connection successful")
			return 0
		except error:
			print("Connetion failed with error:", error)
			return 1

	def send_message(self, message):
		try:
			self.client_socket.send(message.encode())
			self.response = self.client_socket.recv(2048).decode()
			return self.response
		except error:
			print("Connection error when sending message to server")
			raise ConnectionError("Connection error when sending message to server " + error)


	def close_connection(self):
		self.client_socket.close()