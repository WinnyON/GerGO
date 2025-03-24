from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font

class DbTree(QTreeWidget):
	def __init__(self, parent_widget, editor_frame):
		super().__init__()
		self.parent_widget = parent_widget
		self.editor_frame = editor_frame
		self.current_table = None
		self.current_db = None
		self.setHeaderHidden(False)
		self.setHeaderLabels(['Databases'])
		self.header().setFont(h2_font)
		self.setStyleSheet("""
			QTreeWidget::item:selected {
			                background-color: #87CEEB;
			                color: #FFFFFF;
			            }
			QTreeWidget {
				margin: 5px;
			}
			QTreeWidget::item {
				margin-right: 40px;
			}
        	""")


		self.edit_menu = QMenu()
		self.create_db_action = QAction('Create new database')
		self.create_db_action.triggered.connect(self.create_db_action_handler)
		self.create_table_action = QAction('Create new table')
		self.create_table_action.triggered.connect(self.create_table_action_handler)
		self.create_constraint_action = QAction('Add new foreign key')
		self.create_constraint_action.triggered.connect(self.create_fk_action_handler)

		self.edit_menu.addAction(self.create_db_action)
		self.edit_menu.addAction(self.create_table_action)
		self.edit_menu.addAction(self.create_constraint_action)

		self.setContextMenuPolicy(Qt.CustomContextMenu)
		self.customContextMenuRequested.connect(self.show_context_menu)

		self.itemSelectionChanged.connect(self.handle_item_selection)



	def get_item_level(self, item):
		level = 0
		while item.parent() is not None:
			level += 1
			item = item.parent()
		return level

	def show_context_menu(self, position):
		item = self.itemAt(position)
		if item:
			if self.get_item_level(item) == 1:
				self.current_table  = item.text(0)
				self.current_db = item.parent().text(0)
				self.create_constraint_action.setEnabled(True)
			else:
				if self.get_item_level(item) == 0:
					self.current_db = item.text(0)
				self.create_constraint_action.setEnabled(False)
			self.edit_menu.exec_(self.viewport().mapToGlobal(position))	

	def handle_item_selection(self):
		item = self.currentItem()
		if item and self.get_item_level(item) == 1:
			self.parent_widget.show_selected_state(item.parent().text(0), item.text(0))
			self.editor_frame.change_editor_to_rows()
			
			# self.editor_frame.set_selected_db(item.parent)
			# self.editor_frame.set_selected_table(self.current_table)
			# print(item.text(0))


	def create_db_action_handler(self):
		self.parent_widget.show_create_state("db", None)

	def create_table_action_handler(self):
		self.parent_widget.show_create_state("table", self.current_db)

	def create_fk_action_handler(self):
		if self.current_table:
			self.editor_frame.change_editor_to_create_fk(self.current_db, self.current_table)


	def create_demo_tree(self):
		rootItem1 = QTreeWidgetItem(['Root Item 1'])
		rootItem2 = QTreeWidgetItem(['Root Item 2'])
		self.addTopLevelItem(rootItem1)
		self.addTopLevelItem(rootItem2)
		childItem1 = QTreeWidgetItem(['Child Item 1'])
		childItem2 = QTreeWidgetItem(['Child Item 2'])
		rootItem1.addChild(childItem1)
		rootItem1.addChild(childItem2)
		# self.setCurrentItem(childItem1)