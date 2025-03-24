from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font

class CreatePage(QWidget):
	def __init__(self, parent_stack_layout):
		super().__init__()
		self.parent_stack_layout = parent_stack_layout
		self.layout = QGridLayout()
		self.submit_layout = QHBoxLayout()
		self.title_layout = QHBoxLayout()

		self.title_label = QLabel("Enter the name of the entity")
		self.title_label.setFont(h2_font)
		self.title_label.setAlignment(Qt.AlignmentFlag.AlignCenter)
		self.name_text = QLineEdit()
		self.name_text.setAlignment(Qt.AlignmentFlag.AlignCenter)
		self.submit_button = QPushButton("Submit")

		self.title_layout.addWidget(self.title_label)
		self.submit_layout.addWidget(self.submit_button)
		self.layout.setAlignment(Qt.AlignmentFlag.AlignCenter)


		self.title_label.setContentsMargins(20, 0, 20, 10)
		self.name_text.setContentsMargins(0, 0, 0, 10)
		self.name_text.setMinimumSize(300, 50)
		self.name_text.setMaximumSize(400, 50)
		self.submit_button.setContentsMargins(200, 20, 200, 0)
		self.submit_button.setMinimumSize(100, 50)
		self.submit_button.setMaximumSize(200, 100)

		self.layout.addLayout(self.title_layout, 0, 0)
		self.layout.addWidget(self.name_text, 1, 0)
		self.layout.addLayout(self.submit_layout, 2, 0)

		self.setLayout(self.layout)

	def set_submit_action(self, type):
		if type == "db":
			self.submit_button.clicked.connect(switch_to_empty)
		else:
			self.submit_button.clicked.connect(switch_to_create)

	def switch_to_empty(self):
		self.parent_stack_layout.setCurrentIndex(0)
		#TODO: send data to server to create db

	def switch_to_create(self):
		self.parent_stack_layout.setCurrentIndex(1)
		#TODO: load empty column edit page