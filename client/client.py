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
			# self.client_socket.setsockopt(SOL_SOCKET, SO_RCVTIMEO, struct.pack('LL', 15, 0))
			print("Connection successful")
			return 0
		except error:
			print("Connetion failed with error:", error)
			return 1


	def send_message_pers(self, message):
		try:
			# self.connect()
			self.client_socket.send(message.encode())
			# print("SENT MESSAGE: ", message)
			self.response = self.client_socket.recv(2048).decode()
			# self.close_connection()
			return self.response
		except error:
			print("Connection error when sending message to server")
			raise ConnectionError("Connection error when sending message to server " + str(error))

	def send_message(self, message):
		try:
			self.connect()
			self.client_socket.send(message.encode())
			# print("SENT MESSAGE: ", message)
			self.response = self.client_socket.recv(2048).decode()
			self.close_connection()
			return self.response
		except error:
			print("Connection error when sending message to server")
			raise ConnectionError("Connection error when sending message to server " + str(error))

		
		# except timeout:
			# raise ConnectionError("Connection timed out when waiting for response")


	def close_connection(self):
		self.client_socket.close()