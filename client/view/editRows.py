from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font

class EditRows(QTableWidget):
	def __init__(self):
		super().__init__() 
		self.setEditTriggers(QAbstractItemView.AllEditTriggers)
		self.setAlternatingRowColors(True)
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

		self.primary_keys = []
		self.column_names = []
		self.create_state = False


	def select_row(self, row):
		# print(row)
		self.clearSelection()
		self.selectRow(row)

	def keyPressEvent(self, event):
		if event.key() == Qt.Key_Delete:
			row = self.currentRow()
			if row != -1:
				row_data = []
				for column in range(self.columnCount()):
					if self.horizontalHeaderItem(column).text().split('(')[0] in self.primary_keys:
						row_data.append(self.item(row, column).text())
				self.deleted_rows.append(row_data)
				self.removeRow(row)
				print(self.deleted_rows)

		else:
			super().keyPressEvent(event)

	def set_data(self, column_names, column_types, rows, pk):
	# def set_data(self, column_names, rows, pk):
		self.column_names = column_names
		self.create_state = True
		self.primary_keys = pk
		self.setRowCount(len(rows))
		self.setColumnCount(len(column_names))
		column_header_texts = []
		for i in range(len(column_names)):
			column_header_texts.append(column_names[i] + "(" + column_types[i] + ")")
		self.setHorizontalHeaderLabels(column_header_texts)
		# print(rows)
		# print(column_names)
		for row in range(self.rowCount()):
			for column in range(self.columnCount()):
				item = QTableWidgetItem(rows[row][column])
				# print(rows[row][column])
				item.setFlags(Qt.ItemIsEnabled | Qt.ItemIsEditable)
				self.setItem(row, column, item)

		self.create_state = False

	# def get_data(self):
	# 	data = []
	# 	for row in range(self.rowCount()):
	# 		row_data = {}
	# 		for column in range(self.columnCount()):
	# 			row_data[self.horizontalHeaderItem(column).text()] = self.item(row, column).text()
	# 		data.append(row_data)
	# 	return data

	def get_modified_rows(self):
		# data = []
		# for row in self.modified_rows.keys():
			# row_data = []
			# for column in range(self.columnCount()):
			# 	row_data.append(self.item(row, column).text())
			# data.append(row_data)
		data = self.modified_rows.values()
		self.modified_rows = {}
		return list(data)

	def get_deleted_rows(self):
		removed = self.deleted_rows
		self.deleted_rows = []
		return removed

	def add_row(self):
		self.create_state = True
		self.setRowCount(self.rowCount() + 1)
		row = self.rowCount() - 1
		# print("HERE")
		for column in range(self.columnCount()):
			item = QTableWidgetItem("")
			item.setFlags(Qt.ItemIsEnabled | Qt.ItemIsEditable)

			self.setItem(row, column, item)

		self.create_state = False


	def on_item_changed(self, item):
		if self.create_state:
			return
		row = item.row()
		row_data = []
		for column in range(self.columnCount()):
			row_data.append(self.item(row, column).text())
		self.modified_rows[row] = row_data

	def get_column_names(self):
		return self.column_names