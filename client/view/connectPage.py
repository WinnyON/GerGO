from PyQt5.QtWidgets import *
from PyQt5.QtCore import Qt
from PyQt5.QtGui import QIntValidator
from fonts import h1_font
import os
import sys

parent_folder = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, parent_folder)

from client import Client

class ConnectPage(QWidget):
    def __init__(self, parent_stack_layout, conn_client, main_editor):
        super().__init__()
        self.client = conn_client
        self.main_editor = main_editor
        self.parent_stack_layout = parent_stack_layout
        self.layout = QGridLayout()
        self.form_layout = QFormLayout()
        self.welcome_layout = QHBoxLayout()
        self.submit_layout = QHBoxLayout()       
        self.connection_failed_layout = QHBoxLayout()
        self.layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.welcome_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.submit_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.connection_failed_layout.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.setLayout(self.layout)

        self.welcome_message = QLabel("WELCOME TO GERGO")
        self.welcome_message.setFont(h1_font)
        self.welcome_message.setAlignment(Qt.AlignmentFlag.AlignCenter)
        self.ip_label = QLabel("IP")
        self.port_label = QLabel("Port")
        self.input_ip = QLineEdit()
        self.input_port = QLineEdit()
        self.input_port.setValidator(QIntValidator())
        self.submit_button = QPushButton("Connect to server")
        self.submit_button.clicked.connect(self.go_to_main_editor_page)
        self.connection_failed_label = QLabel("Could not connect to the server. Try again!")
        self.connection_failed_label.setAlignment(Qt.AlignmentFlag.AlignCenter)

        self.welcome_layout.addWidget(self.welcome_message)
        self.layout.addLayout(self.welcome_layout, 0, 0, 1, 2)

        self.form_layout.addRow(self.ip_label, self.input_ip)
        self.form_layout.addRow(self.port_label, self.input_port)
        self.form_layout.setContentsMargins(20, 20, 20, 20)
        self.layout.addLayout(self.form_layout, 1, 0, 2, 2)

        self.submit_layout.addWidget(self.submit_button)
        self.submit_layout.setContentsMargins(100, 0, 100, 0)
        self.layout.addLayout(self.submit_layout, 3, 0, 1, 2)


        self.connection_failed_layout.addWidget(self.connection_failed_label)
        self.layout.addLayout(self.connection_failed_layout, 4, 0, 1, 2)
        self.connection_failed_label.hide()

    def failed_connection(self):
        self.connection_failed_label.show()

    def go_to_main_editor_page(self):
        self.client.setDestination(self.input_ip.text(), self.input_port.text())
        return_code = self.client.connect()
        if return_code == 0:
            self.parent_stack_layout.setCurrentIndex(1)
            self.main_editor.load_tree_data()
        else:
            self.failed_connection()
