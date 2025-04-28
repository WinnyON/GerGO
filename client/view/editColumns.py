from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font


class EditColumns(QTableWidget):
	def __init__(self):
		super().__init__() 
		self.setEditTriggers(QAbstractItemView.AllEditTriggers)
		self.setAlternatingRowColors(False)
		self.horizontalHeader().setSectionResizeMode(QHeaderView.Stretch)
		self.verticalHeader().setSectionResizeMode(QHeaderView.Stretch)
		self.setShowGrid(True)
		self.setGridStyle(Qt.SolidLine)
		self.setHorizontalScrollBarPolicy(Qt.ScrollBarAlwaysOn)
		self.setVerticalScrollBarPolicy(Qt.ScrollBarAlwaysOn)


		self.horizontalHeader().setStyleSheet("::section { background-color: #87CEEB; color: #FFFFFF }")
		self.horizontalHeader().setDefaultAlignment(Qt.AlignHCenter | Qt.AlignVCenter) 
		self.verticalHeader().setStyleSheet("::section { background-color: #E5E4E2; border: 1px solid #D3D3D3; border-left: none;}")
		self.verticalHeader().setDefaultAlignment(Qt.AlignHCenter | Qt.AlignVCenter)

		self.verticalHeader().sectionClicked.connect(self.select_row)
		# self.set_demo_data()

		self.modified_rows = {}
		self.deleted_rows = []
		self.itemChanged.connect(self.on_item_changed)
		self.create_state = False

	def select_row(self, row):
		print(row)
		self.clearSelection()
		self.selectRow(row)

	def keyPressEvent(self, event):
		if event.key() == Qt.Key_Delete:
			row = self.currentRow()
			if row != -1:
				self.deleted_rows.append(self.item(row, 0).text())
				self.removeRow(row)
		else:
			super().keyPressEvent(event)

	def set_table_data(self, table_data):
		self.create_state = True
		header_labels = ["Name", "Type", "Check", "Default", "Identity", "Primary Key", "Not NULL", "Unique"]
		self.setRowCount(len(table_data))
		self.setColumnCount(8)
		self.setHorizontalHeaderLabels(header_labels)

		for row in range(self.rowCount()):
			col = table_data[row]
			for column in range(self.columnCount()):
				if column > 4:
					container = QWidget()
					container_layout = QHBoxLayout()
					container_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
					container.setLayout(container_layout)

					checkbox = QCheckBox()
					checkbox.setChecked(col[header_labels[column]])
					checkbox.setMinimumSize(10,20)
					container_layout.addWidget(checkbox)
					
					self.setCellWidget(row, column, container)

				else:
					item = QTableWidgetItem(col[header_labels[column]])
					item.setFlags(Qt.ItemIsEnabled | Qt.ItemIsEditable)
					item.setTextAlignment(Qt.AlignCenter)
					self.setItem(row, column, item)

			self.setRowHeight(row, 50)


		self.setStyleSheet("""
			    QTableWidget::item {
			        padding: 5px;
			    }
			""")


		header = self.horizontalHeader()
		header.setMinimumSectionSize(50)
		header.setMaximumSectionSize(500)
		for column in range(self.columnCount()):
			if column > 4:
				header.setSectionResizeMode(column, QHeaderView.ResizeToContents)
			else:
				header.setSectionResizeMode(column, QHeaderView.Interactive)
				header.resizeSection(column, 150)

		header = self.verticalHeader()
		for row in range(self.rowCount()):
			header.setSectionResizeMode(row, QHeaderView.ResizeToContents)

		self.create_state = False

	# def set_demo_data(self):
	# 	self.setRowCount(10)
	# 	self.setColumnCount(8)
	# 	self.setHorizontalHeaderLabels(["Name", "Type", "Check", "Default", "Primary Key", "Identity", "Not NULL", "Unique"])
	#
	# 	for row in range(self.rowCount()):
	# 		for column in range(self.columnCount()):
	# 			if column > 4:
	# 				container = QWidget()
	# 				container_layout = QHBoxLayout()
	# 				container_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
	# 				container.setLayout(container_layout)
	#
	# 				checkbox = QCheckBox()
	# 				checkbox.setMinimumSize(10,20)
	# 				container_layout.addWidget(checkbox)
	#
	# 				self.setCellWidget(row, column, container)
	#
	# 			else:
	# 				item = QTableWidgetItem(f"Row {row}, Column {column}")
	# 				item.setFlags(Qt.ItemIsEnabled | Qt.ItemIsEditable)
	# 				item.setTextAlignment(Qt.AlignCenter)
	# 				self.setItem(row, column, item)
	#
	#
	# 	self.setStyleSheet("""
	# 		    QTableWidget::item {
	# 		        padding: 5px;
	# 		    }
	# 		""")
	#
	#
	# 	header = self.horizontalHeader()
	# 	header.setMinimumSectionSize(50)
	# 	header.setMaximumSectionSize(500)
	# 	for column in range(self.columnCount()):
	# 		if column > 3:
	# 			header.setSectionResizeMode(column, QHeaderView.ResizeToContents)
	# 		else:
	# 			header.setSectionResizeMode(column, QHeaderView.Interactive)
	# 			header.resizeSection(column, 150)
	#
	# 	header = self.verticalHeader()
	# 	for row in range(self.rowCount()):
	# 		header.setSectionResizeMode(row, QHeaderView.ResizeToContents)

	def get_data(self):
		data = []
		for row in range(self.rowCount()):
			column_data = {}
			for column in range(self.columnCount()):
				print(row, column)
				if column < 5:
					self.item(row, column).setText(self.item(row, column).text().replace('^', ''))
					column_data[self.horizontalHeaderItem(column).text()] = self.item(row, column).text()
				else:
					column_data[self.horizontalHeaderItem(column).text()] = self.cellWidget(row, column).findChild(QCheckBox).isChecked()
			data.append(column_data)
		return data

	def add_row(self):
		self.create_state = True
		self.setRowCount(self.rowCount() + 1)
		row = self.rowCount() - 1
		for column in range(self.columnCount()):
			if column > 4:
				container = QWidget()
				container.setMaximumHeight(50)
				container_layout = QHBoxLayout()
				container_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
				container.setLayout(container_layout)

				checkbox = QCheckBox()
				checkbox.setMinimumSize(10,20)
				container_layout.addWidget(checkbox)
				
				self.setCellWidget(row, column, container)
			else:
				item = QTableWidgetItem("")
				item.setFlags(Qt.ItemIsEnabled | Qt.ItemIsEditable)
				item.setTextAlignment(Qt.AlignCenter)
				self.setItem(row, column, item)

		self.setRowHeight(row, 50)
		self.create_state = False


	def get_modified_rows(self):
		data = self.modified_rows.values()
		self.modified_rows = {}
		return data

	def get_deleted_rows(self):
		removed = self.deleted_rows
		self.deleted_rows = []
		return removed

	def on_item_changed(self, item):
		if self.create_state:
			return
		row = item.row()
		column_data = {}
		for column in range(self.columnCount()):
			if column < 5:
				self.item(row, column).setText(self.item(row, column).text().replace('^', ''))
				column_data[self.horizontalHeaderItem(column).text()] = self.item(row, column).text()
			else:
				column_data[self.horizontalHeaderItem(column).text()] = self.cellWidget(row, column).findChild(
					QCheckBox).isChecked()
		print(column_data)
		self.modified_rows[row] = column_data

	#TODO: save deleted and modified rows separately and apply changes differently for each