from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt

class MenuBar(QMenuBar):
	def __init__(self, parent_widget):
		super().__init__(parent_widget)
		self.edit_menu = QMenu('Edit', parent_widget)
		self.addMenu(self.edit_menu)
		self.create_db_action = QAction('Create new database', parent_widget)
		self.create_table_action = QAction('Create new table', parent_widget)
		self.create_constraint_action = QAction('Add new constraint', parent_widget)

		self.edit_menu.addAction(self.create_db_action)
		self.edit_menu.addAction(self.create_table_action)
		self.edit_menu.addAction(self.create_constraint_action)