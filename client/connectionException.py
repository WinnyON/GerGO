class ConnectionError(Exception):
	def __init__(self, message):
		self.msg = message
		super().__init__(message)
	def get_text(self):
		return self.msg