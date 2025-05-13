from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font

class QueryResults(QTableWidget):
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

		self.column_names = []
		self.create_state = False

	def set_data(self, column_names, rows):
		self.column_names = column_names
		self.setRowCount(len(rows))
		self.setColumnCount(len(column_names))
		column_header_texts = []
		for i in range(len(column_names)):
			column_header_texts.append(column_names[i])
		self.setHorizontalHeaderLabels(column_header_texts)
		for row in range(self.rowCount()):
			for column in range(self.columnCount()):
				item = QTableWidgetItem(rows[row][column])
				# item.setFlags(Qt.ItemIsEnabled | Qt.ItemIsEditable)
				self.setItem(row, column, item)
