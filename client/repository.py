from connectionException import ConnectionError

class Repository():
	def __init__(self,client):
		self.client = client

	def create_db(self, db_name):
		command = "1^" + db_name
		try:
			code = self.client.send_message(command)
			print(code)
			if code[0] == '1':
				return 1, code.split('^')[1]
			return 0, "OK"
		except ConnectionError as ce:
			return 1, ce

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
		return fk["name"] + "^" + fk["tableName"] + "^" + fk["foreignColumn"] + "^" + fk["column"]



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

	def create_table(self, db_name, table_name):
		command = "3^" + db_name + "^" + table_name
		try:
			# print(command)
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			# else:
			# 	# if code[0] == '1':
			# 	# 	return 1, code.split('^')[1]
			# 	for column in table["columns"]:
			# 		code = self.add_column(column)
			# 		if code[0] == '1':
			# 			return 1, code.split('^')[1]
			# 	# code = self.client.send_message("-5")
			# 	# if code[0] == '1':
			# 	# 	return 1, code.split('^')[1]
			# 	for fk in table["FK"]:
			# 		code = self.add_foreign_key(fk)
			# 		if code[0] == '1':
			# 			return 1, code.split('^')[1]
			# code = self.client.send_message("-3 END")
			# if code[0] == '1':
			# 	return 1, code.split('^')[1]
			return 0, 'OK'
		except ConnectionError as ce:
			return 1, ce

	def drop_database(self, db_name):
		command = "2^" + db_name
		try:
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			return 0, 'OK'
		except ConnectionError as ce:
			return 1, ce

	def drop_table(self, db_name, table_name):
		command = "4^" + db_name + "^" + table_name
		try:
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			return 0, 'OK'
		except ConnectionError as ce:
			return 1, ce


	def get_db_data(self):
		command = "5^GET"
		databases = []
		try:
			code = self.client.send_message_pers(command)
			if code[0] == '1':
				return 1, code.split('^')[0]
			while code[0] != '0':
				db = {}
				data = code.split('^')
				db["name"] = data[0]
				db["tables"] = []
				for table in data[1:]:
					db["tables"].append(table)
				databases.append(db)

				code = self.client.send_message_pers("0^OK")
				if code[0] == '1':
					return 1, code.split('^')[0]
			return 0, databases
		except ConnectionError as ce:
			return 1, ce

	def get_tables(self, db_name):
		command = "9^" + db_name
		try:
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[0]
			tables = code.split('^')
			return 0, tables 
		except ConnectionError as ce:
			return 1, ce

	def get_table_data(self, db_name, table_name):
		command = "6^" + db_name + "^" + table_name
		table_data = []
		try:
			self.client.connect()
			code = self.client.send_message_pers(command)

			if code[0] == '1':
				return 1, code.split('^')[0]
			code = self.client.send_message_pers("0")
			while code[0] != '0':
				print(code)
				column = {}
				data = code.split('^')
				column["Name"] = data[0]
				column["Type"] = data[1]
				column["Primary Key"] = False if data[2] == "--" else True
				# if data[2] == "--":
				# 	column["Primary Key"] = False
				# else:
				# 	column["Primary Key"] = True
				column["Not NULL"] = False if data[3] == "--" else True
				column["Default"] = "-" if data[4] == "--" else data[4]
				column["Identity"] = False if data[5] == "--" else True
				column["Unique"] = False if data[6] == "--" else True
				column["Check"] = "-" if data[7] == "--" else data[7]

				table_data.append(column)
				code = self.client.send_message_pers("0^OK")
				if code[0] == '1':
					return 1, code.split('^')[0]
			return 0, table_data
		except ConnectionError as ce:
			return 1, ce