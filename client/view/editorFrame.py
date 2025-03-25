from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font
from editRows import EditRows
from editColumns import EditColumns
from constraintEditor import ConstraintEditor
from editForeignKeys import EditForeignKeys

class EditorFrame(QWidget):
	def __init__(self, repository):
		super().__init__()
		self.repository = repository
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
		self.apply_button = QPushButton('Apply changes')
		self.apply_button.clicked.connect(self.apply_changes_handler)

		self.edit_rows_widget = EditRows()
		self.edit_columns_widget = EditColumns()
		self.edit_constraints_widget = ConstraintEditor()
		self.edit_foreign_keys_widget = EditForeignKeys()

		self.header_layout.addWidget(self.columns_button)
		self.header_layout.addWidget(self.rows_button)
		self.header_layout.addWidget(self.foreign_key_button)
		self.header_layout.addWidget(self.apply_button)

		self.editor_type_layout.addWidget(self.edit_rows_widget)
		self.editor_type_layout.addWidget(self.edit_columns_widget)
		self.editor_type_layout.addWidget(self.edit_constraints_widget)
		self.editor_type_layout.addWidget(self.edit_foreign_keys_widget)

		self.editor_type_layout.setCurrentIndex(1)

		self.layout.addLayout(self.header_layout, 0, 0, 1, 3)
		self.layout.addLayout(self.editor_type_layout, 1, 0, 1, 6)

	def change_editor_to_rows(self):
		self.editor_type_layout.setCurrentIndex(0)
		self.selected_editor_type = "rows"

	def change_editor_to_columns(self):
		self.editor_type_layout.setCurrentIndex(1)
		self.selected_editor_type = "columns"

	def change_editor_to_create_fk(self, db_name, table_name):
		self.editor_type_layout.setCurrentIndex(2)
		self.selected_editor_type = "create_fk"

	def change_editor_to_edit_fk(self):
		self.editor_type_layout.setCurrentIndex(3)
		self.selected_editor_type = "edit_fk"


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
			code = self.repository.add_foreign_key(self.selected_db, self.selected_table, fk)
			print(code)

		elif self.selected_editor_type == "columns":
			data = self.edit_columns_widget.get_data()
			print(data)
			code = self.repository.add_columns(self.selected_db, self.selected_table, data)
			print(code)






