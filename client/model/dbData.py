class DbData():
	def __init__(self):
		self.dbs = []

	def add_empty_db(self, db_name):
		self.dbs.append({"name", db_name})

	def add_db(self, db):
		self.dbs.append(db)

	def add_table(self, db_name, table):
		for db in self.dbs:
			if db["name"] == db_name:
				db["tables"].append(table)
				return 1
		return 0

	def add_column(self, db_name, table_name, column):
		for db in self.dbs:
			if db["name"] == db_name:
				for table in db["tables"]:
					if table["name"] == table_name:
						table["columns"].append(column)
						return 1
		return 0


	def add_row(self, db_name, table_name, row):
		for db in self.dbs:
			if db["name"] == db_name:
				for table in db["tables"]:
					if table["name"] == table_name:
						table["rows"].append(row)
						return 1
		return 0

	def add_fk(self, db_name, table_name, fk):
		for db in self.dbs:
			if db["name"] == db_name:
				for table in db["tables"]:
					if table["name"] == table_name:
						table["FK"].append(fk)
						return 1
		return 0

	def get_database_names(self):
		db_names = []
		for db in self.dbs:
			db_names.append(db["name"])
		return db_names

	def get_table_names(self, db_name):
		for db in self.dbs:
			if db["name"] == db_name:
				table_names = []
				for table in db["tables"]:
					table_names.append(table["name"])
				return table_names
		return []

	def get_columns(self, db_name, table_name):
		for db in self.dbs:
			if db["name"] == db_name:
				for table in db["tables"]:
					if table["name"] == table_name:
						return table["columns"]
		return []

	def get_rows(self, db_name, table):
		for db in self.dbs:
			if db["name"] == db_name:
				for table in db["tables"]:
					if table["name"] == table_name:
						return table["rows"]
		return []

	def get_foreign_keys(self, db_name, table):
		for db in self.dbs:
			if db["name"] == db_name:
				for table in db["tables"]:
					if table["name"] == table_name:
						return table["FK"]
		return []