from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font
from editRows import EditRows

class EditorFrame(QWidget):
	def __init__(self):
		super().__init__()
		self.layout = QGridLayout()
		self.header_layout = QHBoxLayout()
		self.editor_type_layout = QStackedLayout()
		self.setLayout(self.layout)

		self.columns_button = QPushButton('Columns')
		self.rows_button = QPushButton('Rows')
		self.apply_button = QPushButton('Apply changes')

		self.edit_rows_widget = EditRows()

		self.header_layout.addWidget(self.columns_button)
		self.header_layout.addWidget(self.rows_button)
		self.header_layout.addWidget(self.apply_button)

		self.editor_type_layout.addWidget(self.edit_rows_widget)

		self.layout.addLayout(self.header_layout, 0, 0, 1, 3)
		self.layout.addLayout(self.editor_type_layout, 1, 0, 1, 6)



