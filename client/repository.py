from connectionException import ConnectionError

class Repository():
	def __init__(self,client):
		self.client = client

	def create_db(self, db_name):
		command = "1^" + db_name
		code = self.client.send_message(command)
		if "-1" in code:
			return -1, "There was an error with the connection"
		elif code[0] == '1':
			return 1, code.split()[1]
		elif code[0] == '0':
			return 0, "OK"

	def build_column_command(self, column):
		command = column["Name"] + "^" + column["Type"] + "^"
		if column["Primary Key"]:
			command += "1^"
		else:
			command += "--^"
		if column["Not NULL"]:
			command += "1^"
		else:
			command += "--^"
		if column["Default"]:
			command += column["Default"] + "^"
		else:
			command += "--^"
		if column["Identity"]:
			# command += column["identity"] + "^"
			command += "1^"
		else:
			command += "--^"
		if column["Unique"]:
			command += "1"
		else:
			command += "--^"
		if column["Check"]:
			command += column["Check"]
		else:
			command += "--"

		return command

	def build_fk_command(self, fk):
		command = fk["name"] + "^" + fk["tableName"] + "^" + fk["foreignColumn"] + "^" + fk["column"]


	def add_column(self, db_name, table_name, column):
		command = "7^" + db_name + "^" + table_name + "^" + self.build_column_command(column)
		return self.client.send_message(command)

	def add_columns(self, db_name, table_name, columns):
		for column in columns:
			code = self.add_column(db_name, table_name, column)
			if code[0] == '1':
				return code
		return "0 OK"

	def add_foreign_key(self, db_name, table_name, fk):
		command = "8^" + db_name + "^" + table_name + "^" + self.build_fk_command(fk)
		return self.client.send_message(command)

	def create_table(self, db_name, table):
		command = "3^" + db_name + "^" + table["name"]
		try:
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split()[1]
			# else:
			# 	# if code[0] == '1':
			# 	# 	return 1, code.split()[1]
			# 	for column in table["columns"]:
			# 		code = self.add_column(column)
			# 		if code[0] == '1':
			# 			return 1, code.split()[1]
			# 	# code = self.client.send_message("-5")
			# 	# if code[0] == '1':
			# 	# 	return 1, code.split()[1]
			# 	for fk in table["FK"]:
			# 		code = self.add_foreign_key(fk)
			# 		if code[0] == '1':
			# 			return 1, code.split()[1]
			# code = self.client.send_message("-3 END")
			# if code[0] == '1':
			# 	return 1, code.split()[1]
			return 0, 'OK'
		except ConnectionError as ce:
			return 1, ce

	def drop_database(self, db_name):
		command = "2^" + db_name
		try:
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split()[1]
			return 0, 'OK'
		except ConnectionError as ce:
			return 1, ce

	def drop_table(self, db_name, table_name):
		command = "4^" + db_name + "^" + table_name
		try:
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split()[1]
			return 0, 'OK'
		except ConnectionError as ce:
			return 1, ce