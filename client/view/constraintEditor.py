from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt

class ConstraintEditor(QWidget):
	def __init__(self):
		super().__init__()
		self.selected_column = None
		self.selected_option = None
		self.selected_table = None
		self.layout = QGridLayout()
		self.form_layout = QFormLayout()
		self.table_tree = QTreeWidget()
		self.setLayout(self.layout)

		self.form_layout.setContentsMargins(200, 100, 200, 10)

		self.table_tree = QTreeWidget()
		self.table_tree.setHeaderHidden(True)
		self.table_tree.setStyleSheet("""
			QTreeWidget::item:selected {
			                background-color: #87CEEB;
			                color: #FFFFFF;
			            }
			QTreeWidget {
				margin-right: 200px;
				margin-left: 200px;
				margin-bottom: 200px;
			}
			QTreeWidget::item {
				margin-right: 40px;
			}
			""")

		self.table_tree.itemSelectionChanged.connect(self.handle_item_selection)

		self.constraint_name_label = QLabel("Foreign key name")
		self.constraint_name_input = QLineEdit()
		# self.constraint_name_label.setStyleSheet("margin-top: 50")
		# self.constraint_name_input.setStyleSheet("margin-top: 50")
		self.current_table_label = QLabel("Current column")
		self.combo_box= QComboBox()
		self.combo_box.addItems(["Option 1", "Option 2", "Option 3"])
		self.combo_box.currentIndexChanged.connect(self.handle_combo_box_change)

		self.form_layout.addRow(self.constraint_name_label, self.constraint_name_input)
		self.form_layout.addRow(self.current_table_label, self.combo_box)

		self.create_demo_tree()

		self.layout.addLayout(self.form_layout, 0, 0, 2, 2)
		self.layout.addWidget(self.table_tree, 2, 0, 2, 1)

	def create_demo_tree(self):
		rootItem1 = QTreeWidgetItem(['Table 1'])
		rootItem2 = QTreeWidgetItem(['Table 2'])

		rootItem1.setFlags(rootItem1.flags() & ~Qt.ItemIsSelectable)
		rootItem2.setFlags(rootItem2.flags() & ~Qt.ItemIsSelectable)

		self.table_tree.addTopLevelItem(rootItem1)
		self.table_tree.addTopLevelItem(rootItem2)

		childItem1 = QTreeWidgetItem(['Column 1'])
		rootItem1.addChild(childItem1)

	def handle_combo_box_change(self, index):
		# Get the selected option
		self.selected_option = self.combo_box.itemText(index)
		print(f"Selected Option: {self.selected_option}")

	def get_item_level(self, item):
		level = 0
		while item.parent() is not None:
			level += 1
			item = item.parent()
		return level

	def handle_item_selection(self):
		item = self.table_tree.currentItem()
		if item and self.get_item_level(item) == 1:
			self.selected_column = item.text(0)
			self.selected_table = item.parent.text(0)


	def get_data(self):
		constraint_name = self.constraint_name_input.text()
		current_column = self.selected_option
		foreign_column = self.selected_column
		foreign_table = self.selected_table
		return [constraint_name, current_column, foreign_table, foreign_column]