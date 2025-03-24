from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font

class EditForeignKeys(QTableWidget):
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
		self.set_demo_data()

	def select_row(self, row):
		print(row)
		self.clearSelection()
		self.selectRow(row)

	def keyPressEvent(self, event):
		if event.key() == Qt.Key_Delete:
			row = self.currentRow()
			if row != -1:
				self.removeRow(row)
		else:
			super().keyPressEvent(event)

	def set_demo_data(self):
		self.setRowCount(10)
		self.setColumnCount(4)
		self.setHorizontalHeaderLabels(["Constraint Name", "Column", "Foreign Table", "Foreign Column"])

		for row in range(self.rowCount()):
			for column in range(self.columnCount()):
				item = QTableWidgetItem(f"Row {row}, Column {column}")
				item.setFlags(Qt.ItemIsEnabled | Qt.ItemIsEditable)
				self.setItem(row, column, item)
