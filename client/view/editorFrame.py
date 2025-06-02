from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font
from editRows import EditRows
from editColumns import EditColumns
from constraintEditor import ConstraintEditor
from editForeignKeys import EditForeignKeys
from createIndex import CreateIndex
from view.queryResults import QueryResults
from view.queryEditor import QueryEditor


class EditorFrame(QWidget):
	def __init__(self, repository, main_editor_page):
		super().__init__()
		self.repository = repository
		self.main_editor_page = main_editor_page
		self.selected_db = None
		self.selected_table = None
		self.selected_editor_type = None
		self.layout = QGridLayout()
		self.header_layout = QHBoxLayout()
		self.editor_type_layout = QStackedLayout()
		self.setLayout(self.layout)

		self.columns_button = QPushButton('Columns')
		self.columns_button.clicked.connect(self.change_editor_to_columns)
		self.rows_button = QPushButton('Rows')
		self.rows_button.clicked.connect(self.change_editor_to_rows)
		self.foreign_key_button = QPushButton('Foreign keys')
		self.foreign_key_button.clicked.connect(self.change_editor_to_edit_fk)
		self.query_button = QPushButton('Create Query')
		self.query_button.clicked.connect(self.change_editor_to_query)
		self.apply_button = QPushButton('Apply changes')
		self.apply_button.clicked.connect(self.apply_changes_handler)
		self.add_entry_button = QPushButton('Add Entry')
		self.add_entry_button.clicked.connect(self.add_column_handler)



		self.edit_rows_widget = EditRows()
		self.edit_columns_widget = EditColumns()
		self.edit_constraints_widget = ConstraintEditor()
		self.edit_foreign_keys_widget = EditForeignKeys()
		self.create_index_widget = CreateIndex()
		self.query_widget = QueryEditor()
		self.query_results_widget = QueryResults()

		self.header_layout.addWidget(self.columns_button)
		self.header_layout.addWidget(self.rows_button)
		self.header_layout.addWidget(self.foreign_key_button)
		self.header_layout.addWidget(self.query_button)
		self.header_layout.addWidget(self.add_entry_button)
		self.header_layout.addWidget(self.apply_button)


		self.editor_type_layout.addWidget(self.edit_rows_widget)
		self.editor_type_layout.addWidget(self.edit_columns_widget)
		self.editor_type_layout.addWidget(self.edit_constraints_widget)
		self.editor_type_layout.addWidget(self.edit_foreign_keys_widget)
		self.editor_type_layout.addWidget(self.create_index_widget)
		self.editor_type_layout.addWidget(self.query_widget)
		self.editor_type_layout.addWidget(self.query_results_widget)

		self.editor_type_layout.setCurrentIndex(1)

		self.layout.addLayout(self.header_layout, 0, 0, 1, 3)
		self.layout.addLayout(self.editor_type_layout, 1, 0, 1, 6)


	def add_column_handler(self):
		if self.selected_editor_type == "columns":
			self.edit_columns_widget.add_row()
		elif self.selected_editor_type == "rows":
			self.edit_rows_widget.add_row()

	def change_editor_to_rows(self):
		code, data = self.repository.get_table_columns(self.selected_db, self.selected_table)
		if code == 1:
			self.show_message("ERROR", "Error loading columns")
			return
		headers = []
		pks = []
		types = []
		for column in data:
			headers.append(column["Name"])
			types.append(column["Type"])
			if column["Primary Key"]:
				pks.append(column["Name"])
		# TODO: add column names to the get_table_rows request
		code, row_data = self.repository.get_table_rows(self.selected_db, self.selected_table, headers)
		if code == 1:
			self.show_message("ERROR", "Error loading rows")
			return
		self.edit_rows_widget.set_data(headers, types, row_data, pks)
		# self.edit_rows_widget.set_data(headers, row_data, pks) #TODO: get rows from server
		self.editor_type_layout.setCurrentIndex(0)
		self.selected_editor_type = "rows"

	def change_editor_to_columns(self):	
		code, data = self.repository.get_table_columns(self.selected_db, self.selected_table)
		if code == 0:
			self.edit_columns_widget.set_table_data(data)
			self.editor_type_layout.setCurrentIndex(1)
			self.selected_editor_type = "columns"
		else:
			self.show_message("ERROR", "Could not load table data!\n" + data)

	def change_editor_to_create_index(self, db_name, table_name):
		code, data = self.repository.get_table_columns(db_name, table_name)
		if code == 1:
			self.show_message("ERROR", "Could not load table columns!\n" + data)
			return
		columns = []
		for column in data:
			# columns.append(column["Name"])
			#TODO: put it back
			if not column["Primary Key"]:
				columns.append(column["Name"])
		self.create_index_widget.set_column_data(columns)
		self.editor_type_layout.setCurrentIndex(4)
		self.selected_editor_type = "index"


	def change_editor_to_create_fk(self, db_name, table_name):
		code, data = self.repository.get_tables(db_name)
		if code == 0:
			table_details = []
			current_columns = []
			for table in data:
				code, columns = self.repository.get_table_columns(db_name, table)
				if code == 0:
					if table == table_name:
						current_columns = [column["Name"] for column in columns] 
					else:
						table_details.append({"name": table, "columns": columns})
				else:
					self.show_message("ERROR", "Table data could not be loaded!")
					return

			# print(current_columns)
			self.edit_constraints_widget.load_tree_data(table_details)
			self.edit_constraints_widget.load_table_columns(current_columns)

		self.editor_type_layout.setCurrentIndex(2)
		self.selected_editor_type = "create_fk"

	def change_editor_to_edit_fk(self):
		code, data = self.repository.get_table_foreign_keys(self.selected_db, self.selected_table)
		if code == 0:
			self.edit_foreign_keys_widget.set_table_data(data)
			self.editor_type_layout.setCurrentIndex(3)
			self.selected_editor_type = "edit_fk"
		else:
			self.show_message("ERROR", "Error loading foreign keys")

	def change_editor_to_query(self):
		code, data = self.repository.get_tables(self.selected_db)
		if code == 0:
			self.query_widget.set_table_names(data)
			self.editor_type_layout.setCurrentIndex(5)
			self.selected_editor_type = "query"
		else:
			self.show_message("ERROR", "Error loading tables")

	#TODO: set this when new table and db is created
	def set_selected_db(self, db):
		self.selected_db = db

	def set_selected_table(self, table):
		self.selected_table = table


	def apply_changes_handler(self):
		print(self.selected_editor_type)
		if self.selected_editor_type == "create_fk":
			data = self.edit_constraints_widget.get_data()
			# itt mar lehet elerheto a db, table nev
			fk = {"name": data[0], "tableName": data[2], "foreignColumn": data[3], "column": data[1]}
			code, msg = self.repository.add_foreign_key(self.selected_db, self.selected_table, fk)
			# print(code)
			if code == 1:
				self.show_message("ERROR", "Error while creating foreign key!\n" + msg)
			else:
				self.show_message("SUCCESS", "Foreign key created successfully")

		elif self.selected_editor_type == "columns":
			# data = self.edit_columns_widget.get_data()
			# code, msg = self.repository.add_columns(self.selected_db, self.selected_table, data)
			# if code == 0:
			# 	self.show_message("SUCCESS", "Columns edited successfully!")
			# else:
			# 	self.show_message("ERROR", "There was an error when editing the columns!\n" + msg)
			#new
			inserted_data = self.edit_columns_widget.get_modified_rows()
			deleted_data = self.edit_columns_widget.get_deleted_rows()

			if inserted_data:
				column_names = map(lambda x: x["Name"], inserted_data)
				code, msg = self.repository.delete_columns(self.selected_db, self.selected_table, column_names)
				if code == 1:
					self.show_message("ERROR", "There was an error when deleting the columns!\n" + msg)
					return
				code, msg = self.repository.add_columns(self.selected_db, self.selected_table, inserted_data)
				if code == 1:
					self.show_message("ERROR", "There was an error when inserting the columns!\n" + msg)
					return

			if deleted_data:
				code, msg = self.repository.delete_columns(self.selected_db, self.selected_table, deleted_data)
				if code == 1:
					self.show_message("ERROR", "There was an error when deleting the columns!\n" + msg)
					return


			# end
		elif self.selected_editor_type == "rows":
			column_names = self.edit_rows_widget.get_column_names()
			inserted_data = self.edit_rows_widget.get_modified_rows()
			deleted_data = self.edit_rows_widget.get_deleted_rows()
			print(deleted_data)
			if inserted_data:
				code, msg = self.repository.insert_rows(self.selected_db, self.selected_table, inserted_data, column_names)
				if code == 1:
					self.show_message("ERROR", "There was an error when inserting the rows!\n" + msg)
					return
			if deleted_data:
				code, msg = self.repository.delete_rows(self.selected_db, self.selected_table, deleted_data)
				if code == 1:
					self.show_message("ERROR", "There was an error when deleting the rows!\n" + msg)
					return
			self.show_message("SUCCESS", "Rows modified successfully!")
			#TODO: reload rows

		elif self.selected_editor_type == "index":
			name, columns = self.create_index_widget.get_data()
			code, msg = self.repository.create_index(self.selected_db, self.selected_table, name, columns)
			if code == 1:
				self.show_message("ERROR", "Error while creating index!\n" + msg)
				return
			self.show_message("SUCCESS", "Index created successfully!")
			self.main_editor_page.add_index_to_tree(self.selected_db, self.selected_table, {"name": name, "columns": columns})

		elif self.selected_editor_type == "query":
			code, data = self.query_widget.parse_command()
			if code == -1:
				self.show_message("ERROR", "Error parsing command: " + data)
				return
			if code == 1:
				code, data = self.repository.insert_rows(self.selected_db, data[0], data[2], data[1])
				if code == -1:
					self.show_message("ERROR", "Error inserting rows: " + data)
					return
				self.show_message("SUCCESS", "Rows inserted successfully!")
				return
			if code == 2:
				print(data)
				code, data = self.repository.delete_where_rows(self.selected_db, data[0], data[1])
				if code == 1:
					self.show_message("ERROR", "Error deleting rows: " + data)
					return
				self.show_message("SUCCESS", "Rows deleted successfully!")
				return
			print(data)
			code, data = self.repository.select_rows(self.selected_db, data[0], data[1], data[2], data[3], data[4])
			if code == 1:
				self.show_message("ERROR", "Error while selecting rows!\n" + data)
				return
			self.query_results_widget.set_data(data[1], data[2])
			self.editor_type_layout.setCurrentIndex(6)
			self.selected_editor_type = "query_results"
			print(data)

		# elif self.selected_editor_type == "edit_fk":
			# data = self.repository.get_table_foreign_keys(self.selected_db, self.selected_table)

	def show_message(self, title, message):
		msg_box = QMessageBox()
		msg_box.setWindowTitle(title)
		msg_box.setText(message)
		msg_box.setIcon(QMessageBox.Information)
		msg_box.setStandardButtons(QMessageBox.Ok)
		msg_box.exec_()

