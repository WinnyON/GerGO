from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from fonts import h2_font

class CreatePage(QWidget):
	def __init__(self, parent_stack_layout, repository, db_tree):
		super().__init__()
		self.repository = repository
		self.db_tree = db_tree
		self.db_name = None
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

	def set_submit_action(self, create_type, db_name):
		self.db_name = db_name
		if create_type == "db":
			try:
				self.submit_button.clicked.disconnect(self.switch_to_empty)
			except Exception:
				pass
			try:
				self.submit_button.clicked.disconnect(self.switch_to_create)
			except Exception:
				pass
			self.submit_button.clicked.connect(self.switch_to_empty)
		else:
			try:
				self.submit_button.clicked.disconnect(self.switch_to_empty)
			except Exception:
				pass
			try:
				self.submit_button.clicked.disconnect(self.switch_to_create)
			except Exception:
				pass
			self.submit_button.clicked.connect(self.switch_to_create)

	def switch_to_empty(self):
		print(self.name_text.text())
		code, msg = self.repository.create_db(self.name_text.text())
		# print(code)
			# set selected db to current
			# set selected db to none
		#create popup for success or error

		self.parent_stack_layout.setCurrentIndex(0)
		if code == 0:
			self.db_tree.add_db(self.name_text.text())
			self.db_tree.current_table = None
			self.db_tree.current_db = self.name_text.text()
			self.show_message("SUCCESS", "Database created successfully")
		else:
			self.show_message("Error creating the database", msg)

		# self.name_text.setText("")

	def switch_to_create(self):
		print(self.name_text.text())
		print(self.db_name)
		code, msg = self.repository.create_table(self.db_name, self.name_text.text())
		print(code)
		self.parent_stack_layout.setCurrentIndex(1)
		if code == 0:
			self.db_tree.add_table(self.db_name, self.name_text.text())
			self.show_message("SUCCESS", "Table created successfully")
		else:
			self.show_message("Error creating the table", msg)
		# self.name_text.clear()

		#TODO: load empty column edit page

	def show_message(self, title, message):
		msg_box = QMessageBox()
		msg_box.setWindowTitle(title)
		msg_box.setText(message)
		msg_box.setIcon(QMessageBox.Information)
		msg_box.setStandardButtons(QMessageBox.Ok)
		msg_box.exec_()