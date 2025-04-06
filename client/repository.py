from connectionException import ConnectionError

class Repository():
	def __init__(self,client):
		self.client = client

	def create_db(self, db_name):
		command = "1^" + db_name
		try:
			self.client.connect()
			code = self.client.send_message(command)
			print(code)
			if code[0] == '1':
				return 1, code.split('^')[1]
			return 0, "OK"
		except ConnectionError as ce:
			return 1, ce

	def build_column_command(self, column):
		# column["Name"] = column["Name"].replace('^', '')
		# column["Type"] = column["Type"].replace('^', '')
		# column["Default"] = column["Default"].replace('^', '')
		# column["Identity"] = column["Identity"].replace('^', '')
		# column["Check"] = column["Check"].replace('^', '')

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
			# command += column["Identity"] + "^"
			# command += "1^"
			identity_data = column["Identity"].split(",")
			if len(identity_data) == 2:
				command += identity_data[0] + "^" + identity_data[1] + "^"
			else:
				return 1, "Invalid Identity Format"
		else:
			command += "--^--^"
		if column["Unique"]:
			command += "1^"
		else:
			command += "--^"
		if column["Check"]:
			command += column["Check"]
		else:
			command += "--"

		return 0, command

	def build_fk_command(self, fk):
		return fk["name"] + "^" + fk["tableName"] + "^" + fk["foreignColumn"] + "^" + fk["column"]



	def add_column(self, db_name, table_name, column):
		code, columns_string = self.build_column_command(column)
		if code == 1:
			return "1^" + columns_string
		command = "7^" + db_name + "^" + table_name + "^" + columns_string
		print(command)
		return self.client.send_message(command)

	def add_columns(self, db_name, table_name, columns):
		try:
			self.client.connect()
			print(columns)
			for column in columns:
				code = self.add_column(db_name, table_name, column)
				if code[0] == '1':
					return 1, code.split('^')[1]
			return 0, "OK"
		except ConnectionError as ce:
			return 1, ce

	def add_foreign_key(self, db_name, table_name, fk):
		self.client.connect()
		command = "8^" + db_name + "^" + table_name + "^" + self.build_fk_command(fk)
		# return self.client.send_message(command)
		code = self.client.send_message(command)
		if code[0] == '1':
			return 1, code.split('^')[1]
		else:
			return 0, "OK"

	def create_table(self, db_name, table_name):
		command = "3^" + db_name + "^" + table_name
		try:
			# print(command)
			self.client.connect()
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
			self.client.connect()
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			return 0, 'OK'
		except ConnectionError as ce:
			return 1, ce

	def drop_table(self, db_name, table_name):
		command = "4^" + db_name + "^" + table_name
		try:
			self.client.connect()
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
			# self.client.connect()
			code = self.client.send_message(command)
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

				code = self.client.send_message("0^OK")
				if code[0] == '1':
					return 1, code.split('^')[0]
			return 0, databases
		except ConnectionError as ce:
			return 1, ce

	def get_tables(self, db_name):
		command = "9^" + db_name
		try:
			self.client.connect()
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[0]
			tables = code.split('^')
			return 0, tables 
		except ConnectionError as ce:
			return 1, ce

	def get_table_columns(self, db_name, table_name):
		command = "6^" + db_name + "^" + table_name
		table_data = []
		try:
			self.client.connect()
			# code = self.client.send_message_pers(command)
			code = self.client.send_message(command)

			if code[0] == '1':
				return 1, code.split('^')[0]
			code = self.client.send_message("0")
			while code[0] != '0':
				# print(code)
				column = {}
				data = code.split('^')
				column["Name"] = data[0]
				column["Type"] = data[1]
				column["Primary Key"] = False if data[2] == "--" else True
				column["Not NULL"] = False if data[3] == "--" else True
				column["Default"] = "" if data[4] == "--" else data[4]
				column["Identity"] = "" if data[5] == "--" else data[5] + "," + data[6]
				column["Unique"] = False if data[7] == "--" else True
				column["Check"] = "" if data[8] == "--" else data[8]

				table_data.append(column)
				code = self.client.send_message("0^OK")
				if code[0] == '1':
					return 1, code.split('^')[0]
			return 0, table_data
		except ConnectionError as ce:
			return 1, ce

	def get_table_foreign_keys(self, db_name, table_name): 
		command = "10^" + db_name + "^" + table_name
		key_data = []
		try:
			self.client.connect()
			# code = self.client.send_message_pers(command)
			code = self.client.send_message(command)

			if code[0] == '1':
				return 1, code.split('^')[0]
			code = self.client.send_message("0")
			while code[0] != '0':
				# print(code)
				foreign_key = {}
				data = code.split('^')
				foreign_key["Constraint Name"] = data[0]
				foreign_key["Column"] = data[1]
				foreign_key["Foreign Table"] = data[2]
				foreign_key["Foreign Column"] = data[3]

				key_data.append(foreign_key)
				code = self.client.send_message("0^OK")
				if code[0] == '1':
					return 1, code.split('^')[0]
			return 0, key_data
		except ConnectionError as ce:
			return 1, ce

	def get_table_rows(self, db_name, table_name):
		command = "14^" + db_name + "^" + table_name
		rows = []
		try:
			self.client.connect()
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			code = self.client.send_message("1")
			while code[0] != '0':
				row = code.split('^')[1:] # elso 0^ arra van hogy vege van
				rows.append(row)
				code = self.client.send_message("1")
			return 0, rows
		except ConnectionError as ce:
			return 1, ce

		#
		# except ConnectionError as ce:
		# 	return 1, ce

	def get_table_indexes(self, db_name, table_name):
		command = "15^" + db_name + "^" + table_name
		indexes = []
		try:
			self.client.connect()
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			code = self.client.send_message("0")
			while code[0] != '0':
				data = code.split('^')
				index = {}
				index["name"] = data[0]
				index["columns"] = data[1:]
				indexes.append(index)
				code = self.client.send_message("0")
			return 0, indexes
		except ConnectionError as ce:
			return 1, ce


	def build_row_command(self, row):
		command = ""
		for item in row:
			item = item.replace('^', '')
			command += item + "^"
		return command[:-1]

	def delete_rows(self, db_name, table_name, rows):
		command = "12^" + db_name + "^" + table_name
		try:
			self.client.connect()
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			for row in rows:
				command = self.build_row_command(row)
				code = self.client.send_message(command)
				if code[0] == '1':
					return 1, code.split('^')[1]
			self.client.send_message("0")
			return 0, "OK"
		except ConnectionError as ce:
			return 1, ce

	def insert_rows(self, db_name, table_name, rows):
		command = "11^" + db_name + "^" + table_name
		try:
			self.client.connect()
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			for row in rows:
				command = self.build_row_command(row)
				code = self.client.send_message(command)
				if code[0] == '1':
					return 1, code.split('^')[1]
			self.client.send_message("0")
			return 0, "OK"
		except ConnectionError as ce:
			return 1, ce

	def create_index(self, db_name, table_name, name, columns):
		name = name.replace('^', '')
		command = "13^" + db_name + "^" + table_name + "^" + name + "^" + self.build_row_command(columns)
		try:
			self.client.connect()
			code = self.client.send_message(command)
			if code[0] == '1':
				return 1, code.split('^')[1]
			return 0, "OK"
		except ConnectionError as ce:
			return 1, ce
